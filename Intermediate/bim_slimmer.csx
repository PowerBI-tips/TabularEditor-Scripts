//
// Title: BIM Slimmer - strip metadata bloat to reduce file size
//
// Author: Alexis Olson using GPT-5 Thinking and Claude Opus 4.1
//
// Description:
//   Opens a Tabular model .bim file, removes UI/engine bloat while preserving model semantics,
//   and saves a sibling .slim JSON. Preserves structural containers (model, tables, columns,
//   measures, partitions, relationships, etc.), removes empty/unused values, and supports
//   optional switches for display/query group metadata. Shows a summary with items removed and
//   size savings.
//
// How to use:
//   - Use in Tabular Editor (2 or 3) Advanced Scripting.
//   - (Optional) Customize the configuration options at the top of the script.
//   - When prompted, select a <ModelName>.bim file.
//   - The script writes <ModelName>.slim and displays a summary.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ==================== CONFIGURATION ====================
// Use defaults specified or customize as needed

// Core metadata removal (recommended: ON)
bool REMOVE_Annotations     = true;   // annotations, changedProperties, extendedProperties  
bool REMOVE_Lineage         = true;   // lineageTag, sourceLineageTag
bool REMOVE_LanguageData    = true;   // cultures, translations, synonyms, linguisticMetadata

// Value-based cleanup (recommended: ON)
bool REMOVE_DefaultValues   = true;   // dataCategory:Uncategorized, summarizeBy:none
bool REMOVE_RedundantNames  = true;   // sourceColumn==name, displayName==name  
bool REMOVE_EmptyContainers = true;   // empty {} and [] (preserves structural containers)

// Presentation properties (optional)
bool REMOVE_SummarizeBy     = true;  // summarizeBy (all values, not just none)
bool REMOVE_DisplayProps    = true;  // isHidden, displayFolder
bool REMOVE_QueryGroups     = false; // queryGroup, queryGroups, folder
bool REMOVE_FormatString    = true;  // formatString literal only (NEVER formatStringDefinition)

// Additional metadata (recommended: ON)
bool REMOVE_ExtraMetadata   = true;  // sourceProviderType, isNameInferred, isDataTypeInferred

// ==================== OUTPUT FORMAT =====================
// Human-friendly indented JSON (false) or compacted (true)
bool MINIFY_OUTPUT = true;

// ==================== MAIN EXECUTION ====================
try 
{
    // Select file
    string inputPath;
    using (var dialog = new OpenFileDialog {
        Title = "Select BIM file to slim",
        Filter = "Tabular Model (*.bim)|*.bim|All files (*.*)|*.*",
        RestoreDirectory = true
    }) {
        if (dialog.ShowDialog() != DialogResult.OK) return;
        inputPath = dialog.FileName;
    }
    
    // Generate output path
    var outputPath = Path.ChangeExtension(inputPath, ".slim");
    var originalSize = new FileInfo(inputPath).Length;
    
    // Parse JSON
    var root = JToken.Parse(File.ReadAllText(inputPath));
    
    // Build removal rules
    var dropKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    if (REMOVE_Annotations) {
        dropKeys.UnionWith(new[] { "annotations", "changedProperties", "extendedProperties" });
    }
    if (REMOVE_Lineage) {
        dropKeys.UnionWith(new[] { "lineageTag", "sourceLineageTag" });
    }
    if (REMOVE_LanguageData) {
        dropKeys.UnionWith(new[] { "cultures", "translations", "synonyms", "linguisticMetadata" });
    }
    if (REMOVE_SummarizeBy) {
        dropKeys.Add("summarizeBy");
    }
    if (REMOVE_DisplayProps) {
        dropKeys.UnionWith(new[] { "isHidden", "displayFolder" });
    }
    if (REMOVE_QueryGroups) {
        dropKeys.UnionWith(new[] { "queryGroup", "queryGroups", "folder" });
    }
    if (REMOVE_ExtraMetadata) {
        dropKeys.UnionWith(new[] { "sourceProviderType", "isNameInferred", "isDataTypeInferred" });
    }
    
    // Structural containers - never remove even if empty (preserves model schema)
    var preserve = new HashSet<string>(new[] {
        "model", "tables", "columns", "measures", "relationships", "partitions",
        "roles", "hierarchies", "levels", "dataSources", "perspectives", "expressions"
    }, StringComparer.OrdinalIgnoreCase);
    
    // Track removals
    var stats = new Dictionary<string, int>();
    Action<string> Track = delegate(string key) { 
        stats[key] = stats.ContainsKey(key) ? stats[key] + 1 : 1; 
    };
    
    // Helpers
    Func<string, string, bool> Eq = delegate(string a, string b) { 
        return string.Equals(
            a != null ? a.Trim() : null,
            b != null ? b.Trim() : null,
            StringComparison.OrdinalIgnoreCase
        );
    };
    
    Func<JToken, bool> IsEmpty = delegate(JToken t) { 
        return t == null || t.Type == JTokenType.Null || 
               (t is JContainer && !((JContainer)t).HasValues) ||
               (t.Type == JTokenType.String && string.IsNullOrWhiteSpace((string)t)); 
    };
    
    // Recursive cleaner
    Action<JToken> Clean = null;
    Clean = delegate(JToken token) {
        if (token == null) return;
        
        if (token.Type == JTokenType.Object) {
            var obj = (JObject)token;
            
            // Recurse first (depth-first)
            foreach (var prop in obj.Properties().ToList()) Clean(prop.Value);
            
            var toRemove = new List<JProperty>();
            
            foreach (var prop in obj.Properties()) {
                // Name-based removals
                if (dropKeys.Contains(prop.Name)) {
                    toRemove.Add(prop);
                    Track(prop.Name);
                    continue;
                }
                
                // formatString special handling (protect formatStringDefinition)
                if (REMOVE_FormatString && Eq(prop.Name, "formatString")) {
                    toRemove.Add(prop);
                    Track("formatString");
                    continue;
                }
                
                // Empty container removal (with structural preservation)
                if (REMOVE_EmptyContainers && IsEmpty(prop.Value) && !preserve.Contains(prop.Name)) {
                    toRemove.Add(prop);
                    Track("empty");
                    continue;
                }
            }
            
            // Value-based removals (checked after structure scan)
            if (REMOVE_DefaultValues) {
                var dc = obj.Property("dataCategory", StringComparison.OrdinalIgnoreCase);
                if (dc != null && dc.Value is JValue && Eq((string)dc.Value, "Uncategorized")) {
                    toRemove.Add(dc);
                    Track("dataCategory=default");
                }
                
                var sb = obj.Property("summarizeBy", StringComparison.OrdinalIgnoreCase);
                if (sb != null && sb.Value is JValue && Eq((string)sb.Value, "none")) {
                    toRemove.Add(sb);
                    Track("summarizeBy=none");
                }
            }
            
            if (REMOVE_RedundantNames) {
                var name = obj.Property("name", StringComparison.OrdinalIgnoreCase);
                if (name != null && name.Value is JValue) {
                    var nameStr = (string)name.Value;
                    var nameBracketed = nameStr != null ? string.Format("[{0}]", nameStr) : null;

                    var src = obj.Property("sourceColumn", StringComparison.OrdinalIgnoreCase);
                    if (
                        src != null &&
                        src.Value is JValue &&
                        (
                            Eq((string)src.Value, nameStr) ||
                            Eq((string)src.Value, nameBracketed)
                        )
                    ) {
                        toRemove.Add(src);
                        Track("sourceColumn=name");
                    }

                    var disp = obj.Property("displayName", StringComparison.OrdinalIgnoreCase);  
                    if (
                        disp != null &&
                        disp.Value is JValue &&
                        (
                            Eq((string)disp.Value, nameStr) ||
                            Eq((string)disp.Value, nameBracketed)
                        )
                    ) {
                        toRemove.Add(disp);
                        Track("displayName=name");
                    }
                }
            }
            
            // Apply all removals
            foreach (var prop in toRemove.Distinct()) prop.Remove();
        }
        else if (token.Type == JTokenType.Array) {
            var arr = (JArray)token;
            foreach (var item in arr.ToList()) {
                Clean(item);
                if (REMOVE_EmptyContainers && IsEmpty(item)) {
                    item.Remove();
                    Track("empty");
                }
            }
        }
    };
    
    // Execute cleaning
    Clean(root);
    
    // Save result
    var formatting = MINIFY_OUTPUT ? Formatting.None : Formatting.Indented;
    File.WriteAllText(outputPath, root.ToString(formatting));
    
    // Report results
    var newSize = new FileInfo(outputPath).Length;
    var reduction = (1.0 - (double)newSize / originalSize) * 100;
    var summary = 
        "BIM Slimmer Results\n" +
        "==================\n" +
        string.Format("Input:  {0} ({1:N1} KB)\n", Path.GetFileName(inputPath), originalSize / 1024.0) +
        string.Format("Output: {0} ({1:N1} KB)\n", Path.GetFileName(outputPath), newSize / 1024.0) +
        string.Format("Saved:  {0:F1}%\n\n", reduction) +
        string.Format("Removed: {0:N0} items\n", stats.Values.Sum()) +
        string.Join(
            "\n",
            stats.OrderBy(k => k.Key)
                .Select(k => string.Format("  • {0}: {1:N0}", k.Key, k.Value))
                .ToArray()
        );
    
    Info(summary);
}
catch (Exception ex) 
{
    Error(string.Format("Processing failed: {0}", ex.Message));
}
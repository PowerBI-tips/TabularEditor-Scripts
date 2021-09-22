/*
 * Title: Dynamic measure selector
 * 
 * Author: Daniel Otykier, twitter.com/DOtykier
 * 
 * Use this script to auto-generate a disconnected measure selector table
 * along with a single SWITCH measure, for a selection of measures.
 * More info: https://tabulareditor.com/2020/08/24/Generating-a-dynamic-measure-selector.html
 */

// (1) Name of disconnected selector table:
var selectorTableName = "Measure Selector";

// (2) Name of column on selector table:
var selectorTableColumnName = "Measure";

// (3) Name of dynamic switch measure:
var dynamicMeasureName = "Dynamic Measure";

// (4) Name of dynamic switch measure's parent table:
var dynamicMeasureTableName = "Measure Selector";

// (5) Fallback DAX expression:
var fallbackDax = "BLANK()";

// ----- Do not modify script below this line -----

if(Selected.Measures.Count == 0) {
    Error("Select one or more measures");
    return;
}

// Get or create selector table:
CalculatedTable selectorTable;
if(!Model.Tables.Contains(selectorTableName)) Model.AddCalculatedTable(selectorTableName);
selectorTable = Model.Tables[selectorTableName] as CalculatedTable;

// Get or create dynamic measure:
Measure dynamicMeasure;
if(!Model.Tables[dynamicMeasureTableName].Measures.Contains(dynamicMeasureName))
    Model.Tables[dynamicMeasureTableName].AddMeasure(dynamicMeasureName);
dynamicMeasure = Model.Tables[dynamicMeasureTableName].Measures[dynamicMeasureName];

// Generate DAX for disconnected table:
// SELECTCOLUMNS({"Measure 1", "Measure 2", ...}, "Measure", [Value])
var selectorTableDax = "SELECTCOLUMNS(\n    {\n        " +
    string.Join(",\n        ", Selected.Measures.Select(m => "\"" + m.Name + "\"").ToArray()) +
    "\n    },\n    \"" + selectorTableColumnName + "\", [Value]\n)";

// Generate DAX for dynamic metric:
// VAR _s = SELECTEDVALUE('Metric Selection'[Value]) RETURN SWITCH(_s, ...)
var dynamicMeasureDax = 
    "VAR _s =\n    SELECTEDVALUE('" + selectorTableName + "'[" + selectorTableColumnName + "])\n" +
    "RETURN\n    SWITCH(\n        _s,\n        " +
    string.Join(",\n        ", Selected.Measures.Select(m => "\"" + m.Name + "\", " + m.DaxObjectFullName).ToArray()) +
    ",\n        " + fallbackDax + "\n    )";

// Assign DAX expressions:
selectorTable.Expression = selectorTableDax;
dynamicMeasure.Expression = dynamicMeasureDax;
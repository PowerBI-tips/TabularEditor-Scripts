/*
 * Find and Replace text in Calculation Item expressions
 *
 * Author: Dan Meissner
 *
 * This script allows you to select a Calculation Group and one or more Calculation Items,
 * then perform a case-insensitive find and case-sensitive replace operation on the
 * expressions of the selected items.
 */

using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

// ===== CONFIGURATION =====
bool SHOW_PREVIEW_CONFIRMATION = false;  // Set to false to skip preview and apply changes immediately

// ===== VALIDATION =====
var calcGroups = Model.Tables.Where(t => t is CalculationGroupTable).Cast<CalculationGroupTable>().ToList();

if (calcGroups.Count == 0)
{
    Error("No calculation groups found in the model.");
    return;
}

// Get the Tabular Editor window as the parent
var teWindow = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
var ownerForm = System.Windows.Forms.Control.FromHandle(teWindow) as Form;

// ===== STEP 1: SELECT CALCULATION GROUP =====
CalculationGroupTable selectedCalcGroup = null;

if (calcGroups.Count == 1)
{
    selectedCalcGroup = calcGroups[0];
}
else
{
    var cgForm = new Form()
    {
        Text = "Select Calculation Group",
        Width = 400,
        Height = 225,
        StartPosition = FormStartPosition.Manual,
        FormBorderStyle = FormBorderStyle.FixedDialog,
        MaximizeBox = false,
        MinimizeBox = false
    };
    
    if (ownerForm != null)
    {
        int centerX = ownerForm.Location.X + (ownerForm.Width - cgForm.Width) / 2;
        int quarterY = ownerForm.Location.Y + (ownerForm.Height / 5);
        cgForm.Location = new Point(centerX, quarterY);
    }
    else
    {
        cgForm.StartPosition = FormStartPosition.CenterScreen;
    }
    
    var cgLabel = new Label()
    {
        Text = "Select a calculation group:",
        Location = new Point(10, 10),
        Width = 360,
        Height = 20
    };
    cgForm.Controls.Add(cgLabel);
    
    var cgComboBox = new ComboBox()
    {
        Location = new Point(10, 40),
        Width = 360,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    
    foreach (var cg in calcGroups)
    {
        cgComboBox.Items.Add(cg.Name);
    }
    cgComboBox.SelectedIndex = 0;
    cgForm.Controls.Add(cgComboBox);
    
    var cgOkButton = new Button()
    {
        Text = "OK",
        Location = new Point(200, 120),
        Width = 80,
        Height = 30,
        DialogResult = DialogResult.OK
    };
    cgForm.Controls.Add(cgOkButton);
    cgForm.AcceptButton = cgOkButton;
    
    var cgCancelButton = new Button()
    {
        Text = "Cancel",
        Location = new Point(290, 120),
        Width = 80,
        Height = 30,
        DialogResult = DialogResult.Cancel
    };
    cgForm.Controls.Add(cgCancelButton);
    cgForm.CancelButton = cgCancelButton;
    
    if ((ownerForm != null ? cgForm.ShowDialog(ownerForm) : cgForm.ShowDialog()) != DialogResult.OK)
    {
        Info("Operation cancelled by user.");
        return;
    }
    
    selectedCalcGroup = calcGroups.First(cg => cg.Name == cgComboBox.SelectedItem.ToString());
}

var calcItems = selectedCalcGroup.CalculationItems.ToList();

if (calcItems.Count == 0)
{
    Error($"No calculation items found in '{selectedCalcGroup.Name}'.");
    return;
}

// ===== STEP 2: SELECT CALCULATION ITEMS =====
var itemForm = new Form()
{
    Text = "Select Calculation Items",
    Width = 500,
    Height = 450,
    StartPosition = FormStartPosition.Manual,
    FormBorderStyle = FormBorderStyle.FixedDialog,
    MaximizeBox = false,
    MinimizeBox = false
};

if (ownerForm != null)
{
    int centerX = ownerForm.Location.X + (ownerForm.Width - itemForm.Width) / 2;
    int quarterY = ownerForm.Location.Y + (ownerForm.Height / 5);
    itemForm.Location = new Point(centerX, quarterY);
}
else
{
    itemForm.StartPosition = FormStartPosition.CenterScreen;
}

var itemLabel = new Label()
{
    Text = $"Calculation Group: {selectedCalcGroup.Name}\n\nSelect calculation items to modify:",
    Location = new Point(10, 10),
    Width = 460,
    Height = 50,
    Font = new Font("Segoe UI", 9, FontStyle.Regular)
};
itemForm.Controls.Add(itemLabel);

var itemPanel = new Panel()
{
    Location = new Point(10, 70),
    Width = 460,
    Height = 280,
    BorderStyle = BorderStyle.FixedSingle,
    AutoScroll = true
};
itemForm.Controls.Add(itemPanel);

var checkBoxes = new System.Collections.Generic.List<CheckBox>();
int yPos = 5;

foreach (var item in calcItems)
{
    var checkBox = new CheckBox()
    {
        Text = item.Name,
        Location = new Point(10, yPos),
        Width = 420,
        Height = 25,
        Font = new Font("Segoe UI", 9)
    };
    itemPanel.Controls.Add(checkBox);
    checkBoxes.Add(checkBox);
    yPos += 30;
}

var selectAllBtn = new Button()
{
    Text = "Select All",
    Location = new Point(10, 360),
    Width = 100,
    Height = 30
};
selectAllBtn.Click += (s, e) => {
    foreach (var cb in checkBoxes) cb.Checked = true;
};
itemForm.Controls.Add(selectAllBtn);

var clearAllBtn = new Button()
{
    Text = "Clear All",
    Location = new Point(120, 360),
    Width = 100,
    Height = 30
};
clearAllBtn.Click += (s, e) => {
    foreach (var cb in checkBoxes) cb.Checked = false;
};
itemForm.Controls.Add(clearAllBtn);

var itemOkButton = new Button()
{
    Text = "Next",
    Location = new Point(305, 360),
    Width = 80,
    Height = 30,
    DialogResult = DialogResult.OK
};
itemForm.Controls.Add(itemOkButton);
itemForm.AcceptButton = itemOkButton;

var itemCancelButton = new Button()
{
    Text = "Cancel",
    Location = new Point(390, 360),
    Width = 90,
    Height = 30,
    DialogResult = DialogResult.Cancel
};
itemForm.Controls.Add(itemCancelButton);
itemForm.CancelButton = itemCancelButton;

if ((ownerForm != null ? itemForm.ShowDialog(ownerForm) : itemForm.ShowDialog()) != DialogResult.OK)
{
    Info("Operation cancelled by user.");
    return;
}

var selectedItems = new System.Collections.Generic.List<CalculationItem>();

for (int i = 0; i < checkBoxes.Count; i++)
{
    if (checkBoxes[i].Checked)
    {
        selectedItems.Add(calcItems[i]);
    }
}

if (selectedItems.Count == 0)
{
    Warning("No calculation items were selected. Operation cancelled.");
    return;
}

// ===== STEP 3: FIND/REPLACE DIALOG =====
var frForm = new Form()
{
    Text = "Find and Replace",
    Width = 550,
    Height = 440,
    StartPosition = FormStartPosition.Manual,
    FormBorderStyle = FormBorderStyle.FixedDialog,
    MaximizeBox = false,
    MinimizeBox = false
};

if (ownerForm != null)
{
    int centerX = ownerForm.Location.X + (ownerForm.Width - frForm.Width) / 2;
    int quarterY = ownerForm.Location.Y + (ownerForm.Height / 5);
    frForm.Location = new Point(centerX, quarterY);
}
else
{
    frForm.StartPosition = FormStartPosition.CenterScreen;
}

var frHeaderLabel = new Label()
{
    Text = "Find is case-insensitive, Replace is case-sensitive",
    Location = new Point(10, 10),
    Width = 520,
    Height = 20,
    Font = new Font("Segoe UI", 9, FontStyle.Regular)
};
frForm.Controls.Add(frHeaderLabel);

var itemsLabel = new Label()
{
    Text = $"Selected items ({selectedItems.Count}):",
    Location = new Point(10, 35),
    Width = 520,
    Height = 20,
    Font = new Font("Segoe UI", 9, FontStyle.Bold)
};
frForm.Controls.Add(itemsLabel);

var itemsListBox = new TextBox()
{
    Location = new Point(10, 60),
    Width = 520,
    Height = 150,
    Multiline = true,
    ScrollBars = ScrollBars.Vertical,
    ReadOnly = true,
    Font = new Font("Segoe UI", 8),
    Text = string.Join(Environment.NewLine, selectedItems.Select(i => "  • " + i.Name))
};
frForm.Controls.Add(itemsListBox);

var findLabel = new Label()
{
    Text = "Find text:",
    Location = new Point(10, 225),
    Width = 520,
    Height = 20
};
frForm.Controls.Add(findLabel);

var findTextBox = new TextBox()
{
    Location = new Point(10, 250),
    Width = 520,
    Height = 25,
    Font = new Font("Segoe UI", 9)
};
frForm.Controls.Add(findTextBox);

var replaceLabel = new Label()
{
    Text = "Replace with: (leave empty to remove find text)",
    Location = new Point(10, 285),
    Width = 520,
    Height = 20
};
frForm.Controls.Add(replaceLabel);

var replaceTextBox = new TextBox()
{
    Location = new Point(10, 310),
    Width = 520,
    Height = 25,
    Font = new Font("Segoe UI", 9)
};
frForm.Controls.Add(replaceTextBox);

var frOkButton = new Button()
{
    Text = "Execute",
    Location = new Point(355, 355),
    Width = 80,
    Height = 30,
    DialogResult = DialogResult.OK
};
frForm.Controls.Add(frOkButton);
frForm.AcceptButton = frOkButton;

var frCancelButton = new Button()
{
    Text = "Cancel",
    Location = new Point(445, 355),
    Width = 80,
    Height = 30,
    DialogResult = DialogResult.Cancel
};
frForm.Controls.Add(frCancelButton);
frForm.CancelButton = frCancelButton;

if ((ownerForm != null ? frForm.ShowDialog(ownerForm) : frForm.ShowDialog()) != DialogResult.OK)
{
    Info("Operation cancelled by user.");
    return;
}

string findText = findTextBox.Text;
string replaceText = replaceTextBox.Text;

// Validate find text
if (string.IsNullOrEmpty(findText))
{
    Error("Find text cannot be empty. Operation cancelled.");
    return;
}

// ===== ANALYZE CHANGES =====
var changes = new System.Collections.Generic.List<(CalculationItem item, string oldExpr, string newExpr, int count)>();
var itemsNotFound = new System.Collections.Generic.List<string>();

foreach (var item in selectedItems)
{
    string oldExpression = item.Expression;
    
    // Case-insensitive find, case-sensitive replace
    int count = Regex.Matches(oldExpression, Regex.Escape(findText), RegexOptions.IgnoreCase).Count;
    
    if (count == 0)
    {
        itemsNotFound.Add(item.Name);
        continue;
    }
    
    string newExpression = Regex.Replace(oldExpression, Regex.Escape(findText), replaceText, RegexOptions.IgnoreCase);
    changes.Add((item, oldExpression, newExpression, count));
}

// Warn about items where find text wasn't found
if (itemsNotFound.Count > 0)
{
    Warning($"Find text '{findText}' not found in {itemsNotFound.Count} item(s):\n" + 
            string.Join("\n", itemsNotFound));
}

if (changes.Count == 0)
{
    Warning("No changes to make. Find text was not found in any selected items.");
    return;
}

// ===== STEP 4: PREVIEW/CONFIRMATION (OPTIONAL) =====
if (SHOW_PREVIEW_CONFIRMATION)
{
    var previewForm = new Form()
    {
        Text = "Preview Changes",
        Width = 1900,
        Height = 900,
        StartPosition = FormStartPosition.Manual,
        FormBorderStyle = FormBorderStyle.Sizable,
        MaximizeBox = true,
        MinimizeBox = false
    };
    
    if (ownerForm != null)
    {
        int centerX = ownerForm.Location.X + (ownerForm.Width - previewForm.Width) / 2;
        int quarterY = ownerForm.Location.Y + (ownerForm.Height / 5);
        previewForm.Location = new Point(centerX, quarterY);
    }
    else
    {
        previewForm.StartPosition = FormStartPosition.CenterScreen;
    }
    
    var previewLabel = new Label()
    {
        Text = $"Found '{findText}' in {changes.Count} item(s). Total replacements: {changes.Sum(c => c.count)}",
        Location = new Point(10, 10),
        Width = 1860,
        Height = 25,
        Font = new Font("Segoe UI", 10, FontStyle.Bold)
    };
    previewForm.Controls.Add(previewLabel);
    
    var previewTextBox = new TextBox()
    {
        Location = new Point(10, 40),
        Width = 1860,
        Height = 775,
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        ReadOnly = true,
        Font = new Font("Consolas", 9),
        WordWrap = false
    };
    
    var previewText = new System.Text.StringBuilder();
    foreach (var change in changes)
    {
        previewText.AppendLine($"═══ {change.item.Name} ({change.count} replacement(s)) ═══");
        previewText.AppendLine($"BEFORE:\n{change.oldExpr}");
        previewText.AppendLine($"\nAFTER:\n{change.newExpr}");
        previewText.AppendLine();
    }
    
    previewTextBox.Text = previewText.ToString();
    previewForm.Controls.Add(previewTextBox);
    
    var previewOkButton = new Button()
    {
        Text = "Apply Changes",
        Location = new Point(1685, 825),
        Width = 100,
        Height = 30,
        DialogResult = DialogResult.OK
    };
    previewForm.Controls.Add(previewOkButton);
    previewForm.AcceptButton = previewOkButton;
    
    var previewCancelButton = new Button()
    {
        Text = "Cancel",
        Location = new Point(1790, 825),
        Width = 80,
        Height = 30,
        DialogResult = DialogResult.Cancel
    };
    previewForm.Controls.Add(previewCancelButton);
    previewForm.CancelButton = previewCancelButton;
    
    if ((ownerForm != null ? previewForm.ShowDialog(ownerForm) : previewForm.ShowDialog()) != DialogResult.OK)
    {
        Info("Operation cancelled by user.");
        return;
    }
}

// ===== STEP 5: APPLY CHANGES =====
int totalReplacements = 0;

foreach (var change in changes)
{
    change.item.Expression = change.newExpr;
    totalReplacements += change.count;
   // Info($"Updated: {change.item.Name} ({change.count} replacement(s))");
}

Info($"\n✓ Successfully modified {changes.Count} calculation item(s) with {totalReplacements} total replacement(s).");
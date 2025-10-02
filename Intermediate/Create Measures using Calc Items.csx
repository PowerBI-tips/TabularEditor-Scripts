/*
 * Generate measures based on a selected calculation items already created.
 *
 * Author: Dan Meissner
 *
 * You must have created the calculation group items beforehand.
 *
 * This script will work on any calculation groups/items within the model, but is most useful
 * and was conceived specifically for Time Intelligence Calc. Items.
 *
 * Once one or more measures are selected, running this script will pop up a dialog box that
 * allows the user to select a Calculation Group and then select one or more Calculation Items
 * from that group. The script will then make a single measure for each base measure && calc item
 * combination.  The new measures will maintain the format string of the original base measure
 * and will be created in the same display folder as the original base measure.
 */


using System.Windows.Forms;
using System.Drawing;

// ===== VALIDATION =====
if (Selected.Measures.Count == 0)
{
    Error("Please select at least one measure before running this script.");
    return;
}

// Get all calculation groups in the model
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
    // Only one calculation group, use it automatically
    selectedCalcGroup = calcGroups[0];
}
else
{
    // Multiple calculation groups - let user choose
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
    
    // Position the form: horizontally centered, 1/5 down from top
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

// Get calculation items from selected group
var calcItems = selectedCalcGroup.CalculationItems.ToList();

if (calcItems.Count == 0)
{
    Error($"No calculation items found in '{selectedCalcGroup.Name}'.");
    return;
}

// ===== STEP 2: SELECT CALCULATION ITEMS =====
var form = new Form()
{
    Text = "Select Calculation Items",
    Width = 500,
    Height = 450,
    StartPosition = FormStartPosition.Manual,
    FormBorderStyle = FormBorderStyle.FixedDialog,
    MaximizeBox = false,
    MinimizeBox = false
};

// Position the form: horizontally centered, 1/5 down from top
if (ownerForm != null)
{
    int centerX = ownerForm.Location.X + (ownerForm.Width - form.Width) / 2;
    int quarterY = ownerForm.Location.Y + (ownerForm.Height / 5);
    form.Location = new Point(centerX, quarterY);
}
else
{
    form.StartPosition = FormStartPosition.CenterScreen;
}

string measuresText = Selected.Measures.Count == 1 
    ? $"Base Measure: {Selected.Measures.First().Name}" 
    : $"Base Measures: {Selected.Measures.Count} selected";

var label = new Label()
{
    Text = $"{measuresText}\nCalculation Group: {selectedCalcGroup.Name}\n\nSelect calculation items:",
    Location = new Point(10, 10),
    Width = 460,
    Height = 60,
    Font = new Font("Segoe UI", 9, FontStyle.Regular)
};
form.Controls.Add(label);

var panel = new Panel()
{
    Location = new Point(10, 80),
    Width = 460,
    Height = 270,
    BorderStyle = BorderStyle.FixedSingle,
    AutoScroll = true
};
form.Controls.Add(panel);

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
    panel.Controls.Add(checkBox);
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
form.Controls.Add(selectAllBtn);

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
form.Controls.Add(clearAllBtn);

var okButton = new Button()
{
    Text = "Create Measures",
    Location = new Point(280, 360),
    Width = 100,
    Height = 30,
    DialogResult = DialogResult.OK
};
form.Controls.Add(okButton);
form.AcceptButton = okButton;

var cancelButton = new Button()
{
    Text = "Cancel",
    Location = new Point(385, 360),
    Width = 90,
    Height = 30,
    DialogResult = DialogResult.Cancel
};
form.Controls.Add(cancelButton);
form.CancelButton = cancelButton;

// ===== PROCESS RESULTS =====
if ((ownerForm != null ? form.ShowDialog(ownerForm) : form.ShowDialog()) != DialogResult.OK)
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
    Warning("No calculation items were selected. No measures created.");
    return;
}

// ===== CREATE NEW MEASURES =====
int createdCount = 0;
string calcColumnName = selectedCalcGroup.Columns.First().Name;

// Outer loop: iterate through each selected measure
foreach (var currentMeasure in Selected.Measures)
{
    var targetTable = currentMeasure.Table;
    Info($"\nProcessing base measure: {currentMeasure.Name}");
    
    // Inner loop: iterate through each selected calculation item
    foreach (var calcItem in selectedItems)
    {
        string newMeasureName = $"{calcItem.Name} {currentMeasure.Name}";
        
        if (targetTable.Measures.Contains(newMeasureName))
        {
            Warning($"Measure '{newMeasureName}' already exists. Skipping.");
            continue;
        }
        
        var newMeasure = targetTable.AddMeasure(newMeasureName);
        newMeasure.Expression = $"CALCULATE ( [{currentMeasure.Name}], '{selectedCalcGroup.Name}'[{calcColumnName}] = \"{calcItem.Name}\" )";
        newMeasure.FormatString = currentMeasure.FormatString;
        newMeasure.Description = $"Time intelligence calculation of {currentMeasure.Name} using {calcItem.Name}";
        newMeasure.DisplayFolder = currentMeasure.DisplayFolder;
        
        foreach (var perspective in Model.Perspectives)
        {
            if (currentMeasure.InPerspective[perspective])
            {
                newMeasure.InPerspective[perspective] = true;
            }
        }
        
        // Info($"Created measure: {newMeasureName}");
        createdCount++;
    }
}

Info($"\nSuccessfully created {createdCount} total measure(s) from {Selected.Measures.Count} base measure(s)");
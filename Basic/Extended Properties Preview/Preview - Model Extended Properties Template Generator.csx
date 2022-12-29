
//
// Author: Tommy Puglia
// Website: https://pugliabi.com
// Check out the Explicit Measures Podcast on Apple, spotify, & YouTube!
// https://www.youtube.com/playlist?list=PLn1m_aBmgsbHr83c1P6uqaWF5PLdFzOjj
//
// Global Model Extended Properties Template Generator
//
// Tied to an example of the Group by Global Measure Script in this same repo

// This Script is a preview of how  easy it can be to create standard Extended Properties based on a model (such as a Date Table, referencing the fact table's date column, etc.)
//
// Why?
// Creating Global Extended Properties with the same keys across Models would open up endless possibilities for utilizing scripts in Tabular Editor to automate tasks based on the Extended Properties.
//
// Take the example this script does. It looks at the current model if the standard Extended Properties are available, if not will prompt you to choose the columns and it auto-create the Extended Properties.
// Then, any other macro can simply call the key value and use it for Time Intelligence, Rolling Calcs, Calculation Groups, or any other use case.
// Rather than having to always select the objects per model, or worse hard-coding the value in a macro script that changes per model, having global Extended Properties would allow for a more dynamic approach.

// Instructions
// Simply run the script on the model, and it will prompt you to choose the following:
// Date Table, Date column, and Date Year Column (exp 2022)
// Fact Table, and the date column in the fact table
// A Global Measure (for default calculations)

// More can be added!

#r "C:\Program Files\Tabular Editor 3\TabularEditor3.exe" // *** Needed for C# scripting, remove in TE3 ***
#r "C:\Program Files (x86)\Tabular Editor 3\TabularEditor3.exe" // *** Needed for C# scripting, remove in TE3 ***
#r "System.IO"
#r "Microsoft.VisualBasic"

using TabularEditor.TOMWrapper; // *** Needed for C# scripting, remove in TE3 ***
using TabularEditor.Scripting; // *** Needed for C# scripting, remove in TE3 ***
using System.IO;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

Model Model; // *** Needed for C# scripting, remove in TE3 ***
TabularEditor.Shared.Interaction.Selection Selected; // *** Needed for C# scripting, remove in TE3 ***



Func<IList<string>, string, string> SelectString = (IList<string> listText, string titleText) =>
{

    var listboxText = new ListBox()
    {
        Dock = DockStyle.Fill
    };

    var panelButtons = new Panel()
    {
        Height = 22,
        Dock = DockStyle.Bottom
    };
    
    var buttonOK = new Button()
    {
        Text = "OK",
        DialogResult = DialogResult.OK,
        Left = 120
    };

    var buttonCancel = new Button()
    {
        Text = "Cancel",
        DialogResult = DialogResult.Cancel,
        Left = 204
    };

    var formInputBox = new Form()
    {
        Text = titleText,
        Padding = new System.Windows.Forms.Padding(8),
        FormBorderStyle = FormBorderStyle.FixedDialog,
        MinimizeBox = false,
        MaximizeBox = false,
        StartPosition = FormStartPosition.CenterScreen,
        AcceptButton = buttonOK,
        CancelButton = buttonCancel
    };
    listboxText.Items.AddRange(listText.ToArray());
    listboxText.SelectedItem = listText[0];
    formInputBox.Controls.AddRange(new Control[] { listboxText, panelButtons });
    panelButtons.Controls.AddRange(new Control[] { buttonOK, buttonCancel });
    return formInputBox.ShowDialog() == DialogResult.OK ? listboxText.SelectedItem.ToString() : null;

};
// Get Extended Properties & Template
var mExtProp = Model.GetExtendedProperties();
var TemplateExtendedProperties = new string[] {
    "DateTable",
    "DateColumn",
    "DateYearColumn",
    "FactTable",
    "FactDateColumn",
    "GlobalMeasure"
};
var ChooseInput = (new string[] {"Yes","No"});


// If Copying
var  stepDateTable = "DateTable";
var stepDateColumn = "DateColumn";
var stepDateYearColumn = "DateYearColumn";
var stepFactTable = "FactTable";
var stepFactDateColumn = "FactDateColumn";
var stepGlobalMeasure = "GlobalMeasure";

var sb = new System.Text.StringBuilder();
var createdb = new System.Text.StringBuilder();
string newline = Environment.NewLine;
sb.Append("ExtenPropertyName" + '\t' + "ExtenPropertyValue" + '\t' + "Exisits" + newline);
createdb.Append("{" + newline );


// Add DateTable Extended Property
var epDateTable = "";
var epDateColumn = "";
var epDateYearColumn = "";
var quotes = "_";
if(Model.GetExtendedProperty(stepDateTable) == null)
{
    
   var DateTable = SelectTable(label: "Select your Date Table");
    var DateColumn = SelectColumn(DateTable.Columns, label: "Select your Date Column");
    var DateYearColumn = SelectColumn(DateTable.Columns, label: "Select your Date Year Column");
   string epName = DateTable.Name;
    Model.SetExtendedProperty(stepDateTable, epName, ExtendedPropertyType.String);
    Model.SetExtendedProperty(stepDateColumn, DateColumn.DaxObjectFullName, ExtendedPropertyType.String);
    Model.SetExtendedProperty(stepDateYearColumn, DateYearColumn.DaxObjectFullName, ExtendedPropertyType.String);
}
else
{
    var UpdateDates = SelectString(new string[] {"Yes", "No"}, "Update Date Extended Properties?");
    if (UpdateDates == "Yes")
    {
        var DateTable = SelectTable(label: "Select your Date Table");
        var DateColumn = SelectColumn(DateTable.Columns, label: "Select your Date Column");
        var DateYearColumn = SelectColumn(DateTable.Columns, label: "Select your Date Year Column");
        string epName = DateTable.Name;
        Model.SetExtendedProperty(stepDateTable, epName, ExtendedPropertyType.String);
        Model.SetExtendedProperty(stepDateColumn, DateColumn.DaxObjectFullName, ExtendedPropertyType.String);
        Model.SetExtendedProperty(stepDateYearColumn, DateYearColumn.DaxObjectFullName, ExtendedPropertyType.String);
    }
}
if(Model.GetExtendedProperty(stepFactTable) == null)
{
var FactTable = SelectTable(label: "Select your fact Table");
var FactDateColumn = SelectColumn(FactTable.Columns, label: "Select your Fact Date Column");
Model.SetExtendedProperty(stepFactTable, FactTable.Name, ExtendedPropertyType.String);
Model.SetExtendedProperty(stepFactDateColumn, FactDateColumn.DaxObjectFullName, ExtendedPropertyType.String);
}
else
{
    var UpdateFact = SelectString(new string[] {"Yes", "No"}, "Update Fact Extended Properties?");
    if (UpdateFact == "Yes")
    {
        var FactTable = SelectTable(label: "Select your fact Table");
        var FactDateColumn = SelectColumn(FactTable.Columns, label: "Select your Fact Date Column");
        Model.SetExtendedProperty(stepFactTable, FactTable.Name, ExtendedPropertyType.String);
        Model.SetExtendedProperty(stepFactDateColumn, FactDateColumn.DaxObjectFullName, ExtendedPropertyType.String);
    }
}
if (stepGlobalMeasure == null)
{
var GlobalMeasures = SelectMeasure(label: "Select your Global Measure");   
Model.SetExtendedProperty(stepGlobalMeasure, GlobalMeasures.DaxObjectFullName, ExtendedPropertyType.String);
}
else
{
    var UpdateGlobal = SelectString(new string[] {"Yes", "No"}, "Update Global Measure?");
    if (UpdateGlobal == "Yes")
    {
        var GlobalMeasures = SelectMeasure(label: "Select your Global Measure");   
        Model.SetExtendedProperty(stepGlobalMeasure, GlobalMeasures.DaxObjectFullName, ExtendedPropertyType.String);
    }
}

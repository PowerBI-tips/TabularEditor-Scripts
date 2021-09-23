
// Create visuals translations measures 
// Author: Tommy Puglia powerbi.tips/podcast
// This script creates a pop-up form that provides the calculation type and the format to apply to muliptple columns
// You can use this to create new measures with currency, or even percentage. Choose Sum, Average, or Count. 
// Note that the form pop up is not pretty, but it works.


#r "System.Drawing"

using System.Drawing;
using System.Windows.Forms;

System.Windows.Forms.Form newForm = new System.Windows.Forms.Form();

System.Windows.Forms.Panel midPanel = new System.Windows.Forms.Panel();
System.Windows.Forms.Panel topMidPanel = new System.Windows.Forms.Panel();
System.Windows.Forms.RadioButton newmodelButton = new System.Windows.Forms.RadioButton();
System.Windows.Forms.RadioButton Measure = new System.Windows.Forms.RadioButton();
System.Windows.Forms.Button goButton = new System.Windows.Forms.Button();
System.Windows.Forms.TextBox enterTextBox = new System.Windows.Forms.TextBox();
System.Windows.Forms.ComboBox enterComboBox = new System.Windows.Forms.ComboBox();
System.Windows.Forms.ComboBox enterCurrency = new System.Windows.Forms.ComboBox();
System.Windows.Forms.Button JustMeasuresButton = new System.Windows.Forms.Button();


int startScreenX = 230;
int startScreenY = 150;
//EnterCombo
// Enter Combo Box
enterComboBox.Visible = true;
enterComboBox.Size = new Size(100,40);
enterComboBox.Location = new Point(startScreenX+44,startScreenY+44);
enterComboBox.Items.Add("Average");
enterComboBox.Items.Add("Sum");
enterComboBox.Items.Add("CountRows");
enterComboBox.Items.Add("CountDistinct");

// Enter Combo Box
enterCurrency.Visible = true;
enterCurrency.Size = new Size(100,40);
enterCurrency.Location = new Point(startScreenX+33,startScreenY+20);
enterCurrency.Items.Add("$");
enterCurrency.Items.Add("Number");
enterCurrency.Items.Add("%");

//Choice

//NE
int formWidth = 1000;
int formHeight = 700;
newForm.TopMost = true;
newForm.Text = "Create Measures Button";
newForm.Size = new Size(formWidth,formHeight);

// New Model Button
goButton.Size = new Size(100,25);
goButton.Location = new Point(startScreenX+25,startScreenY+80);
goButton.Text = "Go";
goButton.Visible = true;
goButton.Enabled = true; 

// New Model Button
JustMeasuresButton.Size = new Size(100,25);
JustMeasuresButton.Location = new Point(startScreenX+15,startScreenY+60);
JustMeasuresButton.Text = "Just Measures";
JustMeasuresButton.Visible = true;
JustMeasuresButton.Enabled = true; 

string calType = string.Empty;
string forType = string.Empty;
string formatString = string.Empty;

goButton.Click += (sender2, e2) => {

    calType = enterComboBox.Text;
    forType = enterCurrency.Text;
    if( forType == "$")
    {
        formatString = "$ #,##0";
    }
    else if (forType == "%")
    {
        formatString = "0.0%";
    }
    else 
    {
        formatString = "#,0";
        
    }
    if(calType == "Average")
    {

    foreach(var c in Selected.Columns)
    {
        var newMeasure = c.Table.AddMeasure(
        "Avg. " + c.Name,                    // Name
        "Average(" + c.DaxObjectFullName + ")"   // DAX expression
                         // Display Folder
    );
    newMeasure.FormatString = formatString;
    c.IsHidden = true;
    newMeasure.DisplayFolder = "_KPI";
    }
    }
    else if (calType == "Sum")
    {
    foreach(var c in Selected.Columns)
    {
        var newMeasure = c.Table.AddMeasure(
        "Total " + c.Name,                    // Name
        "SUM(" + c.DaxObjectFullName + ")"
        // DAX expression
                            // Display Folder
    );

        newMeasure.FormatString = formatString;
        newMeasure.DisplayFolder = "_KPI";

        c.IsHidden = true;

    }   
    }
      else if (calType == "CountRows")
    {
    foreach(var c in Selected.Columns)
    {
        var newMeasure = c.Table.AddMeasure(
        "Count of " + c.Name,                    // Name
        "COUNTROWS(" + c.Table + ")"
        // DAX expression
                            // Display Folder
    );

        newMeasure.FormatString = formatString;
        newMeasure.DisplayFolder = "_KPI";

        c.IsHidden = true;

    }   
    }
    else 
    {
    foreach(var c in Selected.Columns)
    {
        var newMeasure = c.Table.AddMeasure(
        "Distinct " + c.Name,                    // Name
        "DISTINCTCOUNT(" + c.DaxObjectFullName + ")"
        // DAX expression
                            // Display Folder
    );

        newMeasure.FormatString = formatString;
        newMeasure.DisplayFolder = "_KPI";

        c.IsHidden = true;

    }   
    }
};
JustMeasuresButton.Click += (sender3, e3) => {


    forType = enterCurrency.Text;
    if( forType == "$")
    {
        formatString = "$ #,##0";
    }
    else if (forType == "%")
    {
        formatString = "0.0%";
    }
    else 
    {
        formatString = "#,0";
        
    }

    foreach(var c in Selected.Measures)
    {

    c.FormatString = formatString;
    }
    };




string perspName = string.Empty;

newForm.Controls.Add(midPanel);
newForm.Controls.Add(topMidPanel);
newForm.Controls.Add(goButton);
newForm.Controls.Add(enterTextBox);
newForm.Controls.Add(Measure);
newForm.Controls.Add(newmodelButton);
newForm.Controls.Add(enterComboBox);
newForm.Controls.Add(enterCurrency);
newForm.Controls.Add(JustMeasuresButton);
newForm.Show();
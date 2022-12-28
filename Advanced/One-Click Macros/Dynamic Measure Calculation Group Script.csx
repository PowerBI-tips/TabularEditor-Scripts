#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;

// by Johnny Winter
// www.greyskullanalytics.com
// '2021-10-15 / B.Agullo / dynamic parameters by B.Agullo / 
// '2022-05-14 / B.Agullo / you can now rerun the script to simply add new measures or update existing ones
// '2022-07-11 / B.Agullo / no check on the number of measures selected so it can be launched just to create the calc group table

// Instructions:
//select the measures you want to add to your Dynamic Measure and then run this script (or store it as macro)

//
// ----- do not modify script below this line -----
//

string dynamicMeasureCgTag = "@GreyskullPBI";
string dynamicMeasureCgValue = "Dynamic Measure Calculation Group";

string dummyMeasureTag = dynamicMeasureCgTag;
string dummyMeasureValue = "Dummy Measure";

string calcGroupName = "";
string columnName = "";
string measureName = "";
string secondaryMeasureName = "";
string conditionalFormatMeasureName = ""; 

Measure dummyMeasure = null as Measure;

var dynamicCGs = Model.Tables.Where(x => x.GetAnnotation(dynamicMeasureCgTag) == dynamicMeasureCgValue);

CalculationGroupTable cgTable = null as CalculationGroupTable; 
// CalculationGroup cg = null as CalculationGroup;

if(dynamicCGs.Count() == 1)
{
    //reuse the calc group
    cgTable = dynamicCGs.First() as CalculationGroupTable;

}
else if (dynamicCGs.Count() < 1)
{
    //create the calc group
    calcGroupName = Interaction.InputBox("Provide a name for your Calc Group", "Calc Group Name", "Dynamic Measure", 740, 400);
    if (calcGroupName == "") return;

    columnName = Interaction.InputBox("Calc Group column name", "Column Name", calcGroupName, 740, 400);
    if (columnName == "") return;

    //check to see if a table with this name already exists
    //if it doesnt exist, create a calculation group with this name
    if (!Model.Tables.Contains(calcGroupName))
    {
        cgTable = Model.AddCalculationGroup(calcGroupName);
        cgTable.Description = "Contains dynamic measures and a column called " + columnName + ". The contents of the dynamic measures can be controlled by selecting values from " + columnName + ".";
    };
    
    //set variable for the calc group
    Table calcGroup = Model.Tables[calcGroupName];

    //if table already exists, make sure it is a Calculation Group type
    if (calcGroup.SourceType.ToString() != "CalculationGroup")
    {
        Error("Table exists in Model but is not a Calculation Group. Rename the existing table or choose an alternative name for your Calculation Group.");
        return;
    };

    //apply the annotation so the user is not asked again
    cgTable = calcGroup as CalculationGroupTable;
    cgTable.SetAnnotation(dynamicMeasureCgTag, dynamicMeasureCgValue);

    //by default the calc group has a column called Name. If this column is still called Name change this in line with specfied variable
    if (cgTable.Columns.Contains("Name"))
    {
        cgTable.Columns["Name"].Name = columnName;
    };
    cgTable.Columns[columnName].Description = "Select value(s) from this column to control the contents of the dynamic measures.";

}
else
{
    //make them choose the calc group -- should not happen! 
    cgTable = SelectTable(dynamicCGs, label: "Select your Dynamic Measure Calculation Group For Arbitrary 2-row Header") as CalculationGroupTable;
}

//get the column name in case the calc group was already there
columnName = cgTable.Columns.Where(x => x.Name != "Ordinal").First().Name;

var dummyMeasures = Model.AllMeasures.Where(x => x.GetAnnotation(dummyMeasureTag) == dummyMeasureValue);

if (dummyMeasures.Count() == 1)
{
    //get the measure
    measureName = dummyMeasures.First().Name;

}
else if (dummyMeasures.Count() < 1)
{
    //create the measure
    measureName = Interaction.InputBox("Dynamic Measure Name (cannot be named \"" + columnName + "\")", "Measure Name", "Dummy", 740, 400);
    if (measureName == "") return;

}
else
{
    //choose measure (should not happen!)
    measureName = SelectMeasure(dummyMeasures).Name;
};

secondaryMeasureName = measureName + " 2";
conditionalFormatMeasureName = measureName + " CF";

//check to see if dynamic measure has been created, if not create it now
//if a measure with that name alredy exists elsewhere in the model, throw an error
if (!cgTable.Measures.Contains(measureName))
{
    dummyMeasure = cgTable.AddMeasure(measureName, "BLANK()");
    dummyMeasure.Description = "Control the content of this measure by selecting values from " + columnName + ".";
    dummyMeasure.SetAnnotation(dummyMeasureTag, dummyMeasureValue);
};

if (!cgTable.Measures.Contains(secondaryMeasureName))
{
    dummyMeasure = cgTable.AddMeasure(secondaryMeasureName, "BLANK()");
    dummyMeasure.Description = "Control the content of this measure by selecting values from " + columnName + ". Secondary dynamic measure for complex use cases";
};

if (!cgTable.Measures.Contains(conditionalFormatMeasureName))
{
    dummyMeasure = cgTable.AddMeasure(conditionalFormatMeasureName, "BLANK()");
    dummyMeasure.Description = "Control the content of this measure by selecting values from " + columnName + ". Used this measure for conditional format purposes";
};



string isSelectedMeasureString = "[" + measureName + "],[" + secondaryMeasureName + "],[" + conditionalFormatMeasureName + "]";

//if no measures were selected that's the end of the story
//(only makes sense if being launched from another script and soon after will be launched against one or more measures)
if (Selected.Measures.Count == 0) 
{
    return;
}
foreach (var m in Selected.Measures)
{
    
    //remove calculation item if already exists
    if (cgTable.CalculationItems.Contains(m.Name)) {
        cgTable.CalculationItems[m.Name].Delete();
    };
            
    //if (!cg.CalculationItems.Contains(m.Name))
    //{

    var newCalcItem = 
        cgTable.AddCalculationItem(
            m.Name, 
            "IF ( " + "ISSELECTEDMEASURE (" + isSelectedMeasureString + "), " + "[" + m.Name + "], " + "SELECTEDMEASURE() )"
        );

    // '2021-10-15 / B.Agullo / double quotes in format string need to be doubled to be preserved
    newCalcItem.FormatStringExpression = "IF ( " + "ISSELECTEDMEASURE (" + isSelectedMeasureString + "),\"" + m.FormatString.Replace("\"", "\"\"") + "\", SELECTEDMEASUREFORMATSTRING() )";
    newCalcItem.FormatDax();


    //};
};
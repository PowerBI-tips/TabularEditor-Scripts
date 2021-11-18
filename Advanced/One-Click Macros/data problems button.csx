#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;


// '2021-05-26 / B.Agullo / 
// '2021-10-13 / B.Agullo / dynamic parameters for one-click operation
// by Bernat Agulló
// www.esbrina-ba.com

// Instructions: 
//select the measures that counts the number of "data problems" the model has and then run the script or as macro
//when adding macro select measure context for execution 

//
// ----- do not modify script below this line -----
//


if (Selected.Measures.Count != 1) {
    Error("Select one and only one measure");
    return;
};


string navigationTableName = Interaction.InputBox("Provide a name for navigation measures table name", "Navigation Table Name", "Navigation", 740, 400);
if(navigationTableName == "") return;

if(Model.Tables.Any(Table => Table.Name == navigationTableName)) {
    Error(navigationTableName + " already exists!");
    return; 
};

string buttonTextMeasureName = Interaction.InputBox("Name for your button text measure", "Button text measure name", "Button Text", 740, 400);
if(buttonTextMeasureName == "") return;

string buttonTextPattern = Interaction.InputBox("Provide a pattern for your button text", "Button text pattern (# = no. of problems)", "There are # data problems", 740, 400);
if(buttonTextPattern == "") return;

string buttonBackgroundMeasureName = Interaction.InputBox("Name your button background measure", "Button Background Measure", "Button Background", 740, 400);
if(buttonBackgroundMeasureName == "") return;

string buttonNavigationMeasureName = Interaction.InputBox("Name your button navigation measure", "Button Navigation Measure", "Button Navigation", 740, 400);
if(buttonNavigationMeasureName == "") return;

string thereAreDataProblemsMeasureName = Interaction.InputBox("Name your data problems flag measure", "Data problems Flag Measure", "There are Data Problems", 740, 400);
if(thereAreDataProblemsMeasureName == "") return;

string dataProblemsSheetName = Interaction.InputBox("Where are the data problems detail?", "Data problems Sheet", "Data Problems", 740, 400);
if(dataProblemsSheetName == "") return;


//colors will be created if not present
string buttonColorMeasureNameWhenVisible = Interaction.InputBox("What's the color measure name when the button is visible?", "Visible color measure name", "Warning Color", 740, 400);
if(buttonColorMeasureNameWhenVisible == "") return;

string buttonColorMeasureValueWhenVisible = Interaction.InputBox("What's the color code of " + buttonColorMeasureNameWhenVisible + "?", "Visible color code", "#D64554", 740, 400);
if(buttonColorMeasureValueWhenVisible == "") return;
buttonColorMeasureValueWhenVisible = "\"" + buttonColorMeasureValueWhenVisible + "\""


string buttonColorMeasureNameWhenInvisible = Interaction.InputBox("What's the color measure name when button is invisible?", "Invisible color measure name", "Report Background Color", 740, 400);
if(buttonColorMeasureNameWhenInvisible == "") return;
buttonColorMeasureNameWhenInvisible = "\"" + buttonColorMeasureNameWhenInvisible + "\""

string buttonColorMeasureValueWhenInvisible = Interaction.InputBox("What's the color code of " + buttonColorMeasureNameWhenInvisible + "?", "Invisible color measure name", "Report Background Color", 740, 400);
if(buttonColorMeasureValueWhenInvisible == "") return;
buttonColorMeasureValueWhenInvisible = "\"" + buttonColorMeasureValueWhenInvisible + "\""



//prepare array to iterate on new measure names 
string[] newMeasureNames = 
    {
        buttonTextMeasureName,
        buttonBackgroundMeasureName,
        buttonNavigationMeasureName,
        thereAreDataProblemsMeasureName
    };

//check none of the new measure names already exist as such 
foreach(string measureName in newMeasureNames) {
    if(Model.AllMeasures.Any(Measure => Measure.Name == measureName)) {
        Error(measureName + " already exists!"); 
        return;
    };
};
    
var dataProblemsMeasure = Selected.Measure; 

string navigationTableExpression = 
    "FILTER({1},[Value] = 0)";

var navigationTable = 
    Model.AddCalculatedTable(navigationTableName,navigationTableExpression);
    
navigationTable.FormatDax(); 
navigationTable.Description = 
    "Table to store the measures for the dynamic button that leads to the data problems sheet";

navigationTable.IsHidden = true;     

if(!Model.AllMeasures.Any(Measure => Measure.Name == buttonColorMeasureNameWhenVisible)) {
    navigationTable.AddMeasure(buttonColorMeasureNameWhenVisible,buttonColorMeasureValueWhenVisible);
};

if(!Model.AllMeasures.Any(Measure => Measure.Name == buttonColorMeasureNameWhenInvisible)) {
    navigationTable.AddMeasure(buttonColorMeasureNameWhenInvisible,"\"#FFFFFF00\"");
};


string thereAreDataProblemsMeasureExpression = 
    "[" + dataProblemsMeasure.Name + "]>0";


var thereAreDataProblemsMeasure = 
    navigationTable.AddMeasure(
        thereAreDataProblemsMeasureName,
        thereAreDataProblemsMeasureExpression
    );

thereAreDataProblemsMeasure.FormatDax(); 
thereAreDataProblemsMeasure.Description = "Boolean measure, if true, the button leading to data problems sheet should show (internal use only)" ;
 
string buttonBackgroundMeasureExpression = 
    "VAR colorCode = " + 
    "    IF(" + 
    "        [" + thereAreDataProblemsMeasureName + "]," + 
    "        [" + buttonColorMeasureNameWhenVisible + "]," + 
    "        [" + buttonColorMeasureNameWhenInvisible + "]" + 
    "    )" + 
    "RETURN " + 
    "    FORMAT(colorCode,\"@\")";
    
var buttonBackgroundMeasure = 
    navigationTable.AddMeasure(
        buttonBackgroundMeasureName,
        buttonBackgroundMeasureExpression
    );
    
buttonBackgroundMeasure.FormatDax(); 
buttonBackgroundMeasure.Description = "Use this measure for conditional formatting of button background";  

string buttonNavigationMeasureExpression = 
    "IF(" + 
    "    [" + thereAreDataProblemsMeasureName + "]," + 
    "    \"" + dataProblemsSheetName + "\"," + 
    "    \"\"" + 
    ")";


var buttonNavigationMeasure = 
    navigationTable.AddMeasure(
        buttonNavigationMeasureName,
        buttonNavigationMeasureExpression
    );
    
buttonNavigationMeasure.FormatDax(); 
buttonNavigationMeasure.Description = "Use this measure for conditional page navigation";  


string buttonTextMeasureExpression = 
    "IF(" + 
    "    [" + thereAreDataProblemsMeasureName + "]," + 
    "    SUBSTITUTE(\"" + buttonTextPattern + "\",\"#\",FORMAT([" + dataProblemsMeasure.Name + "],0))," + 
    "    \"\"" + 
    ")";
    
    
var buttonTextMeasure = 
    navigationTable.AddMeasure(
        buttonTextMeasureName,
        buttonTextMeasureExpression
    );
    
buttonTextMeasure.FormatDax(); 
buttonTextMeasure.Description = "Use this measure for dynamic button text";  

dataProblemsMeasure.MoveTo(navigationTable);
    
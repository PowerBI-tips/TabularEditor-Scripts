#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;

// CHANGE LOG:
// '2021-10-16 / B.Agullo / converted to macro using code from Stephen Maguire and Daniel Otykier, and people in twitter helping with C#!  
// '2021-07-10 / B.Agullo / 
// by Bernat Agulló
// www.esbrina-ba.com

// FULL EXPLANATION: 
// https://www.esbrina-ba.com/time-intelligence-dynamic-legend-in-line-charts/

//this script creates an extra calculation group to work together with Time Calculation group 
//you need to create the Time calculation group script with the dynamic label measures before running this script 
//the names of the measure and affected measure table must match 
//if you changed the default valures on the time intel calc group, change them heere too .


var ts = Model.Tables.Where(x => x.GetAnnotation("@AgulloBernat") == "Time Intel Calc Group");

var timeIntelCalcGroup = null as CalculationGroupTable; 

if (ts.Count() == 1 ) {
    timeIntelCalcGroup = ts.First() as CalculationGroupTable;
} else if (ts.Count() < 1) {
    Error("Time Itelligence Calc group script by @AgulloBernat has not been successfuly executed yet. Execute it first and then run again the present script"); 
    return; 
} else { 
    //this should never happen -- who needs two calc groups for time intelligence? 
    timeIntelCalcGroup = SelectTable(ts, label:"Select your existing time intelligence calc group calculation group table:") as CalculationGroupTable;
};

if (timeIntelCalcGroup == null) { return; } // doesn't work in TE3 as cancel button doesn't return null in TE3


//init Affected Measure Table
ts = Model.Tables.Where(x => x.GetAnnotation("@AgulloBernat") == "Time Intel Affected Measures Table");

var affectedMeasuresTable = null as Table; 

if (ts.Count() == 1 ) {
    affectedMeasuresTable = ts.First(); 
} else if (ts.Count() < 1) {
    Error("Time Itelligence Calc group script by @AgulloBernat has not been successfuly executed yet. Execute it first and then run again the present script"); 
    return; 
} else { 
    //this should never happen -- who needs two time intelligence affected measures calc tables? 
    affectedMeasuresTable = SelectTable(ts, label:"Select your existing time intelligence affected measures table:") as CalculationGroupTable;
};

if (affectedMeasuresTable == null) { return; } // doesn't work in TE3 as cancel button doesn't return null in TE3

string labelsCalculationGroupName = Interaction.InputBox("Provide a name for the dynamic labels Calc Group", "Calc Group Name", "Labels", 740, 400);
if(labelsCalculationGroupName == "") return;

string labelsCalculationGroupColumnName = Interaction.InputBox("Provide a name for the column of the Calc Group", "Calc Group Name", labelsCalculationGroupName, 740, 400);
if(labelsCalculationGroupColumnName == "") return;

string labelsCalculationItemName = "Last Point Time Calculation"; 

string affectedMeasuresTableName = "Time Intelligence Affected Measures"; //affectedMeasuresTable.Name; 
string affectedMeasuresColumnName = "Measure"; // affectedMeasuresTable.Columns[0].Name; 

//add the name of the existing time intel calc group here
string calcGroupName = "Time Intelligence";

//add the name for date table of the model
string dateTableName = "Date";
string dateTableDateColumnName = "Date";

string labelAsValueMeasureName = "Label as Value Measure"; 

string flagExpression = "UNICHAR( 8204 )"; 


//generates new calc group 
var calculationGroupTable1 = (Model.AddCalculationGroup(labelsCalculationGroupName) as CalculationGroupTable);

calculationGroupTable1.Description = "Calculation group to manipulate data labels"; 

//sees the default precedence number assigned 
int labelGroupPrecedence = (Model.Tables[labelsCalculationGroupName] as CalculationGroupTable).CalculationGroup.Precedence;
int timeIntelGroupPrecedence = (Model.Tables[calcGroupName] as CalculationGroupTable).CalculationGroup.Precedence;

//if time intel has lower precedence... 
if(labelGroupPrecedence > timeIntelGroupPrecedence) {
    //...swap precedence values 
    (Model.Tables[labelsCalculationGroupName] as CalculationGroupTable).CalculationGroup.Precedence = timeIntelGroupPrecedence;
    (Model.Tables[calcGroupName] as CalculationGroupTable).CalculationGroup.Precedence = labelGroupPrecedence; 
}; 


(Model.Tables["Labels"].Columns["Name"] as DataColumn).Name = labelsCalculationGroupColumnName;
var calculationItem1 = calculationGroupTable1.AddCalculationItem(labelsCalculationItemName);
calculationItem1.Expression = "SELECTEDMEASURE()";
calculationItem1.FormatStringExpression =
"SWITCH(" + 
"\n    TRUE()," + 
"\n    SELECTEDMEASURENAME()" + 
"\n        IN VALUES( '" + affectedMeasuresTableName + "'[" + affectedMeasuresColumnName + "] )," + 
"\n        VAR maxDateInVisual =" + 
"\n            CALCULATE( MAX( '" + dateTableName + "'[" +dateTableDateColumnName + "] ), ALLSELECTED( '" + dateTableName + "' ) )" + 
"\n        VAR maxDateInDataPoint =" + 
"\n            MAX( '" + dateTableName + "'[" + dateTableDateColumnName + "] )" + 
"\n        VAR result =" + 
"\n            IF( maxDateInDataPoint = maxDateInVisual, [" + labelAsValueMeasureName +"] )" + 
"\n        RETURN" + 
"\n           " + flagExpression + " & \"\"\"\" & result & \"\"\";\"\"\" & result & \"\"\";\"\"\" & result & \"\"\";\"\"\" & result & \"\"\"\"," + 
"\n    SELECTEDMEASUREFORMATSTRING()" + 
"\n)";

calculationItem1.Description = "Show dynamic label as data label of the last point in a line series over a time axis"; 

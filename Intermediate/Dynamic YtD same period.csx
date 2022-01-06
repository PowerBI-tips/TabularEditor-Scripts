//
//
// by Tommy Puglia
// twitter: @tommypuglia
// pugliabi.com
//
// REFERENCE: 
// based on this article https://www.daxpatterns.com/standard-time-related-calculations/
// 
// use this to create YtD, PY YtD, and YOY based on same period in the current year
//
//AT LEAST ONE MEASURE HAS TO BE AFFECTED!, 
//either by selecting it or typing its name in the preSelectedMeasures Variable
// CONFIGURATION:
//   1. Update the values from the comments (prefactTableName - predateTableYearColumnName) if you want the script to auto run based on set table
//   column names.
//   2. You Should put  in prefactTableName, the name of the fact table only. Then the column where date is in the fact table, etc. 
//   3. When you run the script, YOU MUST SELECT AT LEAST ONE MEASURE.
//   4. Running the script will show a pop up to enter in the Display Folder you want to group all of the selected measures
//   5. The next pop up will ask you if you want to choose the tables, columns manually or use what is pre-configured in the code. Note if you
//	choose "N" (default) and there are no value filled in the variables below, it will not work.
//	Let it Run! 
 
string[] preSelectedMeasures = {}; //include measure names in double quotes, like: {"Profit","Total Cost"};
string prefactTableName = "afactOrderDetails";
string prefactTableDateColumnName = "Payment_Date";
string predateTableName = "DimDateDF";
string predateTableDateColumnName = "FullDate";
string predateTableYearColumnName = "Calendar Year";
string affectedMeasures = "{";
string[] AllDATES = {
	prefactTableName,
	prefactTableDateColumnName,
	predateTableName,
	predateTableDateColumnName,
	predateTableYearColumnName
};
string factTableName = string.Empty;
string factTableDateColumnName = string.Empty;
string factDateColumnWithTable = string.Empty;
string dateTableName = string.Empty;
string dateTableDateColumnName = string.Empty;
string dateTableYearColumnName = string.Empty;
string dateColumnWithTable = string.Empty;
string ShowValueForDatesMeasureName = string.Empty;
string dateWithSalesColumnName = string.Empty;
// string dateTable;

// Select Tables
var factTable = SelectTable(label: "Select your fact table");
if (factTable == null) return;
var factTableDateColumn = SelectColumn(factTable.Columns, label: "Select the main date column");
if (factTableDateColumn == null) return;

var dateTable = SelectTable(label: "Select your date table");
if (dateTable == null) {
	Error("You just aborted the script");
	return;
} else {

	dateTable.SetAnnotation("@AgulloBernat", "Time Intel Date Table");
};
ShowValueForDatesMeasureName = "ShowValueForDates";
dateWithSalesColumnName = "DateWith" + factTable.Name;
string ADYname = "ADY";
var dateTableDateColumn = SelectColumn(dateTable.Columns, label: "Select the date column");
if (dateTableDateColumn == null) {
	Error("You just aborted the script");
	return;
} else {
	dateTableDateColumn.SetAnnotation("@AgulloBernat", "Time Intel Date Table Date Column");
};

var dateTableYearColumn = SelectColumn(dateTable.Columns, label: "Select the year column");
if (dateTableYearColumn == null) return;

//these names are for internal use only, so no need to be super-fancy, better stick to datpatterns.com mode
factTableName = factTable.Name;
factTableDateColumnName = factTableDateColumn.Name;
factDateColumnWithTable = "'" + factTableName + "'[" + factTableDateColumnName + "]";
dateTableName = dateTable.Name;
dateTableDateColumnName = dateTableDateColumn.Name;
dateTableYearColumnName = dateTableYearColumn.Name;
dateColumnWithTable = "'" + dateTableName + "'[" + dateTableDateColumnName + "]";
ShowValueForDatesMeasureName = "ShowValueForDates";
dateWithSalesColumnName = "DateWith" + factTableName;

// The logic
/*
int i = 0;
int b = 0;

for (i = 0; i <  AllDATES.GetLength(0); i++) {
if(AllDATES[i]  != "") {
 b++;
};
};


for (i = 0; i < preSelectedMeasures.GetLength(0); i++) {

	if (affectedMeasures == "{") {
		affectedMeasures = affectedMeasures + "\"" + preSelectedMeasures[i] + "\"";
	} else {
		affectedMeasures = affectedMeasures + ",\"" + preSelectedMeasures[i] + "\"";
	};

};

if (Selected.Measures.Count != 0) {

	foreach(var m in Selected.Measures) {
		if (affectedMeasures == "{") {
			affectedMeasures = affectedMeasures + "\"" + m.Name + "\"";
		} else {
			affectedMeasures = affectedMeasures + ",\"" + m.Name + "\"";
		};
	};
};


//check that by either method at least one measure is affected
// '2021-09-24 / B.Agullo / model object selection prompts! 

if(b == 0)
{
var factTable = SelectTable(label: "Select your fact table");
if (factTable == null) return;
var factTableDateColumn = SelectColumn(factTable.Columns, label: "Select the main date column");
if (factTableDateColumn == null) return;

var vdateTable = SelectTable(label: "Select your date table");
if (vdateTable == null) {
	Error("You just aborted the script");
	return;
} else {
dateTable = vdateTable;
	dateTable.SetAnnotation("@AgulloBernat", "Time Intel Date Table");
};
ShowValueForDatesMeasureName = "ShowValueForDates";
dateWithSalesColumnName = "DateWith" + factTable.Name;
string ADYname = "ADY";
var dateTableDateColumn = SelectColumn(dateTable.Columns, label: "Select the date column");
if (dateTableDateColumn == null) {
	Error("You just aborted the script");
	return;
} else {
	dateTableDateColumn.SetAnnotation("@AgulloBernat", "Time Intel Date Table Date Column");
};

var dateTableYearColumn = SelectColumn(dateTable.Columns, label: "Select the year column");
if (dateTableYearColumn == null) return;

//these names are for internal use only, so no need to be super-fancy, better stick to datpatterns.com model


 factTableName = factTable.Name;
 factTableDateColumnName = factTableDateColumn.Name;
 factDateColumnWithTable = "'" + factTableName + "'[" + factTableDateColumnName + "]";
 dateTableName = dateTable.Name;
 dateTableDateColumnName = dateTableDateColumn.Name;
 dateTableYearColumnName = dateTableYearColumn.Name;
 dateColumnWithTable = "'" + dateTableName + "'[" + dateTableDateColumnName + "]";
} else {
 factTableName = prefactTableName;
dateTable =  predateTableName;
 factTableDateColumnName = prefactTableDateColumnName;
 factDateColumnWithTable = "'" + factTableName + "'[" + factTableDateColumnName + "]";
 dateTableName = predateTableName;
 dateTableDateColumnName = predateTableDateColumnName;
 dateTableYearColumnName = predateTableYearColumnName;
 dateColumnWithTable = "'" + dateTableName + "'[" + dateTableDateColumnName + "]";
 ShowValueForDatesMeasureName = "ShowValueForDates";
 dateWithSalesColumnName = "DateWith" + factTableName;
string ADYname = "ADY";
};
*/
string DateWithSalesCalculatedColumnExpression = dateColumnWithTable + " <= MAX ( " + factDateColumnWithTable + ")";

dateTable.AddCalculatedColumn(dateWithSalesColumnName, DateWithSalesCalculatedColumnExpression);

string ShowValueForDatesMeasureExpression = "VAR LastDateWithData = " + "    CALCULATE ( " + "        MAX (  " + factDateColumnWithTable + " ), " + "        REMOVEFILTERS () " + "    )" + "VAR FirstDateVisible = " + "    MIN ( " + dateColumnWithTable + " ) " + "VAR Result = " + "    FirstDateVisible <= LastDateWithData " + "RETURN " + "    Result ";

var ShowValueForDatesMeasure = dateTable.AddMeasure(ShowValueForDatesMeasureName, ShowValueForDatesMeasureExpression);

ShowValueForDatesMeasure.FormatDax();

string ShowADY = "VAR _LSD = " + "    MAX (  " + factDateColumnWithTable + " ) " + " VAR _LSDPY = " + "    EDATE( " + "        _LSD, " + "        - 12 " + "    )" + " RETURN " + "    " + dateColumnWithTable + "<= _LSDPY ";

var ShowValueForDatesADY = dateTable.AddCalculatedColumn(ADYname, ShowADY);

ShowValueForDatesMeasure.FormatDax();

string YTDName = " YtD";
string PYYTDName = " PY YtD";
string YOYName = " YoY";

string YTDExpressionFirstPart = "Calculate( ";
string YTDEXPRENextPart = " , DATESYTD(" + dateColumnWithTable + "))";

string PYTEXPFirst = " VAR LastDaySelection = " + "    LASTNONBLANK( " + factDateColumnWithTable + ", ";
string firstBracket = "[";
string PYLMEA = "]";
string FinishPY = ")" + " VAR CurrentRange = " + "    DATESBETWEEN( " + dateColumnWithTable + ", " + "        MIN( " + factDateColumnWithTable + " ) , " + "        LastDaySelection )" + " VAR PreviousRange = " + "    SAMEPERIODLASTYEAR( CurrentRange ) " + "RETURN " + " IF(LastDaySelection " + "            >= MIN( " + dateColumnWithTable + " )," + "        CALCULATE( " + firstBracket;
string FinalPiech = PYLMEA + " , " + "            PreviousRange, " + "        " + dateTableName + "[ADY] = TRUE ))";

foreach(var mdd in Selected.Measures) {
	string df = "__" + mdd.Name;
	var MESN = mdd.Table.AddMeasure(
	mdd.Name + YTDName, YTDExpressionFirstPart + mdd.DaxObjectName + YTDEXPRENextPart);
	var PYTN = mdd.Table.AddMeasure(
	mdd.Name + PYYTDName, PYTEXPFirst + firstBracket + MESN.Name + PYLMEA + FinishPY + mdd.Name + FinalPiech);
	var YOYNNN = mdd.Table.AddMeasure(
	mdd.Name + YOYName, "Divide((" + MESN.DaxObjectName + " - " + PYTN.DaxObjectName + "), " + PYTN.DaxObjectName + ")");
	MESN.DisplayFolder = df;
	PYTN.DisplayFolder = df;
	YOYNNN.DisplayFolder = df;
	MESN.FormatDax();
	PYTN.FormatDax();
	YOYNNN.FormatDax();
}
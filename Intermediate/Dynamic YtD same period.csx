#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
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

//AT LEAST ONE MEASURE HAS TO BE AFFECTED!, 
//either by selecting it or typing its name in the preSelectedMeasures Variable

string DisplayFolderName = Interaction.InputBox("Provide the name of the Display Folder", "Display", "KPI");
if (DisplayFolderName == "") return;

string[] preSelectedMeasures = {}; //include measure names in double quotes, like: {"Profit","Total Cost"};
string prefactTableName = "afactKiboMerge";
string prefactTableDateColumnName = "PaymentDate";
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
string factTableName = prefactTableName;
string dateTableName = predateTableName;
string factTableDateColumnName = prefactTableDateColumnName;
string dateTableDateColumnName = predateTableDateColumnName;
string dateTableYearColumnName = predateTableYearColumnName;
string ADYname = "ADY";
string ShowValueForDatesMeasureName = "ShowValueForDates";
string dateTableNameV = dateTableName;

 string ValueSelected = Interaction.InputBox("Choose if you want to Select", "Select Columns", "Y");
string ValueSelected = "N";
if (ValueSelected == "N") {
	var factTablea = Model.Tables[factTableName];
	var dateTablea = Model.Tables[dateTableName];

	var  = Model.Tables[factTableName].Columns[factTableDateColumnName];

	var dateTableDateColumn = Model.Tables[dateTableName].Columns[dateTableDateColumnName];
	var dateTableYearColumn = Model.Tables[dateTableName].Columns[dateTableYearColumnName];
} else {
	// Select Tables
	var factTablen = SelectTable(label: "Select your fact table");
	if (factTablen == null) return;
	var factTableDateColumn = SelectColumn(factTablen.Columns, label: "Select the main date column");
	if (factTableDateColumn == null) return;

	var dateTablen = SelectTable(label: "Select your date table");
	if (dateTablen == null) {
		Error("You just aborted the script");
		return;
	};

	var dateTableDateColumn = SelectColumn(dateTablen.Columns, label: "Select the date column");
	if (dateTableDateColumn == null) {
		Error("You just aborted the script");
		return;
	};
	var dateTableYearColumn = SelectColumn(dateTablen.Columns, label: "Select the year column");
	if (dateTableYearColumn == null) return;
	factTableName = factTablen.Name;
	factTableDateColumnName = factTableDateColumn.Name;
	dateTableName = dateTablen.Name;
	dateTableDateColumnName = dateTableDateColumn.Name;
	dateTableYearColumnName = dateTableYearColumn.Name;
};

var factDateColumnWithTableNA = Model.Tables[factTableName].Columns[factTableDateColumnName];
var dateTable = Model.Tables[dateTableName];
var factTable = Model.Tables[factTableName];
var dateColumnWithTableNA = Model.Tables[dateTableName].Columns[dateTableDateColumnName];
var dateColumnWithTable = dateColumnWithTableNA.DaxObjectFullName;
var factDateColumnWithTable = factDateColumnWithTableNA.DaxObjectFullName;

string dateWithSalesColumnName = "DateWith" + factTable.Name;
string DateWithSalesCalculatedColumnExpression = dateColumnWithTable + " <= MAX ( " + factDateColumnWithTable + ")";
string ShowValueForDatesMeasureExpression = "VAR LastDateWithData = " + "    CALCULATE ( " + "        MAX (  " + factDateColumnWithTable + " ), " + "        REMOVEFILTERS () " + "    )" + "VAR FirstDateVisible = " + "    MIN ( " + dateColumnWithTable + " ) " + "VAR Result = " + "    FirstDateVisible <= LastDateWithData " + "RETURN " + "    Result ";
string ShowADY = "VAR _LSD = " + "    MAX (  " + factDateColumnWithTable + " ) " + " VAR _LSDPY = " + "    EDATE( " + "        _LSD, " + "        - 12 " + "    )" + " RETURN " + "    " + dateColumnWithTable + "<= _LSDPY ";

if (!Model.Tables[dateTableName].Columns.Contains(ADYname)) {
	var ShowValueForDatesADY = dateTable.AddCalculatedColumn(ADYname, ShowADY);

	ShowValueForDatesADY.FormatDax();
};
if (!Model.Tables[dateTableName].Columns.Contains(dateWithSalesColumnName)) {

	var AddDateSales = dateTable.AddCalculatedColumn(dateWithSalesColumnName, DateWithSalesCalculatedColumnExpression);
	AddDateSales.FormatDax();
}
if (!Model.Tables[dateTableName].Measures.Contains(ShowValueForDatesMeasureName)) {
	var ShowValueForDatesMeasure = dateTable.AddMeasure(ShowValueForDatesMeasureName, ShowValueForDatesMeasureExpression);

	ShowValueForDatesMeasure.FormatDax();
};

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
	string df = "__" + DisplayFolderName;
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
};

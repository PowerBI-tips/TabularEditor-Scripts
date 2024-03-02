#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
                //
// CHANGELOG:
// '2021-05-01 / B.Agullo / 
// '2021-05-17 / B.Agullo / added affected measure table
// '2021-06-19 / B.Agullo / data label measures
// '2021-07-10 / B.Agullo / added flag expression to avoid breaking already special format strings
// '2021-09-23 / B.Agullo / added code to prompt for parameters (code credit to Daniel Otykier) 
// '2021-09-27 / B.Agullo / added code for general name 
// '2022-10-11 / B.Agullo / added MMT and MWT calc item groups
// '2023-01-24 / B.Agullo / added Date Range Measure and completed dynamic label for existing items
//
// by Bernat Agulló
// twitter: @AgulloBernat
// www.esbrina-ba.com/blog
//
// REFERENCE: 
// Check out https://www.esbrina-ba.com/time-intelligence-the-smart-way/ where this script is introduced
// 
// FEATURED: 
// this script featured in GuyInACube https://youtu.be/_j0iTUo2HT0
//
// THANKS:
// shout out to Johnny Winter for the base script and SQLBI for daxpatterns.com

//select the measures that you want to be affected by the calculation group
//before running the script. 
//measure names can also be included in the following array (no need to select them) 
string[] preSelectedMeasures = { }; //include measure names in double quotes, like: {"Profit","Total Cost"};

//AT LEAST ONE MEASURE HAS TO BE AFFECTED!, 
//either by selecting it or typing its name in the preSelectedMeasures Variable



//
// ----- do not modify script below this line -----
//


string affectedMeasures = "{";

int i = 0;

for (i = 0; i < preSelectedMeasures.GetLength(0); i++)
{

    if (affectedMeasures == "{")
    {
        affectedMeasures = affectedMeasures + "\"" + preSelectedMeasures[i] + "\"";
    }
    else
    {
        affectedMeasures = affectedMeasures + ",\"" + preSelectedMeasures[i] + "\"";
    };

};


if (Selected.Measures.Count != 0)
{

    foreach (var m in Selected.Measures)
    {
        if (affectedMeasures == "{")
        {
            affectedMeasures = affectedMeasures + "\"" + m.Name + "\"";
        }
        else
        {
            affectedMeasures = affectedMeasures + ",\"" + m.Name + "\"";
        };
    };
};

//check that by either method at least one measure is affected
if (affectedMeasures == "{")
{
    Error("No measures affected by calc group");
    return;
};

string calcGroupName = String.Empty;
string columnName = String.Empty;

if (Model.CalculationGroups.Any(cg => cg.GetAnnotation("@AgulloBernat") == "Time Intel Calc Group"))
{
    calcGroupName = Model.CalculationGroups.Where(cg => cg.GetAnnotation("@AgulloBernat") == "Time Intel Calc Group").First().Name;

}
else
{
    calcGroupName = Interaction.InputBox("Provide a name for your Calc Group", "Calc Group Name", "Time Intelligence", 740, 400);
};

if (calcGroupName == String.Empty) return;


if (Model.CalculationGroups.Any(cg => cg.GetAnnotation("@AgulloBernat") == "Time Intel Calc Group"))
{
    columnName = Model.Tables.Where(cg => cg.GetAnnotation("@AgulloBernat") == "Time Intel Calc Group").First().Columns.First().Name;

}
else
{
    columnName = Interaction.InputBox("Provide a name for your Calc Group Column", "Calc Group Column Name", calcGroupName, 740, 400);
};

if (columnName == String.Empty) return;

string affectedMeasuresTableName = String.Empty;

if (Model.Tables.Any(t => t.GetAnnotation("@AgulloBernat") == "Time Intel Affected Measures Table"))
{
    affectedMeasuresTableName = Model.Tables.Where(t => t.GetAnnotation("@AgulloBernat") == "Time Intel Affected Measures Table").First().Name;

}
else
{
    affectedMeasuresTableName = Interaction.InputBox("Provide a name for affected measures table", "Affected Measures Table Name", calcGroupName + " Affected Measures", 740, 400);

};

if (affectedMeasuresTableName ==String.Empty) return;


string affectedMeasuresColumnName = String.Empty;

if (Model.Tables.Any(t => t.GetAnnotation("@AgulloBernat") == "Time Intel Affected Measures Table"))
{
    affectedMeasuresColumnName = Model.Tables.Where(t => t.GetAnnotation("@AgulloBernat") == "Time Intel Affected Measures Table").First().Columns.First().Name;

}
else
{
    affectedMeasuresColumnName = Interaction.InputBox("Provide a name for affected measures column", "Affected Measures Table Column Name", "Measure", 740, 400);

};

if (affectedMeasuresColumnName == String.Empty) return;
//string affectedMeasuresColumnName = "Measure"; 

string labelAsValueMeasureName = "Label as Value Measure";
string labelAsFormatStringMeasureName = "Label as format string";


// '2021-09-24 / B.Agullo / model object selection prompts! 
var factTable = SelectTable(label: "Select your fact table");
if (factTable == null) return;

var factTableDateColumn = SelectColumn(factTable.Columns, label: "Select the main date column");
if (factTableDateColumn == null) return;

Table dateTableCandidate = null;

if (Model.Tables.Any
    (x => x.GetAnnotation("@AgulloBernat") == "Time Intel Date Table"
        || x.Name == "Date"
        || x.Name == "Calendar"))
{
    dateTableCandidate = Model.Tables.Where
        (x => x.GetAnnotation("@AgulloBernat") == "Time Intel Date Table"
            || x.Name == "Date"
            || x.Name == "Calendar").First();

};

var dateTable =
    SelectTable(
        label: "Select your date table",
        preselect: dateTableCandidate);

if (dateTable == null)
{
    Error("You just aborted the script");
    return;
}
else
{
    dateTable.SetAnnotation("@AgulloBernat", "Time Intel Date Table");
};


Column dateTableDateColumnCandidate = null;

if (dateTable.Columns.Any
            (x => x.GetAnnotation("@AgulloBernat") == "Time Intel Date Table Date Column" || x.Name == "Date"))
{
    dateTableDateColumnCandidate = dateTable.Columns.Where
        (x => x.GetAnnotation("@AgulloBernat") == "Time Intel Date Table Date Column" || x.Name == "Date").First();
};

var dateTableDateColumn =
    SelectColumn(
        dateTable.Columns,
        label: "Select the date column",
        preselect: dateTableDateColumnCandidate);

if (dateTableDateColumn == null)
{
    Error("You just aborted the script");
    return;
}
else
{
    dateTableDateColumn.SetAnnotation("@AgulloBernat", "Time Intel Date Table Date Column");
};

Column dateTableYearColumnCandidate = null;
if (dateTable.Columns.Any(x => x.GetAnnotation("@AgulloBernat") == "Time Intel Date Table Year Column" || x.Name == "Year"))
{
    dateTable.Columns.Where
        (x => x.GetAnnotation("@AgulloBernat") == "Time Intel Date Table Year Column" || x.Name == "Year").First();
};

var dateTableYearColumn =
    SelectColumn(
        dateTable.Columns,
        label: "Select the year column",
        preselect: dateTableYearColumnCandidate);

if (dateTableYearColumn == null)
{
    Error("You just abourted the script");
    return;
}
else
{
    dateTableYearColumn.SetAnnotation("@AgulloBernat", "Time Intel Date Table Year Column");
};


//these names are for internal use only, so no need to be super-fancy, better stick to datpatterns.com model
string ShowValueForDatesMeasureName = "ShowValueForDates";
string dateWithSalesColumnName = "DateWith" + factTable.Name;

// '2021-09-24 / B.Agullo / I put the names back to variables so I don't have to tough the script
string factTableName = factTable.Name;
string factTableDateColumnName = factTableDateColumn.Name;
string dateTableName = dateTable.Name;
string dateTableDateColumnName = dateTableDateColumn.Name;
string dateTableYearColumnName = dateTableYearColumn.Name;

// '2021-09-24 / B.Agullo / this is for internal use only so better leave it as is 
string flagExpression = "UNICHAR( 8204 )";

string calcItemProtection = "<CODE>"; //default value if user has selected no measures
string calcItemFormatProtection = "<CODE>"; //default value if user has selected no measures

// check if there's already an affected measure table
if (Model.Tables.Any(t => t.GetAnnotation("@AgulloBernat") == "Time Intel Affected Measures Table"))
{
    //modifying an existing calculated table is not risk-free
    Info("Make sure to include measure names to the table " + affectedMeasuresTableName);
}
else
{
    // create calculated table containing all names of affected measures
    // this is why you need to enable 
    if (affectedMeasures != "{")
    {

        affectedMeasures = affectedMeasures + "}";

        string affectedMeasureTableExpression =
            "SELECTCOLUMNS(" + affectedMeasures + ",\"" + affectedMeasuresColumnName + "\",[Value])";

        var affectedMeasureTable =
            Model.AddCalculatedTable(affectedMeasuresTableName, affectedMeasureTableExpression);

        affectedMeasureTable.FormatDax();
        affectedMeasureTable.Description =
            "Measures affected by " + calcGroupName + " calculation group.";

        affectedMeasureTable.SetAnnotation("@AgulloBernat", "Time Intel Affected Measures Table");

        // this causes error
        // affectedMeasureTable.Columns[affectedMeasuresColumnName].SetAnnotation("@AgulloBernat","Time Intel Affected Measures Table Column");

        affectedMeasureTable.IsHidden = true;

    };
};

//if there where selected or preselected measures, prepare protection code for expresion and formatstring
string affectedMeasuresValues = "VALUES('" + affectedMeasuresTableName + "'[" + affectedMeasuresColumnName + "])";

calcItemProtection =
    "SWITCH(" +
    "   TRUE()," +
    "   SELECTEDMEASURENAME() IN " + affectedMeasuresValues + "," +
    "   <CODE> ," +
    "   ISSELECTEDMEASURE([" + labelAsValueMeasureName + "])," +
    "   <LABELCODE> ," +
    "   SELECTEDMEASURE() " +
    ")";


calcItemFormatProtection =
    "SWITCH(" +
    "   TRUE() ," +
    "   SELECTEDMEASURENAME() IN " + affectedMeasuresValues + "," +
    "   <CODE> ," +
    "   ISSELECTEDMEASURE([" + labelAsFormatStringMeasureName + "])," +
    "   <LABELCODEFORMATSTRING> ," +
    "   SELECTEDMEASUREFORMATSTRING() " +
    ")";


string dateColumnWithTable = "'" + dateTableName + "'[" + dateTableDateColumnName + "]";
string yearColumnWithTable = "'" + dateTableName + "'[" + dateTableYearColumnName + "]";
string factDateColumnWithTable = "'" + factTableName + "'[" + factTableDateColumnName + "]";
string dateWithSalesWithTable = "'" + dateTableName + "'[" + dateWithSalesColumnName + "]";
string calcGroupColumnWithTable = "'" + calcGroupName + "'[" + columnName + "]";

//check to see if a table with this name already exists
//if it doesnt exist, create a calculation group with this name
if (!Model.Tables.Contains(calcGroupName))
{
    var cg = Model.AddCalculationGroup(calcGroupName);
    cg.Description = "Calculation group for time intelligence. Availability of data is taken from " + factTableName + ".";
    cg.SetAnnotation("@AgulloBernat", "Time Intel Calc Group");
};

//set variable for the calc group
Table calcGroup = Model.Tables[calcGroupName];

//if table already exists, make sure it is a Calculation Group type
if (calcGroup.SourceType.ToString() != "CalculationGroup")
{
    Error("Table exists in Model but is not a Calculation Group. Rename the existing table or choose an alternative name for your Calculation Group.");
    return;
};

//adds the two measures that will be used for label as value, label as format string 
var labelAsValueMeasure = calcGroup.AddMeasure(labelAsValueMeasureName, "");
labelAsValueMeasure.Description = "Use this measure to show the year evaluated in tables";

var labelAsFormatStringMeasure = calcGroup.AddMeasure(labelAsFormatStringMeasureName, "0");
labelAsFormatStringMeasure.Description = "Use this measure to show the year evaluated in charts";

Measure dateRangeMeasure = calcGroup.AddMeasure("Date Range", expression: @"FORMAT( MIN( 'Date'[Date] ), ""d-MMM-yy"", ""en-US"" ) & "" to ""
        & FORMAT( MAX( 'Date'[Date] ), ""d-MMM-yy"", ""en-US"" )");

dateRangeMeasure.Description = "This measure is used to display the dynamic label of Moving total calc items. Do not delete.";


//by default the calc group has a column called Name. If this column is still called Name change this in line with specfied variable
if (calcGroup.Columns.Contains("Name"))
{
    calcGroup.Columns["Name"].Name = columnName;

};

calcGroup.Columns[columnName].Description = "Select value(s) from this column to apply time intelligence calculations.";
calcGroup.Columns[columnName].SetAnnotation("@AgulloBernat", "Time Intel Calc Group Column");


//Only create them if not in place yet (reruns)
if (!Model.Tables[dateTableName].Columns.Any(C => C.GetAnnotation("@AgulloBernat") == "Date with Data Column"))
{
    string DateWithSalesCalculatedColumnExpression =
        dateColumnWithTable + " <= MAX ( " + factDateColumnWithTable + ")";

    Column dateWithDataColumn = dateTable.AddCalculatedColumn(dateWithSalesColumnName, DateWithSalesCalculatedColumnExpression);
    dateWithDataColumn.SetAnnotation("@AgulloBernat", "Date with Data Column");
};

if (!Model.Tables[dateTableName].Measures.Any(M => M.Name == ShowValueForDatesMeasureName))
{
    string ShowValueForDatesMeasureExpression =
        "VAR LastDateWithData = " +
        "    CALCULATE ( " +
        "        MAX (  " + factDateColumnWithTable + " ), " +
        "        REMOVEFILTERS () " +
        "    )" +
        "VAR FirstDateVisible = " +
        "    MIN ( " + dateColumnWithTable + " ) " +
        "VAR Result = " +
        "    FirstDateVisible <= LastDateWithData " +
        "RETURN " +
        "    Result ";

    var ShowValueForDatesMeasure = dateTable.AddMeasure(ShowValueForDatesMeasureName, ShowValueForDatesMeasureExpression);

    ShowValueForDatesMeasure.FormatDax();
};



//defining expressions and formatstring for each calc item
string CY =
    "/*CY*/ " +
    "SELECTEDMEASURE()";

string CYlabel =
    "SELECTEDVALUE(" + yearColumnWithTable + ")";


string PY =
    "/*PY*/ " +
    "IF (" +
    "    [" + ShowValueForDatesMeasureName + "], " +
    "    CALCULATE ( " +
    "        " + CY + ", " +
    "        CALCULATETABLE ( " +
    "            DATEADD ( " + dateColumnWithTable + " , -1, YEAR ), " +
    "            " + dateWithSalesWithTable + " = TRUE " +
    "        ) " +
    "    ) " +
    ") ";


string PYlabel =
    "/*PY*/ " +
    "IF (" +
    "    [" + ShowValueForDatesMeasureName + "], " +
    "    CALCULATE ( " +
    "        " + CYlabel + ", " +
    "        CALCULATETABLE ( " +
    "            DATEADD ( " + dateColumnWithTable + " , -1, YEAR ), " +
    "            " + dateWithSalesWithTable + " = TRUE " +
    "        ) " +
    "    ) " +
    ") ";


string YOY =
    "/*YOY*/ " +
    "VAR ValueCurrentPeriod = " + CY + " " +
    "VAR ValuePreviousPeriod = " + PY + " " +
    "VAR Result = " +
    "IF ( " +
    "    NOT ISBLANK ( ValueCurrentPeriod ) && NOT ISBLANK ( ValuePreviousPeriod ), " +
    "     ValueCurrentPeriod - ValuePreviousPeriod" +
    " ) " +
    "RETURN " +
    "   Result ";

string YOYlabel =
    "/*YOY*/ " +
    "VAR ValueCurrentPeriod = " + CYlabel + " " +
    "VAR ValuePreviousPeriod = " + PYlabel + " " +
    "VAR Result = " +
    "IF ( " +
    "    NOT ISBLANK ( ValueCurrentPeriod ) && NOT ISBLANK ( ValuePreviousPeriod ), " +
    "     ValueCurrentPeriod & \" vs \" & ValuePreviousPeriod" +
    " ) " +
    "RETURN " +
    "   Result ";

string YOYpct =
    "/*YOY%*/ " +
   "VAR ValueCurrentPeriod = " + CY + " " +
    "VAR ValuePreviousPeriod = " + PY + " " +
    "VAR CurrentMinusPreviousPeriod = " +
    "IF ( " +
    "    NOT ISBLANK ( ValueCurrentPeriod ) && NOT ISBLANK ( ValuePreviousPeriod ), " +
    "     ValueCurrentPeriod - ValuePreviousPeriod" +
    " ) " +
    "VAR Result = " +
    "DIVIDE ( " +
    "    CurrentMinusPreviousPeriod," +
    "    ValuePreviousPeriod" +
    ") " +
    "RETURN " +
    "  Result";

string YOYpctLabel =
    "/*YOY%*/ " +
   "VAR ValueCurrentPeriod = " + CYlabel + " " +
    "VAR ValuePreviousPeriod = " + PYlabel + " " +
    "VAR Result = " +
    "IF ( " +
    "    NOT ISBLANK ( ValueCurrentPeriod ) && NOT ISBLANK ( ValuePreviousPeriod ), " +
    "     ValueCurrentPeriod & \" vs \" & ValuePreviousPeriod & \" (%)\"" +
    " ) " +
    "RETURN " +
    "  Result";

string YTD =
    "/*YTD*/" +
    "IF (" +
    "    [" + ShowValueForDatesMeasureName + "]," +
    "    CALCULATE (" +
    "        " + CY + "," +
    "        DATESYTD (" + dateColumnWithTable + " )" +
    "   )" +
    ") ";


string YTDlabel = CYlabel + "& \" YTD\"";


string PYTD =
    "/*PYTD*/" +
    "IF ( " +
    "    [" + ShowValueForDatesMeasureName + "], " +
    "   CALCULATE ( " +
    "       " + YTD + "," +
    "    CALCULATETABLE ( " +
    "        DATEADD ( " + dateColumnWithTable + ", -1, YEAR ), " +
    "       " + dateWithSalesWithTable + " = TRUE " +
    "       )" +
    "   )" +
    ") ";

string PYTDlabel = PYlabel + "& \" YTD\"";


string YOYTD =
    "/*YOYTD*/" +
    "VAR ValueCurrentPeriod = " + YTD + " " +
    "VAR ValuePreviousPeriod = " + PYTD + " " +
    "VAR Result = " +
    "IF ( " +
    "    NOT ISBLANK ( ValueCurrentPeriod ) && NOT ISBLANK ( ValuePreviousPeriod ), " +
    "     ValueCurrentPeriod - ValuePreviousPeriod" +
    " ) " +
    "RETURN " +
    "   Result ";


string YOYTDlabel =
    "/*YOYTD*/" +
    "VAR ValueCurrentPeriod = " + YTDlabel + " " +
    "VAR ValuePreviousPeriod = " + PYTDlabel + " " +
    "VAR Result = " +
    "IF ( " +
    "    NOT ISBLANK ( ValueCurrentPeriod ) && NOT ISBLANK ( ValuePreviousPeriod ), " +
    "     ValueCurrentPeriod & \" vs \" & ValuePreviousPeriod" +
    " ) " +
    "RETURN " +
    "   Result ";



string YOYTDpct =
    "/*YOYTD%*/" +
    "VAR ValueCurrentPeriod = " + YTD + " " +
    "VAR ValuePreviousPeriod = " + PYTD + " " +
    "VAR CurrentMinusPreviousPeriod = " +
    "IF ( " +
    "    NOT ISBLANK ( ValueCurrentPeriod ) && NOT ISBLANK ( ValuePreviousPeriod ), " +
    "     ValueCurrentPeriod - ValuePreviousPeriod" +
    " ) " +
    "VAR Result = " +
    "DIVIDE ( " +
    "    CurrentMinusPreviousPeriod," +
    "    ValuePreviousPeriod" +
    ") " +
    "RETURN " +
    "  Result";


string YOYTDpctLabel =
    "/*YOY%*/ " +
   "VAR ValueCurrentPeriod = " + YTDlabel + " " +
    "VAR ValuePreviousPeriod = " + PYTDlabel + " " +
    "VAR Result = " +
    "IF ( " +
    "    NOT ISBLANK ( ValueCurrentPeriod ) && NOT ISBLANK ( ValuePreviousPeriod ), " +
    "     ValueCurrentPeriod & \" vs \" & ValuePreviousPeriod & \" (%)\"" +
    " ) " +
    "RETURN " +
    "  Result";


string MAT =
 "        /*TAM*/" +
 "        IF (" +
    "    [" + ShowValueForDatesMeasureName + "], " +
 "            CALCULATE (" +
 "                SELECTEDMEASURE()," +
 "                DATESINPERIOD (" +
 "                    " + dateColumnWithTable + " ," +
 "                    MAX ( " + dateColumnWithTable + "  )," +
 "                    -1," +
 "                    YEAR" +
 "                )" +
 "                " +
 "            )" +
 "        )";


string MATlabel =
    "        /*TAM*/" +
 "        IF (" +
    "    [" + ShowValueForDatesMeasureName + "], " +
 "            CALCULATE (" +
 "                " + dateRangeMeasure.DaxObjectFullName + "," +
 "                DATESINPERIOD (" +
 "                    " + dateColumnWithTable + " ," +
 "                    MAX ( " + dateColumnWithTable + "  )," +
 "                    -1," +
 "                    YEAR" +
 "                )" +
 "                " +
 "            )" +
 "        )";

string MATminus1 =
 "        /*TAM*/" +
 "        IF (" +
 "            [" + ShowValueForDatesMeasureName + "], " +
 "            CALCULATE (" +
 "                SELECTEDMEASURE()," +
 "                DATESINPERIOD (" +
 "                    " + dateColumnWithTable + "," +
 "                    LASTDATE( DATEADD( " + dateColumnWithTable + ", - 1, YEAR ) )," +
 "                    -1," +
 "                    YEAR" +
 "                )" +
 "            )" +
 "        )";

string MATminus1label = 
    "/*MAT-1*/" +
 "        IF (" +
 "            [" + ShowValueForDatesMeasureName + "], " +
 "            CALCULATE (" +
 "                " + dateRangeMeasure.DaxObjectFullName + "," +
 "                DATESINPERIOD (" +
 "                    " + dateColumnWithTable + "," +
 "                    LASTDATE( DATEADD( " + dateColumnWithTable + ", - 1, YEAR ) )," +
 "                    -1," +
 "                    YEAR" +
 "                )" +
 "            )" +
 "        )";
;

string MATvsMATminus1 =
 "        /*MAT vs MAT-1*/\r\n" +
 "        VAR MAT = " + MAT + "\r\n" +
 "        VAR MAT_1 =" + MATminus1 + "\r\n" +
 "        RETURN \r\n" +
 "            IF( ISBLANK( MAT ) || ISBLANK( MAT_1 ), BLANK(), MAT - MAT_1 )";

string MATvsMATminus1label = "/*MAT vs MAT-1*/" +
    
 "        VAR MAT = " + MATlabel + "\r\n" +
 "        VAR MAT_1 =" + MATminus1label + "\r\n" +
 "        RETURN \r\n" +
 "            IF( ISBLANK( MAT ) || ISBLANK( MAT_1 ), BLANK(), MAT & \" vs \" & MAT_1 )";

string MATvsMATminus1pct =
 "        /*MAT vs MAT-1(%)*/" +
 "        VAR MAT = " + MAT + "\r\n" +
 "        VAR MAT_1 =" + MATminus1 + "\r\n" +
 "        RETURN" +
 "            IF(" +
 "                ISBLANK( MAT ) || ISBLANK( MAT_1 )," +
 "                BLANK()," +
 "                DIVIDE( MAT - MAT_1, MAT_1 )" +
 "            )";

string MATvsMATminus1pctlabel = "/*MAT vs MAT-1 (%)*/" +
                 "        VAR MAT = " + MATlabel + "\r\n" +
 "        VAR MAT_1 =" + MATminus1label + "\r\n" +
 "        RETURN \r\n" +
 "            IF( ISBLANK( MAT ) || ISBLANK( MAT_1 ), BLANK(), MAT & \" vs \" & MAT_1 & \" (%)\" )"; 

string MMT = String.Format(
        @"/*MMT*/
IF(
[{0}],
CALCULATE( SELECTEDMEASURE( ), DATESINPERIOD( {1}, MAX( {1} ), -1, MONTH ) )
)", ShowValueForDatesMeasureName, dateColumnWithTable);

string MMTlabel = String.Format(
        @"/*MMT*/
IF(
[{0}],
CALCULATE( {2}, DATESINPERIOD( {1}, MAX( {1} ), -1, MONTH ) )
)", ShowValueForDatesMeasureName, dateColumnWithTable, dateRangeMeasure.DaxObjectFullName);

string MMTminus1 = String.Format(
        @"/*MMT*/
IF(
[{0}],
CALCULATE( SELECTEDMEASURE( ), DATESINPERIOD( {1}, LASTDATE( DATEADD( {1}, -1, MONTH ) ), -1, MONTH ) )
)", ShowValueForDatesMeasureName, dateColumnWithTable);

string MMTminus1label = "/*MMT-1*/" +
    String.Format(
        @"/*MMT*/
IF(
[{0}],
CALCULATE( {2}, DATESINPERIOD( {1}, LASTDATE( DATEADD( {1}, -1, MONTH ) ), -1, MONTH ) )
)", ShowValueForDatesMeasureName, dateColumnWithTable, dateRangeMeasure.DaxObjectFullName);

string MMTvsMMTminus1 =
 "        /*MMT vs MMT-1*/\r\n" +
 "        VAR MMT = " + MMT + "\r\n" +
 "        VAR MMT_1 =" + MMTminus1 + "\r\n" +
 "        RETURN \r\n" +
 "            IF( ISBLANK( MMT ) || ISBLANK( MMT_1 ), BLANK(), MMT - MMT_1 )";

string MMTvsMMTminus1label =
    "        /*MMT vs MMT-1*/\r\n" +
 "        VAR MMT = " + MMTlabel + "\r\n" +
 "        VAR MMT_1 =" + MMTminus1label + "\r\n" +
 "        RETURN \r\n" +
 "            IF( ISBLANK( MMT ) || ISBLANK( MMT_1 ), BLANK(), MMT & \" vs \" & MMT_1 )"; 

string MMTvsMMTminus1pct =
 "        /*MMT vs MMT-1(%)*/" +
 "        VAR MMT = " + MMT + "\r\n" +
 "        VAR MMT_1 =" + MMTminus1 + "\r\n" +
 "        RETURN" +
 "            IF(" +
 "                ISBLANK( MMT ) || ISBLANK( MMT_1 )," +
 "                BLANK()," +
 "                DIVIDE( MMT - MMT_1, MMT_1 )" +
 "            )";

string MMTvsMMTminus1pctlabel =
    "        /*MMT vs MMT-1(%)*/" +
 "        VAR MMT = " + MMTlabel + "\r\n" +
 "        VAR MMT_1 =" + MMTminus1label + "\r\n" +
 "        RETURN" +
 "            IF( ISBLANK( MMT ) || ISBLANK( MMT_1 ), BLANK(), MMT & \" vs \" & MMT_1  & \" (%)\")";



string MWT = String.Format(
        @"/*MWT*/
IF(
[{0}],
CALCULATE( SELECTEDMEASURE( ), DATESINPERIOD( {1}, MAX( {1} ), -7, DAY ) )
)", ShowValueForDatesMeasureName, dateColumnWithTable);

string MWTlabel = "/*MWT*/" +
    
    String.Format(
        @"/*MWT*/
IF(
[{0}],
CALCULATE( {2}, DATESINPERIOD( {1}, MAX( {1} ), -7, DAY ) )
)", ShowValueForDatesMeasureName, dateColumnWithTable,dateRangeMeasure.DaxObjectFullName); ;

string MWTminus1 = String.Format(
        @"/*MWT*/
IF(
[{0}],
CALCULATE( SELECTEDMEASURE( ), DATESINPERIOD( {1}, LASTDATE( DATEADD( {1}, -7, DAY ) ), -7, DAY ) )
)", ShowValueForDatesMeasureName, dateColumnWithTable);

string MWTminus1label = "/*MWT-1*/" +
    String.Format(
        @"/*MWT*/
IF(
[{0}],
CALCULATE( {2}, DATESINPERIOD( {1}, LASTDATE( DATEADD( {1}, -7, DAY ) ), -7, DAY ) )
)", ShowValueForDatesMeasureName, dateColumnWithTable, dateRangeMeasure.DaxObjectFullName);

string MWTvsMWTminus1 =
 "        /*MWT vs MWT-1*/\r\n" +
 "        VAR MWT = " + MWT + "\r\n" +
 "        VAR MWT_1 =" + MWTminus1 + "\r\n" +
 "        RETURN \r\n" +
 "            IF( ISBLANK( MWT ) || ISBLANK( MWT_1 ), BLANK(), MWT - MWT_1 )";

string MWTvsMWTminus1label = 
    "        /*MWT vs MWT-1*/\r\n" +
 "        VAR MWT = " + MWTlabel + "\r\n" +
 "        VAR MWT_1 =" + MWTminus1label + "\r\n" +
 "        RETURN \r\n" +
 "            IF( ISBLANK( MWT ) || ISBLANK( MWT_1 ), BLANK(), MWT & \" vs \" & MWT_1 )"; 

string MWTvsMWTminus1pct =
 "        /*MWT vs MWT-1(%)*/" +
 "        VAR MWT = " + MWT + "\r\n" +
 "        VAR MWT_1 =" + MWTminus1 + "\r\n" +
 "        RETURN" +
 "            IF(" +
 "                ISBLANK( MWT ) || ISBLANK( MWT_1 )," +
 "                BLANK()," +
 "                DIVIDE( MWT - MWT_1, MWT_1 )" +
 "            )";

string MWTvsMWTminus1pctlabel = 
    "/*MWT vs MWT-1 (%)*/" +
 "        VAR MWT = " + MWTlabel + "\r\n" +
 "        VAR MWT_1 =" + MWTminus1label + "\r\n" +
 "        RETURN \r\n" +
 "            IF( ISBLANK( MWT ) || ISBLANK( MWT_1 ), BLANK(), MWT & \" vs \" & MWT_1 & \" (%)\")";



string defFormatString = "SELECTEDMEASUREFORMATSTRING()";

//if the flag expression is already present in the format string, do not change it, otherwise apply % format. 
string pctFormatString =
"IF(" +
"\n	FIND( " + flagExpression + ", SELECTEDMEASUREFORMATSTRING(), 1, - 1 ) <> -1," +
"\n	SELECTEDMEASUREFORMATSTRING()," +
"\n	\"#,##0.# %\"" +
"\n)";


//the order in the array also determines the ordinal position of the item    
string[,] calcItems =
    {
{"CY",      CY,         defFormatString,    "Current year",             CYlabel},
{"PY",      PY,         defFormatString,    "Previous year",            PYlabel},
{"YOY",     YOY,        defFormatString,    "Year-over-year",           YOYlabel},
{"YOY%",    YOYpct,     pctFormatString,    "Year-over-year%",          YOYpctLabel},
{"YTD",     YTD,        defFormatString,    "Year-to-date",             YTDlabel},
{"PYTD",    PYTD,       defFormatString,    "Previous year-to-date",    PYTDlabel},
{"YOYTD",   YOYTD,      defFormatString,    "Year-over-year-to-date",   YOYTDlabel},
{"YOYTD%",  YOYTDpct,   pctFormatString,    "Year-over-year-to-date%",  YOYTDpctLabel},
{"MAT",     MAT,        defFormatString,    "Moving Anual Total",       MATlabel},
{"MAT-1",   MATminus1,  defFormatString,    "Moving Anual Total -1 year", MATminus1label},
{"MAT vs MAT-1", MATvsMATminus1, defFormatString, "Moving Anual Total vs Moving Anual Total -1 year", MATvsMATminus1label},
{"MAT vs MAT-1(%)", MATvsMATminus1pct, pctFormatString, "Moving Anual Total vs Moving Anual Total -1 year (%)", MATvsMATminus1pctlabel},
{"MMT",     MMT,        defFormatString,    "Moving Monthly Total",       MMTlabel},
{"MMT-1",   MMTminus1,  defFormatString,    "Moving Monthly Total -1 month", MMTminus1label},
{"MMT vs MMT-1", MMTvsMMTminus1, defFormatString, "Moving Monthly Total vs Moving Monthly Total -1 month", MMTvsMMTminus1label},
{"MMT vs MMT-1(%)", MMTvsMMTminus1pct, pctFormatString, "Moving Monthly Total vs Moving Monthly Total -1 month (%)", MMTvsMMTminus1pctlabel},
{"MWT",     MWT,        defFormatString,    "Moving Weekly Total",       MWTlabel},
{"MWT-1",   MWTminus1,  defFormatString,    "Moving Weekly Total -1 week", MWTminus1label},
{"MWT vs MWT-1", MWTvsMWTminus1, defFormatString, "Moving Weekly Total vs Moving Weekly Total -1 week", MWTvsMWTminus1label},
{"MWT vs MWT-1(%)", MWTvsMWTminus1pct, pctFormatString, "Moving Weekly Total vs Moving Weekly Total -1 week (%)", MWTvsMWTminus1pctlabel}
};


int j = 0;


//create calculation items for each calculation with formatstring and description
foreach (var cg in Model.CalculationGroups)
{
    if (cg.Name == calcGroupName)
    {
        for (j = 0; j < calcItems.GetLength(0); j++)
        {

            string itemName = calcItems[j, 0];

            string itemExpression = calcItemProtection.Replace("<CODE>", calcItems[j, 1]);
            itemExpression = itemExpression.Replace("<LABELCODE>", calcItems[j, 4]);

            string itemFormatExpression = calcItemFormatProtection.Replace("<CODE>", calcItems[j, 2]);
            itemFormatExpression = itemFormatExpression.Replace("<LABELCODEFORMATSTRING>", "\"\"\"\" & " + calcItems[j, 4] + " & \"\"\"\"");

            //if(calcItems[j,2] != defFormatString) {
            //    itemFormatExpression = calcItemFormatProtection.Replace("<CODE>",calcItems[j,2]);
            //};

            string itemDescription = calcItems[j, 3];

            if (!cg.CalculationItems.Contains(itemName))
            {
                var nCalcItem = cg.AddCalculationItem(itemName, itemExpression);
                nCalcItem.FormatStringExpression = itemFormatExpression;
                nCalcItem.FormatDax();
                nCalcItem.Ordinal = j;
                nCalcItem.Description = itemDescription;

            };




        };


    };
};
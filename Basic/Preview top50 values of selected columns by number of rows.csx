// Author: Martin de la Herran, expanded from original work from Tommy Puglia ( https://pugliabi.com )
// Script to show the top 50 distinct values based on selected columns. 
// It shows the top 50 values of the selected columns and number and % of rows, 
// The [@SPECIAL] column indicates clearly blank vs empty string, and also shows the total remainig (after the top50) (in this case, disregard the individual value, it is a placeholder)

// Pre-Req:
// Save this Macro with the following:
// Name: Preview\Top50
// Marco Context: Column
// (Hint, you can organize your macros in TE3 by grouping based on a "\" separator!")

// Instructions:
// 1. Select the columns you want to see the distinct values for
// 2. Run the script (Right Click, on the selected columns, and choose the Preview --> Top50 macro)
// 3. For every column chosen, a pop-up will appear


foreach(var c in Selected.Columns)
{
string cName = c.Name;
string cTable = c.Table.DaxObjectFullName;
var cColumn = c.DaxObjectFullName;
var cColumnName = c.DaxObjectName;
string daxQuery =
 " VAR _TotalRows = COUNTROWS( " + cTable + " ) "
+" VAR _TOP = "
+" TOPN(50 ,                                                        "
+"    ADDCOLUMNS(                                                   "
+"        VALUES( " + cColumn + " )                                 "
+"        , \"@SPECIAL\" , IF(ISBLANK( " + cColumn	 + " ) , \"!!____ BLANK ____!!\"  "  
+"            ,  IF(FORMAT( " + cColumn + " , 0.0000 )==\"\", \"!!____ EMPTY STRING ____!!\" "
+"            , blank() )) " 
+"        , \"@CR\" , CALCULATE(COUNTROWS(+ " + cTable + " ))       "
+"        , \"@%CR\" ,ROUND(100*CALCULATE(COUNTROWS(+ " + cTable + " ))/_TotalRows , 2)      "
+"    )                                                             "
+"    , [@CR] , DESC , " + cColumnName + " , ASC                                             "
+" ) "
+" VAR _REMAINING = _TotalRows - sumx(_TOP, [@CR])  " 
+" var _DUMMY = MIN( " + cColumn + " )  "
+" VAR _ROWREMAIN = { ( _DUMMY , \"!!____ OTHER ____!!\" , _REMAINING , ROUND(100*_REMAINING/_TotalRows , 2) )}  "
+" VAR _FINAL = FILTER(UNION( _TOP , _ROWREMAIN), [@CR] > 0 ) "
+ "RETURN _FINAL "
+ "ORDER BY [@SPECIAL] DESC, [@CR] DESC " ;

//daxQuery.Output()	; // uncomment this is you want to see the DAX query

var cResult = EvaluateDax(daxQuery);

cResult.Output();
}

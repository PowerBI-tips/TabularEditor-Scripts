string calcGroupTypeLabel = "CalcGroupType";
string calcGroupTypeValue = "MultiTotal";
IEnumerable<Table> multiTotalCalcGroups = 
    Model.Tables.Where(
        t => 
        t.GetAnnotation(calcGroupTypeLabel) 
            == calcGroupTypeValue);
Table calcGroupAsTable = null as Table; 
if(multiTotalCalcGroups.Count() == 0)
{
    Error("No multi-total calc group found. " +
        "Run the macro to create a multi-total " +
        "calc group first and try again");
    return;
} else if(multiTotalCalcGroups.Count() == 1)
{
    calcGroupAsTable = multiTotalCalcGroups.First();
}
else
{
    calcGroupAsTable = SelectTable(multiTotalCalcGroups, label: "Select Multi-total Calc Group to use");
    if(calcGroupAsTable == null)
    {
        Error("You cancelled the execution.");
        return;
    }
}
if(Selected.CalculationItems.Count() == 0)
{
    Error("Select one or more calculation items and try again.");
    return;
}
string calcGroupValuesFieldLabel = "ValuesField";
string multiTotalBreakDownColumnCode = calcGroupAsTable.GetAnnotation(calcGroupValuesFieldLabel);
CalculationGroupTable calcGroup = calcGroupAsTable as CalculationGroupTable;
foreach(CalculationItem calcItem in Selected.CalculationItems)
{
    string calcItemName = calcItem.Name;
    string calcItemExpression =
        String.Format(
            @"IF(
                NOT ISINSCOPE( {0} ),
                CALCULATE(
                    SELECTEDMEASURE( ),
                    {1} = ""{2}""
                )
            )",
            multiTotalBreakDownColumnCode,
            calcItem.CalculationGroupTable.Columns[0].DaxObjectFullName,
            calcItem.Name);
    CalculationItem customTotalCalcItem = 
        calcGroup.AddCalculationItem(
            name:calcItemName, 
            expression:calcItemExpression);
    string calcItemFormatStringExpression =
        String.Format(
            @"IF(
                NOT ISINSCOPE( {0} ),
                CALCULATE(
                    SELECTEDMEASUREFORMATSTRING( ),
                    {1} = ""{2}""
                )
            )",
            multiTotalBreakDownColumnCode,
            calcItem.CalculationGroupTable.Columns[0].DaxObjectFullName,
            calcItem.Name);
    customTotalCalcItem.FormatStringExpression = 
        calcItemFormatStringExpression;
    customTotalCalcItem.FormatDax();
}

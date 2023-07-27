//'2023-07-27 / B.Agullo / 
// Generate Calc Group to Sort a Matrix by a Calc Item Column
// Automation of the Calculation group introduced in this blog post: https://www.esbrina-ba.com/sorting-a-matrix-by-a-calculation-item-column/
// this script was written live in the Seattle Modern Excel & Power BI User Group  on July 26th 2023.
// To execute select a calculation group table and click execute
// It es recommented to store as macro for Calculation Group Table

string noSortCalcItemName = "No Sort";
if(Selected.Tables.Count != 1)
{
    Error("Please select a single calculation group and try again.");
    return;
}
if(Selected.Table.GetTypeName() != "Calculation Group Table")
{
    Error("This is not a calculation group");
    return;
}
CalculationGroupTable calculationGroupTable =
    Selected.Table as CalculationGroupTable;
calculationGroupTable.AddCalculationItem(noSortCalcItemName, "1");
string calcTableName = calculationGroupTable.Name + " Names";
string calcTableExpression = calculationGroupTable.DaxObjectFullName;
CalculatedTable calculatedTable =
    Model.AddCalculatedTable(calcTableName, calcTableExpression);
bool te2 = (calculatedTable.Columns.Count == 0);
Column firstCalcTableColumn =
    te2 ? calculatedTable.AddCalculatedColumn(
        calculationGroupTable.Columns[0].Name, "1")
    : calculatedTable.Columns[0];
string sortCalcGroupName = "Sort";
string sortCalcItemExpression =
    String.Format(
        @" VAR inTotal =
            NOT HASONEVALUE ( {0} )
        VAR sortBy =
            SELECTEDVALUE ( {1}, ""{2}"" )
        VAR result =
            IF (
                inTotal,
                CALCULATE (
                    SELECTEDMEASURE (),
                    {0} = sortBy
                ),
                SELECTEDMEASURE ()
            )
        RETURN
            result",
        calculationGroupTable.Columns[0].DaxObjectFullName,
        calculatedTable.Columns[0].DaxObjectFullName,
        noSortCalcItemName);
CalculationGroupTable sortCalcGroup =
    Model.AddCalculationGroup(sortCalcGroupName);
CalculationItem sortCalcItem =
    sortCalcGroup.AddCalculationItem(
        sortCalcGroupName,
        sortCalcItemExpression);
sortCalcItem.FormatDax();
if (te2)
{
    calculatedTable.Columns[0].Delete();
}

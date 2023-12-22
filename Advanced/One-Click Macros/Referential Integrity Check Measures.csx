//Select the desired table to store all data quality measures
//See https://www.esbrina-ba.com/easy-management-of-referential-integrity/
// 2023-02-15 / B.Agulló / Added useRelationship in the expression to check also inactive relationships
// 2023-12-22 / B.Agulló / Added suggestions by Ed Hansberry 
string overallCounterExpression = "";
string overallCounterName = "Total Unmapped Items";
string overallDetailExpression = "\"\"";
string overallDetailName = "Data Problems";
Table tableToStoreMeasures = Selected.Tables.First();
foreach (var r in Model.Relationships)
{
    bool isOneToMany =
        r.FromCardinality == RelationshipEndCardinality.One
        && r.ToCardinality == RelationshipEndCardinality.Many;
    bool isManyToOne =
        r.FromCardinality == RelationshipEndCardinality.Many
        && r.ToCardinality == RelationshipEndCardinality.One;
    Column manyColumn = null as Column;
    Column oneColumn = null as Column;
    bool isOneToManyOrManyToOne = true;
    if (isOneToMany)
    {
        manyColumn = r.ToColumn;
        oneColumn = r.FromColumn;
    }
    else if (isManyToOne)
    {
        manyColumn = r.FromColumn;
        oneColumn = r.ToColumn;
    }
    else
    {
        isOneToManyOrManyToOne = false;
    }
    if (isOneToManyOrManyToOne)
    {
        string orphanCountExpression =
            "CALCULATE("
                + "SUMX(VALUES(" + manyColumn.DaxObjectFullName + "),1),"
                + "ISBLANK(" + oneColumn.DaxObjectFullName + "),"
                + "USERELATIONSHIP(" + manyColumn.DaxObjectFullName + "," + oneColumn.DaxObjectFullName + "),"
                + "ALLEXCEPT(" + manyColumn.Table.DaxObjectFullName + "," + manyColumn.DaxObjectFullName + ")"
            + ")";
        string orphanMeasureName =
            manyColumn.Name + " not mapped in " + manyColumn.Table.Name;
        Measure newCounter = tableToStoreMeasures.AddMeasure(name: orphanMeasureName, expression: orphanCountExpression, displayFolder: "_Data quality Measures");
        newCounter.FormatString = "#,##0";
        newCounter.FormatDax();
        string orphanTableTitleMeasureExpression = "FORMAT(" + newCounter.DaxObjectFullName +",\"" + newCounter.FormatString + "\") & \" " + newCounter.Name + "\"";
        string orphanTableTitleMeasureName = newCounter.Name + " Title";
        Measure newTitle = tableToStoreMeasures.AddMeasure(name: orphanTableTitleMeasureName, expression: orphanTableTitleMeasureExpression, displayFolder: "_Data quality Titles");
        newTitle.FormatDax();
        overallCounterExpression = overallCounterExpression + "+" + newCounter.DaxObjectFullName;
        overallDetailExpression = overallDetailExpression
                + " & IF(" + newCounter.DaxObjectFullName + "> 0,"
                            + newTitle.DaxObjectFullName + " & UNICHAR(10))";
    };
};
Measure counter = tableToStoreMeasures.AddMeasure(name: overallCounterName, expression: overallCounterExpression);
counter.FormatString = "#,##0";
counter.FormatDax();
Measure descr = tableToStoreMeasures.AddMeasure(name: overallDetailName, expression: overallDetailExpression);
descr.FormatDax();

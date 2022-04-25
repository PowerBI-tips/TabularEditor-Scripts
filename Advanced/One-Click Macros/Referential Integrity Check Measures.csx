//Select the desired table to store all data quality measures


string overallCounterExpression = "";
string overallCounterName = "Total Unmapped Items";

string overallDetailExpression = "\"\"";
string overallDetailName = "Data Problems";

Table tableToStoreMeasures = Selected.Tables.First();

foreach (var r in Model.Relationships)
{


    bool isOneToMany =
        r.FromCardinality == RelationshipEndCardinality.One
        & r.ToCardinality == RelationshipEndCardinality.Many;

    bool isManyToOne =
        r.FromCardinality == RelationshipEndCardinality.Many
        & r.ToCardinality == RelationshipEndCardinality.One;

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
                + oneColumn.DaxObjectFullName + " = BLANK()"
            + ")";
        string orphanMeasureName =
            manyColumn.Name + " not mapped in " + manyColumn.Table.Name;

        Measure newCounter = tableToStoreMeasures.AddMeasure(name: orphanMeasureName, expression: orphanCountExpression,displayFolder:"_Data quality Measures");
        newCounter.FormatDax(); 

        string orphanTableTitleMeasureExpression = newCounter.DaxObjectFullName + " & \" " + newCounter.Name + "\"";
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
counter.FormatDax();


Measure descr = tableToStoreMeasures.AddMeasure(name: overallDetailName, expression: overallDetailExpression);
descr.FormatDax();

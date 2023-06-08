// '2023-06-08 / B.Agullo / Creates a measure to show only relevant items in slicers for a fact table.


if(Selected.Tables.Count() == 0)
{
    Error("Select at least one table and try again");
    return;
}
foreach(Table table in Selected.Tables)
{
    string measureExpression = String.Format(@"INT(NOT ISEMPTY({0})",table.DaxObjectFullName);
    string measureName = table.Name + " has data";
    string measureDescription = String.Format(@"Returns 1 if {0} has visible rows in filter context, 0 otherwise. Can be used to show only relevant slicer items.", table.DaxObjectFullName);
    Measure measure = table.AddMeasure(measureName, measureExpression);
    measure.Description = measureDescription;
}

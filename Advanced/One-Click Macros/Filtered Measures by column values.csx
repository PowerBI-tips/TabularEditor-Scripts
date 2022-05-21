// '2022-05-21 / B.Agullo / 
// FILTERED MEASURES BY COLUMN VALUES SCRIPT 
// creates a measure for each of the values in a column filtering the selected base measure


var measures = Selected.Measures;

if(measures.Count == 0)
{
    ScriptHelper.Error("Select one or more measures");
}
            
Table table = ScriptHelper.SelectTable();
Column column = ScriptHelper.SelectColumn(table);

string query = "EVALUATE DISTINCT(" + column.DaxObjectFullName + ")";

using (var reader = Model.Database.ExecuteReader(query))
{
    // Create a loop for every row in the resultset
    while (reader.Read())
    {
        string columnValue = reader.GetValue(0).ToString();

        if (column.DataType.Equals(DataType.String))
        {
            columnValue = "\"" + columnValue + "\""; 
        }


        foreach (Measure measure in measures)
        {
            string measureName = measure.Name + " " + columnValue;
            string measureExpression =
                string.Format("CALCULATE({0},{1}={2})",
                    measure.DaxObjectName,
                    column.DaxObjectFullName,
                    columnValue
                );
            string measureDescription =
                string.Format("{0} filtered by {1} = {2}",
                    measure.Name,
                    column.Name,
                    columnValue
               );
            string displayFolderName =
                string.Format("{0} by {1}",
                    measure.Name,
                    column.Name
                );
            Measure newMeasure = 
                measure.Table.AddMeasure(
                    name: measureName,
                    expression: measureExpression,
                    displayFolder: displayFolderName
                );
            newMeasure.Description = measureDescription;
            newMeasure.FormatDax();


        }
    }
}
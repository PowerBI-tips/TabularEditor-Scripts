/*
 * Title: Create Measure for Each Item in Selected Columns
 * 
 * Author: Michael Mays, twitter.com/ItsAMaysin 
 *
 * This script, when executed, will loop through the currently selected columns and
 * create a measure for each item in the column. In the below, the newly
 * created measure will be of the format 
 * CALCULATE ( [BaseMeasure], Table[SelectedColumn] = "Item N in Selected Column")
 */

foreach (var c in Selected.Columns)
{
    string MyColumn = c.DaxObjectFullName;
    string GetItems = "EVALUATE VALUES (" + MyColumn + ")"; // List of items from column

    using (var reader = Model.Database.ExecuteReader(GetItems))
    {

        // Create a loop for every row in the resultset
        while(reader.Read())
        {
            string MyItem = reader.GetValue(0).ToString();
            string MyExpression;
            string MeasureName = "***Your Measure Here****";
            string NamePreFix = "# of ";
            string NamePostFix = "";
    
            //Create measure for item
            MyExpression = 
                "CALCULATE ( [" + MeasureName + "], " + MyColumn + " = \"" + MyItem + "\" )" ;
            c.Table.AddMeasure(
                NamePreFix + MyItem + NamePostFix, 
                MyExpression
            );

        }
    };
};
/*
 * Title: Generate Last Updated table
 * 
 * Author: Irfan Charania
 * 
 * There are times when you want to know when your model was last processed at the table level.
 * SSAS Tabular does not provide an easy built-in way to do this.
 * 
 * This script, when executed, will loop through all the tables in the model, and 
 * 1. create (or update) a hidden "Last Processed" calculated column, and
 * 2. create (or update) a "Last Updated Tabular" calculated table containing data from the above calculated column
 * 
 * Ref: https://www.sqlbi.com/articles/last-process-date-in-ssas-tabular/
 */

// Edit these values as desired 
var columnName = "Last Processed";
var tableName = "Last Updated Tabular";
var dateFormatString = "yyyy-MMM-dd h:mm AM/PM";

// ---------------------------
// 1. create (or update) a hidden "Last Processed" calculated column
// ---------------------------

// Loop through all tables in model
foreach(var table in Model.Tables) {

    // for all non-calculated tables
    if (table.SourceType.ToString() != "Calculated" ){

        CalculatedColumn column;

        // if calculated column does not exist, create one
        if (!table.Columns.Contains(columnName))
        {
            column = table.AddCalculatedColumn(
                "Last Processed",  // Name
                "NOW()",           // DAX Expression
                "_Debug"           // Display Folder
            );
        }
        // else retreive the calculated column
        else{
            column = (CalculatedColumn)table.Columns[columnName];
        }

        // and update it's properties
        column.DataType = DataType.DateTime;
        column.IsHidden = true;
        column.FormatString = dateFormatString;
        column.Description = "Shows when SSAS table was last processed";
    }
}

// ---------------------------
// 2. create (or update) a "Last Updated Tabular" calculated table 
//    containing data from the above calculated column
// ---------------------------

List<string> rowsList = new List<string>();


// Loop through all tables in model
foreach(var table in Model.Tables) {

    // if calculated column exists
    if (table.Columns.Contains(columnName))
    {
        // create DAX row expression to create row data
        var s = String.Format(@"
    ROW (
        ""Last Processed"", FORMAT ( MAX ( {0} ), ""{1}""),
        ""Table Name"", ""{2}""
    )"
                , table.Columns[columnName].DaxObjectFullName
                , dateFormatString
                , table.Name);

        // add row expression to list
        rowsList.Add(s);
    }
}

// Combine all row statements into a single DAX expression
var rows = String.Join(", ", rowsList);
var expression = String.Format("UNION ( {0} )", rows);

CalculatedTable tbl;

// If calculated table already exists, retreive it
if (Model.Tables.Contains(tableName)){
    tbl = (CalculatedTable)Model.Tables[tableName];
}
// else add new calculated table to the model
else{
    tbl = Model.AddCalculatedTable(tableName);
}

// update calculated table's expression to earlier-built DAX expression
tbl.Expression = expression;
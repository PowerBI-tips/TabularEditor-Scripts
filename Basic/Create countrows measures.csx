/*
 * Title: Auto-generate COUNTROWS measures from tables
 * 
 * Author: Edgar Walther, twitter.com/edgarwalther
 * 
 * This script, when executed, will loop through the currently selected tables,
 * creating one COUNTROWS measure for each table.
 */
 
 // Loop through all currently selected tables:
foreach(var table in Selected.Tables) {
    
    var newMeasure = table.AddMeasure(
    "# Rows in " + table.Name,                         // Name
    "COUNTROWS(" + table.DaxObjectFullName + ")"       // DAX expression
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "0";

    // Provide some documentation:
    newMeasure.Description = "This measure is the number of rows in table " + table.DaxObjectFullName;
    
}
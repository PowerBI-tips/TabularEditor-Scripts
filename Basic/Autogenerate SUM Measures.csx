/*
 * Title: Auto-generate SUM measures from columns
 * 
 * Author: Daniel Otykier, twitter.com/DOtykier
 * 
 * This script, when executed, will loop through the currently selected columns,
 * creating one SUM measure for each column and also hiding the column itself.
 */
 
// Loop through all currently selected columns:
foreach(var c in Selected.Columns)
{
    var newMeasure = c.Table.AddMeasure(
        "Sum of " + c.Name,                    // Name
        "SUM(" + c.DaxObjectFullName + ")",    // DAX expression
        c.DisplayFolder                        // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "0.00";

    // Provide some documentation:
    newMeasure.Description = "This measure is the sum of column " + c.DaxObjectFullName;

    // Hide the base column:
    c.IsHidden = true;
}

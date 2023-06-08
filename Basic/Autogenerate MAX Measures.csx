/*
 * Title: Auto-generate MAX measures from columns
 * 
 * Author: B.Agullo (Adapted from Daniel Otykier, twitter.com/DOtykier)
 * 
 * This script, when executed, will loop through the currently selected columns,
 * creating one MAX measure for each column and also hiding the column itself.
 */
 
// Loop through all currently selected columns:
foreach(Column c in Selected.Columns)
{
    var newMeasure = c.Table.AddMeasure(
        "Max  " + c.Name,                    // Name
        "MAX(" + c.DaxObjectFullName + ")",    // DAX expression
        c.DisplayFolder                        // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = c.FormatString;

    // Provide some documentation:
    newMeasure.Description = "This measure is the MAX of column " + c.DaxObjectFullName;

    // Hide the base column:
    //c.IsHidden = true;
}

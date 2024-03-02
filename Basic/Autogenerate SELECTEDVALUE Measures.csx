/*
 * Title: Auto-generate SELECTEDVALUE measures from columns
 * 
 * Author: B.Agullo (Adapted from Daniel Otykier, twitter.com/DOtykier)
 * 
 * This script, when executed, will loop through the currently selected columns,
 * creating one SELECTEDVALUE measure for each column and also hiding the column itself.
 */
 
// Loop through all currently selected columns:
foreach(var c in Selected.Columns)
{
    var newMeasure = c.Table.AddMeasure(
        "Selected  " + c.Name,                    // Name
        "SELECTEDVALUE(" + c.DaxObjectFullName + ")",    // DAX expression
        c.DisplayFolder                        // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "0.00";

    // Provide some documentation:
    newMeasure.Description = "This measure is the SELECTEDVALUE of column " + c.DaxObjectFullName;

    // Hide the base column:
    //c.IsHidden = true;
}

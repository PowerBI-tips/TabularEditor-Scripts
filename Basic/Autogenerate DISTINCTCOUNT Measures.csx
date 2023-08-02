/*
 * Title: Auto-generate DISTINCTCOUNT measures from columns
 * 
 * B.Agullo adapting code from Daniel Otykier
 * 
 * This script, when executed, will loop through the currently selected columns,
 * creating one DISTINCTCOUNT measure for each column
 */
 
// Loop through all currently selected columns:
foreach(var c in Selected.Columns)
{
    var newMeasure = c.Table.AddMeasure(
        "Distinct Count of " + c.Name,                    // Name
        "DISTINCTCOUNT(" + c.DaxObjectFullName + ")",    // DAX expression
        c.DisplayFolder                        // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "0";

    // Provide some documentation:
    newMeasure.Description = "This measure is the distinct count of column " + c.DaxObjectFullName;

    // Hide the base column:
    //c.IsHidden = true;
}

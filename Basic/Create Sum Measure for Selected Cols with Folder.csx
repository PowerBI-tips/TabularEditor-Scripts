/* Creates a SUM measure for every currently selected column(s)
 *
 * Author: Mike Carlo, https://powerbi.tips
 *
 * Select Columns using Control + Left Click
 * Script will create one measure for each column selected
 * Then Change the formatting of the column, adds a description,
 * and places measures in a named folder.
 *
 */

// Loop through the list of selected columns
foreach(var c in Selected.Columns)
{
    var newMeasure = c.Table.AddMeasure(
        "Sum of " + c.Name,                    // Name
        "SUM(" + c.DaxObjectFullName + ")",    // DAX expression
        c.DisplayFolder                        // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "0";

    // Provide some documentation:
    newMeasure.Description = "This measure is the sum of column " + c.DaxObjectFullName;

    // Create all measures within a Named Folder
    newMeasure.DisplayFolder = "_Model";

    // Hide the base column:
    c.IsHidden = true;
}

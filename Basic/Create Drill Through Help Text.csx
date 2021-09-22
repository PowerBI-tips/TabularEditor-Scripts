
/*
 * Title: Create a Drill Through Help Text for a Button
 *
 * Author: Tommy Puglia, https://powerbi.tips/explicit-measures-power-bi-podcast/
 *
 * This script will loop through selected columns to create the text for a button to drill through
 * 
 */
foreach(var c in Selected.Columns)
{
var newMeasure = c.Table.AddMeasure(
        "drtext_" + c.Name,                    // Name
"IF(SELECTEDVALUE(" + c.DaxObjectFullName  + "),0 == 0, \"Click a Company to See Details","See details for \" SELECTEDVALUE(" + c.DaxObjectFullName + "))"

                        // Display Folder;
);
  newMeasure.DisplayFolder = "_Sel";  
}

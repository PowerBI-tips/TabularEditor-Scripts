
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
  
  // Appends a prefix to the name of the column
  "dr_text_" + c.Name ,

  // Generate full measure
  //"IF(SELECTEDVALUE(" + c.DaxObjectFullName  + ", \"Click a Company to See Details\" ), \"See details for \" & SELECTEDVALUE(" + c.DaxObjectFullName + "))"
 // );
  
  "IF(HASONEVALUE(" + c.DaxObjectFullName  + "), \"See details for \" & SELECTEDVALUE(" + c.DaxObjectFullName + "), \"Click a Company to See Details\" )"

  // Display Folder;
  newMeasure.DisplayFolder = "_Sel";  
}

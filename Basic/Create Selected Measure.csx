/*
 * Title: Create a Selected Value for a Drill Through Page
 *
 * Author: Tommy Puglia, https://powerbi.tips/explicit-measures-power-bi-podcast/
 *
 * This script will loop through selected columns to create a text measure that shows what was selected for a Drill Through Page
 * 
 */
foreach(var c in Selected.Columns)
{
var newMeasure = c.Table.AddMeasure(

  // Create name of measure using a prefix
  "Sel. " + c.Name,

  // Create full measure
  "IF(ISFILTERED(" + c.DaxObjectFullName  + "), SELECTEDVALUE(" + c.DaxObjectFullName + ", \" Multiple Selected \"  ),  \"None Selected \" )"
  );
  
  // Display Folder
  newMeasure.DisplayFolder = "_Sel";  
}
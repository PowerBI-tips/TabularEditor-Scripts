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
        "Sel. " + c.Name,                    // Name
"IF(ISFILTERED(" + c.DaxObjectFullName  + "), SELECTEDVALUE(" + c.DaxObjectFullName + ", \" Mulitple Selected \"  ),  \"None Selected \" )"   

                        // Display Folder
    );
  newMeasure.DisplayFolder = "_Sel";  
}
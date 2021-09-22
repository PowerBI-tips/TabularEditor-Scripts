/*
 * Title: Move All Columns to a DisplayFolder  
 *
 * Author: Matt Allington, https://exceleratorbi.com.au  
 *
 *  move all columns into a display folder.  
 *  read why at https://exceleratorbi.com.au/column-sub-folders-better-than-measure-sub-folders/
 */


//Move all columns to display folder
foreach (var c in Model.AllColumns)
{
    c.DisplayFolder = "_Columns";
}

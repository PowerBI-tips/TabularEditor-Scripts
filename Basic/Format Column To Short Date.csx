/*
 * Title: Format All Columns to Short Date
 *
 * Author: Tommy Puglia, https://powerbi.tips/explicit-measures-power-bi-podcast/
 *
 * This script simply formats selected columns in a table to the Short Date format.
 * 
 */


foreach(var c in Selected.Columns) {
c.FormatString = "Short Date";
}

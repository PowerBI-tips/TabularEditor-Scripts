/*
 * Title: Replace Text Strings that appear in many measures
 * 
 * Author: Matt Allington http://xbi.com.au
 * 
 * This script, when executed, will loop through the currently selected measures
 * and replace the FromString with the ToString.
 */

/ Replace Text Strings that appear in many measures

    // String of text that is desired to be found
    // Update the value of the text for your desired usecase
    // This replacement example shows the replacement of a DAX expression and replaces the SUM(table[ColumnName]) with a well defined measure of [Total Sales]
    // It is best practice to use a single measure for Sum of Extended Amount and reference this measure in other DAX formulas. 
	var FromString = "CALCULATE(SUM(Sales[ExtendedAmount])";

    // String of text that is the replaced value
    // Update the value of the text for your desired usecase
	var ToString = "CALCULATE([Total Sales])";

	foreach (var m in Model.AllMeasures)
    	{
           m.Expression = m.Expression.Replace(FromString,ToString);
    	}

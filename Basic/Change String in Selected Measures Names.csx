//
//  Title: Replace Text Strings in name of the selected measures 
//  
//  Author: Artur Nawrocki
//  
//  This script, when executed, will loop through the currently selected measures
//  and replace the FromString with the ToString.
// 

// Replace Text Strings that appear in name of selected measures

// String of text that is desired to be found
// Update the value of the text for your desired usecase
// This replacement example shows the replacement of a DAX expression and replaces the Target with a text of Forecast

var FromString = "Target";

// String of text that is the replaced value
// Update the value of the text for your desired usecase
var ToString = "Forecast";

foreach (var m in Selected.Measures)
    {
        m.Name = m.Name.Replace(FromString,ToString);
    }

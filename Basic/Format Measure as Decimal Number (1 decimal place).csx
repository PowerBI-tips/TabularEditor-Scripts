/*
* Title: Format Measure as a decimal number to 1 decimal place
* 
* Author: Adam Grisdale
* 
* Loops through the selected measures and formats them as a decimal number to 1 decimal place with a thousands separator.
* Designed to be used as a macro action added to the MacroActions json file: %LocalAppData%\TabularEditor\MacroActions.json
*
* {
*   "Name": "Formatting\\Decimal Number\\One Decimal Place",
*   "Enabled": "true",
*   "Execute": "foreach ( var m in Selected.Measures)\r\n{\r\n    m.FormatString = \"#,0.0\";\r\n};",
*   "Tooltip": "Formats the selected measures to one decimal place.",
*   "ValidContexts": "Measure"
* }
*
*/

// Loop through all selected measures
foreach ( var m in Selected.Measures)
{
    // Set the measures format string
    m.FormatString = "#,0.0";
};
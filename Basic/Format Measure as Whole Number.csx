/*
* Title: Format Measure as a whole number
* 
* Author: Adam Grisdale
* 
* Loops through the selected measures and formats them as a whole number with a thousands separator.
* Designed to be used as a macro action added to the MacroActions json file: %LocalAppData%\TabularEditor\MacroActions.json
*
* {
*   "Name": "Formatting\\Whole Number",
*   "Enabled": "true",
*   "Execute": "foreach ( var m in Selected.Measures)\r\n{\r\n    m.FormatString = \"#,0\";\r\n};",
*   "Tooltip": "Formats the selected measures to one decimal place.",
*   "ValidContexts": "Measure"
* }
*
*/

// Loop through all selected measures
foreach ( var m in Selected.Measures)
{
    // Set the measures format string
    m.FormatString = "#,0";
};
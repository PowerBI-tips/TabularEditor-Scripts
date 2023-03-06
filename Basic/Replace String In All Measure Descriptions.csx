// Title: Replace String In All Model Descriptions.csx
// Author: Dan Meissner 
// Description: Replaces text in all descriptions for any measure in the model, replacing one string with another string.

var oldTableNameString = "Fact Headcount";
var newTableNameString = "Headcount Data";

// Loop through all tables in the model, replacing oldTableNameString with newTableNameString
foreach(var m in Model.AllMeasures)
{
    m.Description = m.Description.Replace(oldTableNameString, newTableNameString);
}
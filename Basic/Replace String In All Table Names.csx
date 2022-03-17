// Title: Replace String In All Table Names.csx
// Author: @JamesDBartlett3
// Description: Renames all tables in the model, replacing one string with another string

var oldTableNameString = "";
var newTableNameString = "";

// Loop through all tables in the model and
// replace oldTableNameString in each table name with newTableNameString

foreach(var t in Model.Tables)
{
    t.Name = t.Name.Replace(oldTableNameString, newTableNameString);
}

// Title: Replace String In All Table Names.csx
// Author: @JamesDBartlett3@techhub.social
// Description: Renames all tables in the model, replacing one string with another string

var oldTableNameString = "";
var newTableNameString = "";

// Loop through all tables in the model, replacing oldTableNameString with newTableNameString
foreach(var t in Model.Tables)
{
    t.Name = t.Name.Replace(oldTableNameString, newTableNameString);
	// Loop through all partitions in the table, replacing oldTableNameString with newTableNameString
	foreach(var p in t.Partitions)
	{
		p.Name = p.Name.Replace(oldTableNameString, newTableNameString);
	}
}

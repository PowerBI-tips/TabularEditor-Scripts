// Hide all columns ending with "Key" in selected tables from report

var keySuffix = "Key";

// Loop through all currently selected tables:
foreach(var t in Selected.Tables)
{
    // Loop through all columns ending with "Key" on the current table:
    foreach(var k in t.Columns.Where(c => c.Name.EndsWith(keySuffix)))
    {
        k.IsHidden = true;
    }
}
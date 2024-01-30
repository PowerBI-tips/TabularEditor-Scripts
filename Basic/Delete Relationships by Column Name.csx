// Title: Delete Relationships by Column Name.csx
// Author: @JamesDBartlett3
// Description: Deletes all relationships where the column name 
// on at least one side of the relationship matches the declared
// ColumnName variable. Automatically accounts for spaces.

var ColumnName = "";

List<Relationship> relationshipsToDelete = new List<Relationship>();

foreach(var r in Model.Relationships) {
  string[] c = {
    r.FromColumn.Name.ToString().Replace(" ",""), 
    r.ToColumn.Name.ToString().Replace(" ",""),
    r.FromColumn.Name.ToString(),
    r.ToColumn.Name.ToString()
  };
  if(c.Contains(ColumnName.Replace(" ",""))) {
    relationshipsToDelete.Add(r);
  }
}

foreach(var r in relationshipsToDelete) {
  r.Delete();
}

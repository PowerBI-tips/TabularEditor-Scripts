//
//
// by Tommy Puglia
// twitter: @tommypuglia
// pugliabi.com
//
// REFERENCE: 
// 
// Formats Date columns to short date, Formats Measures with not defined type
//

var sb = new System.Text.StringBuilder();
string newline = Environment.NewLine;

foreach(var c in Model.AllColumns.Where(a => a.DataType == DataType.DateTime)) {
	c.FormatString = "m/d/yyyy";

}
foreach(var c in Model.AllColumns.Where(a => a.DataType == DataType.Int64 || a.DataType == DataType.Decimal || a.DataType == DataType.Double)) {
	c.SummarizeBy = AggregateFunction.None;

}

foreach(var c in Model.AllMeasures) {
	string colFomratString = "";
	if (c.FormatString == colFomratString) {
		c.FormatString = "#,0";
	}
}
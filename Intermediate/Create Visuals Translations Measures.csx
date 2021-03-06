/* 
 * Create visuals translations measures 
 * Author: Didier Terrien      https://thebipower.fr
 * This script generates measures to be used in visuals conditional formating in order to translate 
 * visuals labels (title, ...). It reads a tsv file which contains the measure list  
 *
 * More details here : https://thebipower.fr/index.php/2020/05/07/visuals-labels-translations-in-power-bi-reports
 * Download the example tsv file in the blog post
 * There are different methods which have their own advantages. Be sure to read the post above before to use in production.
 */

// Modify the path and name of the visuals labels file to load 
var Visuals_labels_file = @"...\TE visuals labels.tsv";

// Modify the name of the table that should hold the measures. It will be created automatically
var Target_table_name = "Visuals translations measures";

// Modify the prefix of new measures to avoid conflicts with existing measures
var Measures_prefix = "AT_"; 

// Select a method between "USERCULTURE" and "SLICER"
var Method = "SLICER";


// Optionally select the file manually (Comment code below to use)

// using (var openFileDialog = new System.Windows.Forms.OpenFileDialog()) {
//    openFileDialog.Title = "Select a visuals labels file to load";
//    if (System.IO.File.Exists(Visuals_labels_file)){
//        openFileDialog.InitialDirectory = System.IO.Directory.GetParent(Visuals_labels_file).ToString();
//    }
//    openFileDialog.Filter = "tsv files (*.tsv)|*.tsv|All files (*.*)|*.*";
//    openFileDialog.FilterIndex = 1;
//    openFileDialog.RestoreDirectory = true;
//    if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
//        //Get the path of specified file
//        Visuals_labels_file = openFileDialog.FileName;
//        //System.Windows.Forms.MessageBox.Show("Selected file: " + Visuals_labels_file);
//    } else {
//        Visuals_labels_file = "";
//    }
// }


// If file exists
if (System.IO.File.Exists(Visuals_labels_file)) { 
    // Load the file content
    var tsvFileContent = ReadFile(Visuals_labels_file); 
    
    // Split it into rows
    var Rows = tsvFileContent.Split(new[] {'\r','\n'},StringSplitOptions.RemoveEmptyEntries);

    // Delete the target table to build it again from scratch
    if (Model.Tables.Contains(Target_table_name)) {
        Model.Tables[Target_table_name].Delete();
    }
    var Target_table = Model.AddCalculatedTable(Target_table_name, "{0}");
    var Fake_column = Target_table.AddCalculatedTableColumn("Fake Column", "[Fake Column]", "", DataType.Int64);
    Fake_column.IsHidden = true;
    Target_table.Description = "This table is auto generated by a Tabular Editor script. Do not modify it manually";
    Target_table.SetAnnotation("VTAUTOGEN", "1");  // Set a special annotation on the table. It might be useful in the future//    Model.Tables.First().Columns("Fake column").Is

    // Iterate all rows
    for(int Index_row = 1; Index_row < Rows.Length; Index_row++) {
        var Columns = Rows[Index_row].Split(new[] {'\t'},StringSplitOptions.None); // Split the current row into columns
        var Object_ID = Columns[0];
        var Page_name = Columns[1];
        var Object_type = Columns[2];
        var Visual_name = Columns[3];
        var Reference_text = Columns[4];
        
        // If Reference text is not empty
        if (Reference_text.Trim() != "") { 
            
            // Create the measure
            var VT_measure = Target_table.AddMeasure(Measures_prefix + Visual_name);
            var sb = new System.Text.StringBuilder();
            if (Method == "SLICER"){
                sb.Append("\r\n");
                sb.Append("VAR Reference_text = " + '"' + Visual_name + '"' + "\r\n");
                sb.Append("VAR Filtered_translations = FILTER( 'Visuals translations' ," + "\r\n");
                sb.Append("\t" + "'Visuals translations'[Reference text] = Reference_text )" + "\r\n");
                sb.Append("RETURN COALESCE( CALCULATE(" + "\r\n");
                sb.Append("\t" + "\t" + "\t" + "FIRSTNONBLANK( 'Visuals translations'[Translated text] , TRUE()) ," + "\r\n");
                sb.Append("\t" + "\t" + "\t" + "\t" + "Filtered_translations )" + "\r\n");
                sb.Append("\t" + "\t" + " , Reference_text" + "\r\n");
                sb.Append("\t" + ")");            
            } else if (Method == "USERCULTURE") {
                sb.Append("\r\n");
                sb.Append("VAR Reference_text = " + '"' + Visual_name + '"' + "\r\n");
                sb.Append("VAR Filtered_translations = FILTER( ALL( 'Visuals translations') ," + "\r\n");
                sb.Append("\t" + "'Visuals translations'[Reference text] = Reference_text &&" + "\r\n");
                sb.Append("\t" + "\t" + "\t" + "'Visuals translations'[Language code] = USERCULTURE()" + "\r\n");
                sb.Append("\t" + ")");
                sb.Append("RETURN COALESCE( CALCULATE(" + "\r\n");
                sb.Append("\t" + "\t" + "\t" + "FIRSTNONBLANK( 'Visuals translations'[Translated text] , TRUE()) ," + "\r\n");
                sb.Append("\t" + "\t" + "\t" + "\t" + "Filtered_translations )" + "\r\n");
                sb.Append("\t" + "\t" + " , Reference_text" + "\r\n");
                sb.Append("\t" + ")");
            }
            VT_measure.Expression =  sb.ToString();
            VT_measure.Description = "This measure is auto generated by a Tabular Editor script. Do not modify it manually";
            VT_measure.DisplayFolder = "";
            VT_measure.SetAnnotation("VTAUTOGEN", "1");  // Set a special annotation on the measure. It might be useful in the future
        }
    }
} else {
    Output("Visuals labels file doesn't exist. Please modify it at the top of the script"); 
}
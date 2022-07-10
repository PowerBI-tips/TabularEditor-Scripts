//'2022-07-10 / B.Agullo /  
// creates "Rows of ... " measures to return the numer of visible rows in each selected table
// adapted from Daniel Otykier's code 

// Loop through all currently selected columns:
foreach(Table t in Selected.Tables)
{
    Measure newMeasure = t.AddMeasure(
        "Rows of " + t.Name,                    // Name
        "COUNTROWS(" + t.DaxObjectFullName + ")",    // DAX expression
        "Other Measures"          // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "#,##0";

    // Provide some documentation:
    newMeasure.Description = "This measure is number of rows of " + t.Name;

}

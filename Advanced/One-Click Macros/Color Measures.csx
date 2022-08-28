// '2022-08-27 / B.Agullo / one-click enabled, guessing of name and hex color columns
// '2021-05-09 / B.Agullo / added transparent color
// by Bernat Agulló
// www.esbrina-ba.com

//adapted from Darren Gosbell's script at 
// https://darren.gosbell.com/2020/08/the-best-way-to-generate-data-driven-measures-in-power-bi-using-tabular-editor/

//This script creates the color measures for each of the colors included in the theme color table. 
// See http://www.esbrina-ba.com/theme-compliant-conditional-formatting-measures/

//adjust to fit your particular model


string colorNameTag = "Name" //string likely to indicate a color name column
string colorCodeTag = "Hex" //string likely to indicate a color code column

if(Selected.Tables.Count() != 1) { 
    Error("Select only table containing color definitions and try again"); 
};

string colorTableName = Selected.Table.Name; 

//get the first column containing "Name" in its name
string colorColumnNameCandidate = 
    Selected.Table.Columns
        .Where(x => x.Name.Contains(colorNameTag))
        .First().Name;


// let the user confirm if the guess is correct
string colorColumnName = 
    SelectColumn(
        Selected.Table,
        Selected.Table.Columns[colorColumnNameCandidate],
        "Select color Name Column"
    ).Name; 


//get the first column containing "Hex" in its name
string colorColumnCodeCandidate =
    Selected.Table.Columns
        .Where(x => x.Name.Contains(colorCodeTag))
        .First().Name;

// let the user confirm if the guess is correct
string hexCodeColumnName = 
    SelectColumn(
        Selected.Table,
        Selected.Table.Columns[colorColumnNameCandidate],
        "Select color code Column"
    ).Name; 

bool createTransparentColor = true; 

// do not change code below this line


string colorColumnNameWithTable = "'" + colorTableName + "'[" + colorColumnName + "]";
string hexCodeColumnNameWithTable = "'" + colorTableName + "'[" + hexCodeColumnName + "]";

string query = "EVALUATE VALUES(" + colorColumnNameWithTable + ")";
 
using (var reader = Model.Database.ExecuteReader(query))
{
    // Create a loop for every row in the resultset
    while(reader.Read())
    {
        string myColor = reader.GetValue(0).ToString();
        string measureName = myColor;
        string myExpression = "VAR HexCode = CALCULATE( SELECTEDVALUE( " + hexCodeColumnNameWithTable + "), " + colorColumnNameWithTable + " = \""  + myColor + "\") VAR Result = FORMAT(hexCode,\"@\") RETURN Result ";
        var newColorMeasure = Model.Tables[colorTableName].AddMeasure(measureName, myExpression);
       newColorMeasure.FormatDax(); 
        
    }
}
 
if(createTransparentColor){
    var transparentMeasure = Model.Tables[colorTableName].AddMeasure("Transparent","\"#FFFFFF00\""); 
};


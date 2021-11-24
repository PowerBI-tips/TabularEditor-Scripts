#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;

// this sctipt creates a CALCULATE expression based on an existing measure
// 2021-10-11 B.Agullo @AgulloBernat

if(Selected.Measures.Count != 1) {
    Error("Select one and only one measure"); 
    return; 
} 

var selectedMeasure = Selected.Measure;
var parentTable = selectedMeasure.Table; 

string newMeasureName = Interaction.InputBox("New Measure name", "Name", selectedMeasure.Name + " modified", 740, 400);
string newMeasureExpression = "CALCULATE([" + selectedMeasure.Name + "])"; 

parentTable.AddMeasure(newMeasureName,newMeasureExpression); 

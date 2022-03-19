#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;

// CHANGE LOG: 
// '2021-10-20 / B.Agullo / 
// '2021-11-22 / B.Agullo / Totally rewrote the script as did not work the way it was 


// Instructions:
// select the calculation items and and the measure(s) for which the calculation should be shown. 
// the first time you will be asked to introduce a name for your calc group and dummy measure 
// second time on it will reuse the same group and dummy measure to add new calc items 
// the script will create a new calc item for eachs elected measure
// A pop-up will show if the measure has been previously selected and thus already has a calculation item with its name


// TODO: SWAP PRECEDENCE OF NEW CALC GROUP AND SELECTED CALC GROUP 

//
// ----- do not modify script below this line -----
//

string affectedMeasures = ""; 

string selectedCalcItems = ""; 
string selectedCalcItemsCalcGroupName = ""; 

CalculationGroup selectedCalculationGroup = null as CalculationGroup; 
Column selectedCalcItemsCalcGroupColumn = null as Column; 



if (Selected.Measures.Count == 0) {
    
    Error("No measures selected"); 
    return; 

} else if (Selected.CalculationItems.Count == 0) { 

    Error("No calculation items selected"); 
    return; 

} else {
    
    //foreach(var m in Selected.Measures) {
    //    if(affectedMeasures == "") {
    //        affectedMeasures =  "[" + m.Name + "]";
    //    } else {
    //        affectedMeasures = affectedMeasures + ",[" + m.Name + "]" ;
    //    };
    //}; 

    
    // create in-line table with selected calc item names 
    foreach(var ci in Selected.CalculationItems) { 
        
        if(selectedCalcItems == "") {
            selectedCalcItems += "{\"" + ci.Name + "\""; 

        } else { 
            selectedCalcItems += ",\"" + ci.Name + "\""; 

        }; 
        
        if(selectedCalcItemsCalcGroupName == "") {
            
            
            //selectedCalculationGroup = ci.CalculationGroupTable;
            selectedCalcItemsCalcGroupName = ci.CalculationGroupTable.Name; 
        };
        
        if(selectedCalcItemsCalcGroupColumn == null) {
            //get the only column that is not "ordinal"
            selectedCalcItemsCalcGroupColumn = (ci.CalculationGroupTable as Table).Columns.Where(x => x.Name != "Ordinal").First(); 
            
        };


    }; 
    
    selectedCalcItems += "}"; 
 
};





string calcGroupTag = "Dynamic Measure Calculation Group For Arbitrary 2-row Header";
string dummyMeasureTag = "Dummy Measure for the Dynamic Measure Calculation Group For Arbitrary 2-row Header";




//dynamic Measure CG for 2 row header
var DynamicMeasureCGs = Model.Tables.Where(x => x.GetAnnotation("@AgulloBernat") == calcGroupTag);

var DynamicMeasureCG = null as CalculationGroupTable; 

if (DynamicMeasureCGs.Count() == 1 ) {
    DynamicMeasureCG = DynamicMeasureCGs.First() as CalculationGroupTable;
} else if (DynamicMeasureCGs.Count() < 1) {
    
    string calcGroupName = 
        Interaction.InputBox(
            "Provide a name for your Dynamic Measure Calculation Group For Arbitrary 2-row Header", 
            "Calculation Group Name", "", 740, 400
        );

    if(calcGroupName == "") {
        Error("No name provided");         
        return;
    };
    
    DynamicMeasureCG = Model.AddCalculationGroup(calcGroupName);
    DynamicMeasureCG.Description = 
        "Under this calc group only certain calculation items of " 
            + selectedCalcItemsCalcGroupName 
            + " calculation group will be visible and with a certain measure. See calculation items for details";
    
    DynamicMeasureCG.SetAnnotation("@AgulloBernat",calcGroupTag);

    Model.Tables[calcGroupName].Columns["Name"].Name = calcGroupName; 

} else { 
    //this should never happen --
    DynamicMeasureCG = SelectTable(DynamicMeasureCGs, label:"Select your Dynamic Measure Calculation Group For Arbitrary 2-row Header") as CalculationGroupTable;
};

if (DynamicMeasureCG == null) { return; } // doesn't work in TE3 as cancel button doesn't return null in TE3



//INIT DUMMY MEASURE (if necessary) 
var dummyMeasureCandidates = DynamicMeasureCG.Measures.Where(x => x.GetAnnotation("@AgulloBernat") == dummyMeasureTag);

Measure dummyMeasure = null as Measure; 


if (dummyMeasureCandidates.Count() == 1) { 
    
    dummyMeasure = dummyMeasureCandidates.First() as Measure; 

} else if (dummyMeasureCandidates.Count() < 1) {
    
    string dummyMeasureName = Interaction.InputBox("Enter a name for the dummy measure", "Measure Name", "", 740, 400);

    if(dummyMeasureName == "") {
        Error("No name provided");         
        return;
    };
    
    //add dummy measure if not present yet
    if(!DynamicMeasureCG.Measures.Where(m => m.Name == dummyMeasureName).Any())  {
        
        dummyMeasure = DynamicMeasureCG.AddMeasure(dummyMeasureName,"0");
        
    } else { 

        //TODO: ASK FOR CONFIRMATION 

        //get the reference if already exists 
        dummyMeasure = DynamicMeasureCG.Measures.Where(m => m.Name == dummyMeasureName).First(); 
    };

    //in any case add the annotation so it will be found next time 
    dummyMeasure.SetAnnotation("@AgulloBernat",dummyMeasureTag); 


} else{ 
    //very weird! already two dummy measures? 
    
    dummyMeasure = 
        SelectMeasure(
            dummyMeasureCandidates,
            label:"Select your Dummy Measure for your Dynamic Measure Calculation Group For Arbitrary 2-row Header"
        ) as Measure; 


} ; 

foreach (var m in Selected.Measures) { 
    
    if (!DynamicMeasureCG.CalculationItems.Where(x => x.Name == m.Name).Any()) { 
        string newCalcItemName = m.Name; 
        string newCalcItemExpression = 
             "IF(" + 
             "    ISSELECTEDMEASURE( [" + dummyMeasure.Name + "] )," + 
             "    VAR currentCalcItem =" + 
             "        SELECTEDVALUE( "+  selectedCalcItemsCalcGroupColumn.DaxObjectFullName +", \"NO SELECTION\" )" + 
             "    RETURN" + 
             "        IF( currentCalcItem IN " + selectedCalcItems + ", [" + m.Name + "] )," + 
             "    SELECTEDMEASURE()" + 
             ")";
        
        CalculationItem newCalcItem =  DynamicMeasureCG.AddCalculationItem(newCalcItemName, newCalcItemExpression); 
        newCalcItem.FormatDax(); 
    } else { 
        Info("Calculation item " + m.Name + " already present. Please modify manually, or delete it and try again"); 
    };

}; 


//se asegura que el calculation group original siga teniendo precedencia superior 
//int DynamicMeasureCGprecedence = DynamicMeasureCG.CalculationGroup.Precedence;
//int SelectedCalculationGroupPrecedence = selectedCalculationGroup.CalculationGroup.Precedence; 







CallDaxFormatter(); 


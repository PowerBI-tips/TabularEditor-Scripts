#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic; 
using System.Windows.Forms;

/* '2022-06-13 / B.Agullo / */
/* '2022-09-17 / B.Agullo / possibility to create a Field Parameter with a column for the base measure & calc Item
/* CREATE MEASURES WITH BASE MEASURES AND A CALCULATION GROUP */ 
/* https://www.esbrina-ba.com/creating-well-formatted-measures-based-on-a-calculation-group/  */
/* select measures and execute, you will need to run it twice */ 
/* first time to create aux calc group, second time to actually create measuree*/ 
/* remove aux calc group before going to production, do the right thing */ 

string auxCgTag = "@AgulloBernat";
string auxCgTagValue = "CG to extract format strings";

string auxCalcGroupName = "DELETE AUX CALC GROUP";
string auxCalcItemName = "Get Format String";

string baseMeasureAnnotationName = "Base Measure"; 
string calcItemAnnotationName = "Calc Item"; 
string scriptAnnotationName = "Script";
string scriptAnnotationValue = "Create Measures with a Calculation Group"; 

bool generateFieldParameter;

DialogResult dialogResult = MessageBox.Show("Generate Field Parameter?", "Field Parameter", MessageBoxButtons.YesNo);
generateFieldParameter = (dialogResult == DialogResult.Yes);


/*find any regular CGs (excluding the one we might have created)*/
var regularCGs = Model.Tables.Where(
    x => x.ObjectType == ObjectType.CalculationGroupTable
    & x.GetAnnotation(auxCgTag) != auxCgTagValue);

if (regularCGs.Count() == 0)
{
    Error("No Calculation Groups Found");
    return;
};

/*check if we already created the auxiliary calculation group*/
var auxCgs = Model.Tables.Where(x => x.GetAnnotation(auxCgTag) == auxCgTagValue);

CalculationGroupTable auxCg = null as CalculationGroupTable; 

/*if there are more than one for some reason we'll just use the first one*/
if(auxCgs.Count() >= 1)
{
    auxCg = auxCgs.First() as CalculationGroupTable; 
} else 
{
    /*create the aux calc group and ask for a refresh since it cannot be used in a query before that*/
    auxCg = Model.AddCalculationGroup(name: auxCalcGroupName);
    auxCg.AddCalculationItem(name: auxCalcItemName, expression: "SELECTEDMEASUREFORMATSTRING()");
    auxCg.SetAnnotation(auxCgTag, auxCgTagValue);

    /*better hidden in case someone forgets to delete it*/
    auxCg.IsHidden = true; 
    int maxPrecedence = 0; 

    /*check for the max precedence of other calc groups*/
    foreach (CalculationGroupTable cg in regularCGs)
    {
        if (cg.CalculationGroupPrecedence > maxPrecedence)
        {
            maxPrecedence = cg.CalculationGroupPrecedence;
        };
    };

    /*assign the highest precedence and some margin*/
    auxCg.CalculationGroupPrecedence = maxPrecedence + 10; 

    Info("Save changes to the model, recalculate the model, and launch the script again.");
    return;

};




/*check if any measures are selected*/
if (Selected.Measures.Count == 0)
{
    Error("No measures selected");
    return;
}

CalculationGroupTable regularCg = null as CalculationGroupTable;

/*allow user to select calculation group if more than one is found*/
if (regularCGs.Count() > 1)
{
    regularCg = SelectTable(regularCGs) as CalculationGroupTable;
}
/*otherwise just pick the first (and only)*/
else
{
    regularCg = regularCGs.First() as CalculationGroupTable;
}

/*check if no selection was made*/ 
if(regularCg == null)
{
    Error("No Target Calculation Group selected");
    return;
};

string name; 
if(generateFieldParameter) {
    name = Interaction.InputBox("Provide a name for the field parameter", "Field Parameter", regularCg.Name + " Measures", 740, 400);
    if(name == "") {Error("Execution Aborted"); return;};
}; 


MeasureCollection measures; 

/*iterates through each selected measure*/
foreach (Measure m in Selected.Measures)
{
    /*check that base measure has a proper format string*/ 
    if(m.FormatString == "") {
        Error("Define FormatString for " + m.Name + " and try again");
        return;
    };

    /*prepares a displayfolder to store all new measures*/
    string displayFolderName = m.Name + " Measures";

    /*iterates thorough all calculation items of the selected calc group*/ 
    foreach (CalculationItem calcItem in regularCg.CalculationItems)
    {
        
        /*prepares a query to calculate the resulting format when applying the calculation item on the measure*/ 
        string query = string.Format(
            "EVALUATE {{CALCULATE({0},{1},{2})}}",
            m.DaxObjectFullName,
            string.Format(
                "{0}=\"{1}\"",
                regularCg.Columns[0].DaxObjectFullName,
                calcItem.Name),
            string.Format(
                "{0}=\"{1}\"",
                auxCg.Columns[0].DaxObjectFullName,
                auxCalcItemName)
            );

        /*executes the query*/ 
        using (var reader = Model.Database.ExecuteReader(query))
        {
            // resultset should contain just one row, with the format string
            while (reader.Read())
            {
                /*retrive the formatstring from the query*/ 
                string formatString = reader.GetValue(0).ToString();

                /*build the expression of the measure*/
                string measureExpression = string.Format(
                    "CALCULATE({0},{1}=\"{2}\")",
                    m.DaxObjectName,
                    regularCg.Columns[0].DaxObjectFullName,
                    calcItem.Name);

                /*measure name*/ 
                string measureName = m.Name + " " + calcItem.Name;
                
                /*actually build the measure*/ 
                Measure newMeasure = 
                    m.Table.AddMeasure(
                        name: measureName,
                        expression: measureExpression);


                /*the all important format string!*/
                newMeasure.FormatString = formatString;

                /*final polish*/
                newMeasure.DisplayFolder = displayFolderName;
                newMeasure.FormatDax();
                
                /*add annotations for the creation of the field parameter*/
                newMeasure.SetAnnotation(baseMeasureAnnotationName,m.Name); 
                newMeasure.SetAnnotation(calcItemAnnotationName,calcItem.Name);
                newMeasure.SetAnnotation(scriptAnnotationName,scriptAnnotationValue);

            }
        }
    } 
}


if(!generateFieldParameter) {
    //end of execution
    return;
};


// Before running the script, select the measures or columns that you
// would like to use as field parameters (hold down CTRL to select multiple
// objects). Also, you may change the name of the field parameter table
// below. NOTE: If used against Power BI Desktop, you must enable unsupported
// features under File > Preferences (TE2) or Tools > Preferences (TE3).


if(Selected.Columns.Count == 0 && Selected.Measures.Count == 0) throw new Exception("No columns or measures selected!");

// Construct the DAX for the calculated table based on the measures created previously by the script
var objects = Model.AllMeasures.Where(x => x.GetAnnotation(scriptAnnotationName) == scriptAnnotationValue); 
var dax = "{\n    " + string.Join(",\n    ", objects.Select((c,i) => string.Format("(\"{0}\", NAMEOF('{1}'[{0}]), {2},\"{3}\",\"{4}\")", 
    c.Name, c.Table.Name, i,
    Model.Tables[c.Table.Name].Measures[c.Name].GetAnnotation("Base Measure"),
    Model.Tables[c.Table.Name].Measures[c.Name].GetAnnotation("Calc Item")))) + "\n}";

// Add the calculated table to the model:
var table = Model.AddCalculatedTable(name, dax);

// In TE2 columns are not created automatically from a DAX expression, so 
// we will have to add them manually:
var te2 = table.Columns.Count == 0;
var nameColumn = te2 ? table.AddCalculatedTableColumn(name, "[Value1]") : table.Columns["Value1"] as CalculatedTableColumn;
var fieldColumn = te2 ? table.AddCalculatedTableColumn(name + " Fields", "[Value2]") : table.Columns["Value2"] as CalculatedTableColumn;
var orderColumn = te2 ? table.AddCalculatedTableColumn(name + " Order", "[Value3]") : table.Columns["Value3"] as CalculatedTableColumn;

if(!te2) {
    // Rename the columns that were added automatically in TE3:
    nameColumn.IsNameInferred = false;
    nameColumn.Name = name;
    fieldColumn.IsNameInferred = false;
    fieldColumn.Name = name + " Fields";
    orderColumn.IsNameInferred = false;
    orderColumn.Name = name + " Order";
}
// Set remaining properties for field parameters to work
// See: https://twitter.com/markbdi/status/1526558841172893696
nameColumn.SortByColumn = orderColumn;
nameColumn.GroupByColumns.Add(fieldColumn);
fieldColumn.SortByColumn = orderColumn;
fieldColumn.SetExtendedProperty("ParameterMetadata", "{\"version\":3,\"kind\":2}", ExtendedPropertyType.Json);
fieldColumn.IsHidden = true;
orderColumn.IsHidden = true;
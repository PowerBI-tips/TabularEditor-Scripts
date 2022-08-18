/* '2022-06-13 / B.Agullo / */
/* CREATE MEASURES WITH BASE MEASURES AND A CALCULATION GROUP */ 
/* https://www.esbrina-ba.com/creating-well-formatted-measures-based-on-a-calculation-group/  */
/* select measures and execute, you will need to run it twice */ 
/* first time to create aux calc group, second time to actually create measuree*/ 
/* remove aux calc group before going to production, do the right thing */ 

string auxCgTag = "@AgulloBernat";
string auxCgTagValue = "CG to extract format strings";

string auxCalcGroupName = "DELETE AUX CALC GROUP";
string auxCalcItemName = "Get Format String";

/*find any regular CGs (excluding the one we might have created)*/
var regularCGs = Model.Tables.Where(
    x => x.ObjectType == ObjectType.CalculationGroupTable
    & x.GetAnnotation(auxCgTag) != auxCgTagValue);

if (regularCGs.Count() == 0)
{
    ScriptHelper.Error("No Calculation Groups Found");
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

    ScriptHelper.Info("Save changes to the model, recalculate the model, and launch the script again.");
    return;

};

/*check if any measures are selected*/
if (Selected.Measures.Count == 0)
{
    ScriptHelper.Error("No measures selected");
    return;
}

CalculationGroupTable regularCg = null as CalculationGroupTable;

/*allow user to select calculation group if more than one is found*/
if (regularCGs.Count() > 1)
{
    regularCg = ScriptHelper.SelectTable(regularCGs) as CalculationGroupTable;
}
/*otherwise just pick the first (and only)*/
else
{
    regularCg = regularCGs.First() as CalculationGroupTable;
}

/*check if no selection was made*/ 
if(regularCg == null)
{
    ScriptHelper.Error("No Target Calculation Group selected");
    return;
};

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

            }
        }
    } 
}
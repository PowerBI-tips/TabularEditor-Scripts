#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms;


/* '2023-01-26 / B.Agullo / creates a field parameter of measures filtered by calc group and values of a column with a name defined by a measure evaluated in the filtered value and calc item  */

/* DYNAMIC HEADER FIELD PARAMETER SCRIPT */ 

/* select measures and execute, you will need to run it twice */
/* first time to create aux calc group, second time to actually create measuree*/
/* remove aux calc group before going to production, do the right thing */

string auxCgTag = "@AgulloBernat";
string auxCgTagValue = "CG to extract format strings";

string auxCalcGroupName = "DELETE AUX CALC GROUP";
string auxCalcItemName = "Get Format String";

string baseMeasureAnnotationName = "Base Measure";
string calcItemAnnotationName = "Calc Item";
string calcItemSortOrderName = "Sort Order";
string calcItemSortOrderValue = String.Empty;

string filterValueAnnotationName = String.Empty;
string dynamicNameAnnotationName = "Dynamic Name";


string scriptAnnotationName = "Script";
string scriptAnnotationValue = "Create Measures with a Calculation Group " + DateTime.Now.ToString("yyyyMMddHHmmss") ;

bool generateFieldParameter;

DialogResult dialogResult = MessageBox.Show("Generate Field Parameter?", "Field Parameter", MessageBoxButtons.YesNo);
generateFieldParameter = (dialogResult == DialogResult.Yes);

/*check if any measures are selected*/
if (Selected.Measures.Count == 0)
{
    Error("No measures selected");
    return;
}

/*find any regular CGs (excluding the one we might have created)*/
var regularCGs = Model.Tables.Where(
    x => x.ObjectType == ObjectType.CalculationGroupTable
    & x.GetAnnotation(auxCgTag) != auxCgTagValue);

if (regularCGs.Count() == 0)
{
    Error("No Calculation Groups Found");
    return;
};




//the lambda expression will be avaluated for all calc groups to find a matching calc group
//CalculationGroupTable auxCg = Fx.SelectCalculationGroup(model:Model,lambdaExpression:lambda,selectFirst:true, showErrorIfNoTablesFound:false);

bool calcGroupWasCreated = false;

//the calc group will only be created if not found, and when so the boolean will point to it
CalculationGroupTable auxCg = Fx.AddCalculationGroupExt(model: Model, calcGroupWasCreated: out calcGroupWasCreated, 
    defaultName: auxCalcGroupName, customCalcGroupName: false, annotationName: auxCgTag, annotationValue: auxCgTagValue);

if (calcGroupWasCreated)
{
    CalculationItem cItem = Fx.AddCalculationItemExt(cg: auxCg, calcItemName: auxCalcItemName, valueExpression: "SELECTEDMEASUREFORMATSTRING()");
    auxCg.IsHidden = true; 
    
    Info("Save changes to the model, recalculate the model, and launch the script again.");
    return;
}

//to avoid showing the aux calc group in the list
Func<Table, bool> lambda = (x) => x.GetAnnotation(auxCgTag) != auxCgTagValue;

CalculationGroupTable regularCg = Fx.SelectCalculationGroup(model: Model, lambdaExpression: lambda);
if (regularCg == null) return;


Table filterTable = Fx.SelectTableExt(model: Model, excludeCalcGroups: true, label:"Select table of filter field",showErrorIfNoSelection:true);
if(filterTable == null) return;
Column filterColumn = SelectColumn(filterTable,label:"Select filter Field");
if (filterColumn == null) return;

filterValueAnnotationName = filterColumn.Name; 

String filterQuery = String.Format("EVALUATE DISTINCT({0})", filterColumn.DaxObjectFullName);

List<String> filterValues = new List<String>();

using (var filterReader = Model.Database.ExecuteReader(filterQuery))
{

    while (filterReader.Read())
    {

        filterValues.Add(filterReader.GetValue(0).ToString());
    }
}

string name = String.Empty;
if (generateFieldParameter)
{
    name = Interaction.InputBox("Provide a name for the field parameter", "Field Parameter", regularCg.Name + " Measures", 740, 400);
    if (name == "") { Error("Execution Aborted"); return; };
};

Measure dynamicNameMeasure = SelectMeasure(label: "Select measure for dynamic name, cancel if none");


/*iterates through each selected measure*/
foreach (Measure m in Selected.Measures)
{
    /*check that base measure has a proper format string*/
    if (m.FormatString == "")
    {
        Error("Define FormatString for " + m.Name + " and try again");
        return;
    };

    /*prepares a displayfolder to store all new measures*/
    string displayFolderName = m.Name + " Measures";

    /*iterates thorough all calculation items of the selected calc group*/
    foreach (CalculationItem calcItem in regularCg.CalculationItems)
    {

        string measureNamePrefix = string.Concat(Enumerable.Repeat("\u200B", calcItem.Ordinal));

        foreach (string filterValue in filterValues)
        {
            
            
            
            /*measure name*/
            string measureName = measureName = m.Name + " " + calcItem.Name + " " + filterValue;

            string dynamicMeasureName = String.Empty;  

            if (dynamicNameMeasure == null)
            {
                dynamicMeasureName = measureName;
            }
            else
            {

                string measureNameQuery = String.Empty;

                if (filterColumn.DataType == DataType.String)
                {

                    measureNameQuery =
                        String.Format("EVALUATE {{CALCULATE({0},{1}=\"{2}\",{3}=\"{4}\") & \"\"}}", 
                            dynamicNameMeasure.DaxObjectFullName, 
                            filterColumn.DaxObjectFullName, 
                            filterValue,
                            regularCg.Columns[0].DaxObjectFullName,
                            calcItem.Name);
                }
                else
                {
                    measureNameQuery =
                        String.Format("EVALUATE {{CALCULATE({0},{1}={2},{3}=\"{4}\") & \"\"}}",
                            dynamicNameMeasure.DaxObjectFullName,
                            filterColumn.DaxObjectFullName,
                            filterValue,
                            regularCg.Columns[0].DaxObjectFullName,
                            calcItem.Name);
                }


                Output(measureNameQuery);

                using (var reader = Model.Database.ExecuteReader(measureNameQuery))
                {
                    while (reader.Read())
                    {
                        dynamicMeasureName = reader.GetString(0).ToString();

                    }
                }

                dynamicMeasureName =  m.Name + " " +  measureNamePrefix + dynamicMeasureName;


            }

            

            //only if the measure is not yet there (think of reruns)
            if (!Model.AllMeasures.Any(x => x.Name == measureName))
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

                        Output(formatString);




                        /*build the expression of the measure*/
                        string measureExpression = String.Empty;

                        if(filterColumn.DataType == DataType.String)
                        {
                            measureExpression = string.Format(
                                "CALCULATE({0},{1}=\"{2}\",KEEPFILTERS({3}=\"{4}\"))",
                                m.DaxObjectName,
                                regularCg.Columns[0].DaxObjectFullName,
                                calcItem.Name,
                                filterColumn.DaxObjectFullName,
                                filterValue
                            );
                        }
                        else
                        {
                            measureExpression = string.Format(
                                "CALCULATE({0},{1}=\"{2}\",KEEPFILTERS({3}={4}))",
                                m.DaxObjectName,
                                regularCg.Columns[0].DaxObjectFullName,
                                calcItem.Name,
                                filterColumn.DaxObjectFullName,
                                filterValue
                            );
                        }

                            
                            
                            



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
                        newMeasure.SetAnnotation(baseMeasureAnnotationName, m.Name);
                        newMeasure.SetAnnotation(calcItemAnnotationName, calcItem.Name);
                        newMeasure.SetAnnotation(scriptAnnotationName, scriptAnnotationValue);
                        newMeasure.SetAnnotation(calcItemSortOrderName, calcItem.Ordinal.ToString("000"));
                        newMeasure.SetAnnotation(filterValueAnnotationName, filterValue);
                        newMeasure.SetAnnotation(dynamicNameAnnotationName, dynamicMeasureName);


                    }
                }
            }
        }
            
        
    }
}


if (!generateFieldParameter)
{
    //end of execution
    return;
};


// Before running the script, select the measures or columns that you
// would like to use as field parameters (hold down CTRL to select multiple
// objects). Also, you may change the name of the field parameter table
// below. NOTE: If used against Power BI Desktop, you must enable unsupported
// features under File > Preferences (TE2) or Tools > Preferences (TE3).


if (Selected.Columns.Count == 0 && Selected.Measures.Count == 0) throw new Exception("No columns or measures selected!");

// Construct the DAX for the calculated table based on the measures created previously by the script
var objects = Model.AllMeasures
    .Where(x => x.GetAnnotation(scriptAnnotationName) == scriptAnnotationValue)
    .OrderBy(x => x.GetAnnotation(baseMeasureAnnotationName) + x.GetAnnotation(calcItemSortOrderName));

var dax = "{\n    " + string.Join(",\n    ", objects.Select((c, i) => string.Format("(\"{6}\", NAMEOF('{1}'[{0}]), {2},\"{3}\",\"{4}\",\"{5}\")",
    c.Name, c.Table.Name, i,
    Model.Tables[c.Table.Name].Measures[c.Name].GetAnnotation(baseMeasureAnnotationName),
    Model.Tables[c.Table.Name].Measures[c.Name].GetAnnotation(calcItemAnnotationName),
    Model.Tables[c.Table.Name].Measures[c.Name].GetAnnotation(filterValueAnnotationName),
    Model.Tables[c.Table.Name].Measures[c.Name].GetAnnotation(dynamicNameAnnotationName)
    ))) + "\n}";

// Add the calculated table to the model:
var table = Model.AddCalculatedTable(name, dax);

// In TE2 columns are not created automatically from a DAX expression, so 
// we will have to add them manually:
var te2 = table.Columns.Count == 0;
var nameColumn = te2 ? table.AddCalculatedTableColumn(name, "[Value1]") : table.Columns["Value1"] as CalculatedTableColumn;
var fieldColumn = te2 ? table.AddCalculatedTableColumn(name + " Fields", "[Value2]") : table.Columns["Value2"] as CalculatedTableColumn;
var orderColumn = te2 ? table.AddCalculatedTableColumn(name + " Order", "[Value3]") : table.Columns["Value3"] as CalculatedTableColumn;

if (!te2)
{
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


public static class Fx
{
    



    //in TE2 (at least up to 2.17.2) any method that accesses or modifies the model needs a reference to the model 
    //the following is an example method where you can build extra logic
    public static Table CreateCalcTable(Model model, string tableName, string tableExpression) 
    { 
        return model.AddCalculatedTable(name:tableName,expression:tableExpression);
    }

    public static Table SelectTableExt(Model model, string possibleName = null, string annotationName = null, string annotationValue = null, 
        Func<Table,bool>  lambdaExpression = null, string label = "Select Table", bool skipDialogIfSingleMatch = true, bool showOnlyMatchingTables = true,
        IEnumerable<Table> candidateTables = null, bool showErrorIfNoTablesFound = false, string errorMessage = "No tables found", bool selectFirst = false,
        bool showErrorIfNoSelection = true, string noSelectionErrorMessage = "No table was selected", bool excludeCalcGroups = false,bool returnNullIfNoTablesFound = false)
    {

        Table table = null as Table;

        if (lambdaExpression == null)
        {
            if (possibleName != null) { 
                lambdaExpression = (t) => t.Name == possibleName;
            } else if(annotationName!= null && annotationValue != null)
            {
                lambdaExpression = (t) => t.GetAnnotation(annotationName) == annotationValue;
            }
            else
            {
                lambdaExpression = (t) => true; //no filtering
            }
        }

        //use candidateTables if passed as argument
        IEnumerable<Table> tables = null as IEnumerable<Table>;

        if(candidateTables != null)
        {
            tables = candidateTables;
        }
        else
        {
            tables = model.Tables;
        }

        if(lambdaExpression != null)
        {
            tables = tables.Where(lambdaExpression);
        }

        if (excludeCalcGroups)
        {
            tables = tables.Where(t => t.ObjectType != ObjectType.CalculationGroupTable);
        }

        //none found, let the user choose from all tables
        if (tables.Count() == 0)
        {

            if (returnNullIfNoTablesFound)
            {
                if (showErrorIfNoTablesFound) Error(errorMessage);
                Output("No tables found");
                return table;
            } 
            else
            {
                Output("returnNullIfNoTablesFound is false");
                table =  SelectTable(tables: model.Tables, label: label);
            }
            
        }
        else if (tables.Count() == 1 && !skipDialogIfSingleMatch)
        {
            Output("tables.Count() == 1 && !skipDialogIfSingleMatch");
            table = SelectTable(tables: model.Tables, preselect: tables.First(), label: label);
        }
        else if (tables.Count() == 1 && skipDialogIfSingleMatch)
        {
            table = tables.First();
        } 
        else if (tables.Count() > 1) 
            
        {
            if (selectFirst)
            {
                table = tables.First();
            }
            else if (showOnlyMatchingTables)
            {
                Output("showOnlyMatchingTables");
                table = SelectTable(tables: tables, preselect: tables.First(), label: label);
            }
            else
            {
                Output("else");
                table = SelectTable(tables: model.Tables, preselect: tables.First(), label: label);
            }
            
        }
        else
        {
            Error(@"Unexpected logic in ""SelectTableExt""");
            return null;
        }

        if(showErrorIfNoSelection && table == null)
        {
            Error(noSelectionErrorMessage);
        }

        return table;

    }


    public static CalculationGroupTable SelectCalculationGroup(Model model, string possibleName = null, string annotationName = null, string annotationValue = null,
        Func<Table, bool> lambdaExpression = null, string label = "Select Table", bool skipDialogIfSingleMatch = true, bool showOnlyMatchingTables = true,
        bool showErrorIfNoTablesFound = true, string errorMessage = "No calculation groups found",bool selectFirst = false, 
        bool showErrorIfNoSelection = true, string noSelectionErrorMessage = "No calculation group was selected", bool returnNullIfNoTablesFound = false)
    {

        CalculationGroupTable calculationGroupTable = null as CalculationGroupTable;
        
        Func<Table, bool> lambda = (x) => x.ObjectType == ObjectType.CalculationGroupTable;
        if (!model.Tables.Any(lambda)) return calculationGroupTable;

        IEnumerable<Table> tables = model.Tables.Where(lambda);

        Table table = Fx.SelectTableExt(
            model:model,
            possibleName:possibleName,
            annotationName:annotationName,
            annotationValue:annotationValue,
            lambdaExpression:lambdaExpression,
            label:label,
            skipDialogIfSingleMatch:skipDialogIfSingleMatch,
            showOnlyMatchingTables:showOnlyMatchingTables,
            showErrorIfNoTablesFound:showErrorIfNoTablesFound,
            errorMessage:errorMessage, 
            selectFirst:selectFirst,
            showErrorIfNoSelection:showErrorIfNoSelection,
            noSelectionErrorMessage:noSelectionErrorMessage, 
            returnNullIfNoTablesFound:returnNullIfNoTablesFound, 
            candidateTables:tables);

        if(table == null) return calculationGroupTable;

        calculationGroupTable = table as CalculationGroupTable;

        return calculationGroupTable;

    }

    public static CalculationGroupTable AddCalculationGroupExt(Model model, out bool calcGroupWasCreated, string defaultName = "New Calculation Group", 
        string annotationName = null, string annotationValue = null, bool createOnlyIfNotFound = true, 
        string prompt = "Name", string Title = "Provide a name for the Calculation Group", bool customCalcGroupName = true)
    {
        
        Func<Table,bool> lambda = null as Func<Table,bool>;
        CalculationGroupTable cg = null as CalculationGroupTable;
        calcGroupWasCreated = false;
        string calcGroupName = String.Empty;

        if (createOnlyIfNotFound)
        {

            if (annotationName == null && annotationValue == null)
            {

                if (customCalcGroupName)
                {
                    calcGroupName = Interaction.InputBox(Prompt: "Name", Title: "Provide a name for the Calculation Group");
                }
                else
                {
                    calcGroupName = defaultName;
                }

                cg = Fx.SelectCalculationGroup(model: model, possibleName: calcGroupName, showErrorIfNoTablesFound: false, selectFirst: true);

            }
            else
            {
                cg = Fx.SelectCalculationGroup(model: model, 
                    showErrorIfNoTablesFound: false, 
                    annotationName: annotationName, 
                    annotationValue: annotationValue, 
                    returnNullIfNoTablesFound: true);
            }

            if (cg != null) return cg;
        }
        
        if (calcGroupName == String.Empty)
        {
            if (customCalcGroupName)
            {
                calcGroupName = Interaction.InputBox(Prompt: "Name", Title: "Provide a name for the Calculation Group");
            }
            else
            {
                calcGroupName = defaultName;
            }
        }

        cg = model.AddCalculationGroup(name: calcGroupName);

        if (annotationName != null && annotationValue != null)
        {
            cg.SetAnnotation(annotationName,annotationValue);
        }

        calcGroupWasCreated = true;

        return cg;

    }

    public static CalculationItem AddCalculationItemExt(CalculationGroupTable cg, string calcItemName, string valueExpression = "SELECTEDMEASURE()",
        string formatStringExpression = "", bool createOnlyIfNotFound = true, bool rewriteIfFound = false)
    {

        CalculationItem calcItem = null as CalculationItem;

        Func<CalculationItem, bool> lambda = (ci) => ci.Name == calcItemName;

        if(createOnlyIfNotFound)
        {
            if (cg.CalculationItems.Any(lambda))
            {

                calcItem = cg.CalculationItems.Where(lambda).FirstOrDefault();

                if (!rewriteIfFound)
                {
                    return calcItem;
                }
            }
        }


        if(calcItem == null)
        {
            calcItem = cg.AddCalculationItem(name: calcItemName, expression: valueExpression);
        }
        else 
        {
            //rewrite the found calcItem
            calcItem.Expression = valueExpression;
        }

        if(formatStringExpression != String.Empty)
        {
            calcItem.FormatStringExpression = formatStringExpression;
        }
        
        return calcItem;
            
    }

}
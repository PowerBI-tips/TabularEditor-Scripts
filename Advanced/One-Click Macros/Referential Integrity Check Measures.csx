/* 
//2022-06-29 B.Agullo 
//
// BLOG POST on the actual use case and how to use it
// https://www.esbrina-ba.com/data-validation-with-power-bi/
//
//Selecting a table will select the table for the overall summary measures.
//The script will go through all the tables of the model and 
//for each table if will scan the measures contained in the display folder that starts with "Checks " 
// and will generate a new display folder in the same table
//called "Titles " followed by the original display folder name
//where all measures original measures will have an equivalent measure
//with the expression [original measure] & Original measure name
//also another displayfolder called "summary measures" 
//will contain 2 measures. One is the sum of all selected measures 
//the other one is the concatenation of all the titles measures
//Useful when setting alarms when one measure is greater than 0
//on the original selected table it will store the sum of all the summary measures of the model
//read the blog post for a more clear idea of how to use it.
*/

/*customize if needed*/
string dynamicMeasureCustomActionName = "Dynamic Measure"; 

string alertDisplayFolderPrefix = "Checks ";
string allMeasuresPrefix = "Alerts Value ";
string allTitleMeasuresPrefix = "Alerts Descriptions ";
string allMeasureCountPrefix = "Alert Count "; 

string summaryMeasuresDisplayFolder = "Summary Measures";
string titleMeasuresPrefix = "Title ";
string titleMeasuresDisplayFolderPrefix = "Titles of ";



string overallAlertValueMeasureName = "Total Alerts Value";
string overallAlertDescMeasureName = "Total Alerts Description";
string overallAlertCountMeasureName = "Total Alert Count"; 


/*do not modify below this line*/



string annotationKey = "@AgulloBernat";
string annotationValue = "Alert Summary Sum Measure";
string annotationValueDesc = "Alert Description Concat Measure";
string annotationValueCount = "Alert Count Measure";


if(Selected.Tables.Count() != 1)
{
    ScriptHost.Error("Select a table for overall alert measures and try again.");
    return;
}

//create calculation group without any calc items
Model.CustomAction(dynamicMeasureCustomActionName); 

/* go through each table ... */
foreach (Table t in Model.Tables)
{

    string displayFolderName = "";
    string titleMeasuresDisplayFolderName = "";
    string allMeasureExpression = "";
    string allMeasureName = "";
    string allTitleMeasureExpression = "\"\"";
    string allTitleMeasureName = "";
    string allMeasureCountExpression = "";
    string allMeasureCountName = ""; 


    /*check if there's any measure to process*/
    List<Measure> alertMeasures = new List<Measure>();
    foreach (Measure m in t.Measures)
    {
        if (m.DisplayFolder.Length >= alertDisplayFolderPrefix.Length)
        {
            if( m.DisplayFolder.Substring(0, alertDisplayFolderPrefix.Length) == alertDisplayFolderPrefix)
            {

                alertMeasures.Add(m);
            }
        }
    }

    /* if any was found */
    if (alertMeasures.Count() > 0) { 

        /*for each measure found, process it */
        foreach (Measure m in alertMeasures) { 

            if (displayFolderName == "")
            {
                displayFolderName = m.DisplayFolder;
                titleMeasuresDisplayFolderName = titleMeasuresDisplayFolderPrefix + displayFolderName;
            };

            string titleMeasureName = titleMeasuresPrefix + m.Name;
            string titleMeasureExpression = m.DaxObjectName + " & \" " + m.Name + "\"";

            foreach (Measure delM in Model.AllMeasures.Where(x => x.Name == titleMeasureName).ToList())
            {
                delM.Delete();
            };

            Measure titleM = m.Table.AddMeasure(
                name: titleMeasureName,
                expression: titleMeasureExpression,
                displayFolder: titleMeasuresDisplayFolderName
            );
            
            titleM.FormatDax();

            allMeasureExpression = allMeasureExpression + Environment.NewLine + " + " + m.DaxObjectName;

            allTitleMeasureExpression =
                allTitleMeasureExpression
                    + " & IF(" + m.DaxObjectFullName + "> 0,"
                    + titleM.DaxObjectFullName + " & UNICHAR(10))";
                    
            //m.CustomAction("Dynamic Measure");
            
            allMeasureCountExpression = 
                allMeasureCountExpression 
                + Environment.NewLine + " + IF(" + m.DaxObjectFullName + "> 0, 1)";
                
        };

        /*now create the summary measures for that table*/
        allMeasureName = allMeasuresPrefix + t.Name;
        allTitleMeasureName = allTitleMeasuresPrefix + t.Name;
        allMeasureCountName = allMeasureCountPrefix + t.Name;

        foreach (Measure delM in Model.AllMeasures.Where(x => x.Name == allMeasureName).ToList())
        {
            delM.Delete();
        };

        foreach (Measure delM in Model.AllMeasures.Where(x => x.Name == allTitleMeasureName).ToList())
        {
            delM.Delete();
        };
        
        foreach (Measure delM in Model.AllMeasures.Where(x => x.Name == allMeasureCountName).ToList())
        {
            delM.Delete();
        };

        Measure measure =
            t.AddMeasure(
                name: allMeasureName,
                expression: allMeasureExpression,
                displayFolder: summaryMeasuresDisplayFolder);

        measure.SetAnnotation(annotationKey, annotationValue);
        measure.FormatString = "#,##0";
        measure.FormatDax();
        measure.CustomAction(dynamicMeasureCustomActionName);
        
        Measure titleMeasure =
            t.AddMeasure(
                name: allTitleMeasureName,
                expression: allTitleMeasureExpression,
                displayFolder: summaryMeasuresDisplayFolder);

        titleMeasure.SetAnnotation(annotationKey, annotationValueDesc);
        titleMeasure.FormatDax(); 
        titleMeasure.CustomAction(dynamicMeasureCustomActionName); 
        
        Measure countMeasure =
            t.AddMeasure(
                name: allMeasureCountName,
                expression: allMeasureCountExpression,
                displayFolder: summaryMeasuresDisplayFolder);

        countMeasure.SetAnnotation(annotationKey, annotationValueCount);
        countMeasure.FormatDax(); 
        countMeasure.CustomAction(dynamicMeasureCustomActionName); 
        
    }
}


/*once we processed all tables, time to create overall summary measures*/
/*clean up*/
if (Model.AllMeasures.Any(x => x.Name == overallAlertValueMeasureName))
{
    Model.AllMeasures.Where(
        x => x.Name == overallAlertValueMeasureName
        ).First().Delete();

}

if (Model.AllMeasures.Any(
    x => x.Name == overallAlertDescMeasureName))
{
    Model.AllMeasures.Where(
        x => x.Name == overallAlertDescMeasureName
        ).First().Delete();

}

if (Model.AllMeasures.Any(
    x => x.Name == overallAlertCountMeasureName))
{
    Model.AllMeasures.Where(
        x => x.Name == overallAlertCountMeasureName
        ).First().Delete();

}


/*regenerate if necessary*/
if (Model.AllMeasures.Any(
    x => x.DisplayFolder.IndexOf(summaryMeasuresDisplayFolder) == 0
           & x.Name.IndexOf(allMeasuresPrefix) == 0))
{

    string overallAlertValueMeasureExpression = "";
    string overallAlertDescMeasureExpression = "\"\"";
    string overallAlertCountMeasureExpression = "";

    foreach (Measure m in
        Model.AllMeasures.Where(
            x => x.DisplayFolder.IndexOf(summaryMeasuresDisplayFolder) == 0
                & x.Name.IndexOf(allMeasuresPrefix) == 0))
    {


        overallAlertValueMeasureExpression =
            overallAlertValueMeasureExpression
                + " + " + m.DaxObjectFullName;

    }

    foreach (Measure m in
         Model.AllMeasures.Where(
             x => x.DisplayFolder.IndexOf(summaryMeasuresDisplayFolder) == 0
                 & x.Name.IndexOf(allTitleMeasuresPrefix) == 0))
    {
        overallAlertDescMeasureExpression =
            overallAlertDescMeasureExpression
                + " & IF(LEN(" +m.DaxObjectFullName +")>0, UNICHAR(10) & UNICHAR(10) & \"********** " 
                    + m.Table.Name + "*********\" & UNICHAR(10) & " +  m.DaxObjectFullName + ")";

    }



    foreach (Measure m in
         Model.AllMeasures.Where(
             x => x.DisplayFolder.IndexOf(summaryMeasuresDisplayFolder) == 0
                 & x.Name.IndexOf(allMeasureCountPrefix) == 0))
    {
        overallAlertCountMeasureExpression =
            overallAlertCountMeasureExpression
                + " + " + m.DaxObjectFullName;

    }


    Measure alertValueMeasure = Selected.Table.AddMeasure(overallAlertValueMeasureName, overallAlertValueMeasureExpression);
    alertValueMeasure.FormatDax();
    alertValueMeasure.FormatString ="#,##0";

    Measure alertDescMeasure = Selected.Table.AddMeasure(overallAlertDescMeasureName, overallAlertDescMeasureExpression);
    alertDescMeasure.FormatDax();

    Measure alertCountMeasure =  Selected.Table.AddMeasure(overallAlertCountMeasureName, overallAlertCountMeasureExpression);
    alertCountMeasure.FormatDax();
    alertCountMeasure.FormatString ="#,##0";


};
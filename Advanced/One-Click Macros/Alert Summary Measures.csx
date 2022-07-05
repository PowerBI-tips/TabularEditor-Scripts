/* 
//2022-06-29 B.Agullo 
//Selecting measures from a single display folder
//will generate a new display folder in the same table
//called "Titles " followed by the original display folder
//where all measures original measures will have an equivalent measure
//with the expression [original measure] & Original measure name
//also another displayfolder called "summary measures" 
//will contain 2 measures. One is the sum of all selected measures 
//the other one is the concatenation of all the titles measures
//Useful when setting alarms when one measure is greater than 0
*/

/*customize if needed*/
string alertDisplayFolderPrefix = "Checks ";
string allMeasuresPrefix = "Alerts ";
string allTitleMeasuresPrefix = "Alerts Descriptions ";
string summaryMeasuresDisplayFolder = "Summary Measures";
string titleMeasuresPrefix = "Title ";
string titleMeasuresDisplayFolderPrefix = "Titles of ";

string overallAlertValueMeasureName = "Total Alerts";
string overallAlertDescMeasureName = "Total Alerts Description";


/*do not modify below this line*/



string annotationKey = "@AgulloBernat";
string annotationValue = "Alert Summary Sum Measure";
string annotationValueDesc = "Alert Description Concat Measure";


if(Selected.Tables.Count() != 1)
{
    ScriptHelper.Error("Select a table for overall alert measures and try again.");
    return;
}

/* go through each table ... */
foreach (Table t in Model.Tables)
{

    string displayFolderName = "";
    string titleMeasuresDisplayFolderName = "";
    string allMeasureExpression = "";
    string allMeasureName = "";
    string allTitleMeasureExpression = "\"\"";
    string allTitleMeasureName = "";


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
                    
            CustomAction(m,"Dynamic Measure");

        };

        /*now create the summary measures for that table*/
        allMeasureName = allMeasuresPrefix + t.Name;
        allTitleMeasureName = allTitleMeasuresPrefix + t.Name;

        foreach (Measure delM in Model.AllMeasures.Where(x => x.Name == allMeasureName).ToList())
        {
            delM.Delete();
        };

        foreach (Measure delM in Model.AllMeasures.Where(x => x.Name == allTitleMeasureName).ToList())
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
        measure.CustomAction("Dynamic Measure");
        
        Measure titleMeasure =
            t.AddMeasure(
                name: allTitleMeasureName,
                expression: allTitleMeasureExpression,
                displayFolder: summaryMeasuresDisplayFolder);

        measure.SetAnnotation(annotationKey, annotationValueDesc);
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

/*regenerate if necessary*/
if (Model.AllMeasures.Any(
    x => x.DisplayFolder.IndexOf(summaryMeasuresDisplayFolder) == 0
           & x.Name.IndexOf(allMeasuresPrefix) == 0
           & x.Name.IndexOf(allTitleMeasuresPrefix) != 0))
{

    string overallAlertValueMeasureExpression = "0";
    string overallAlertDescMeasureExpression = "\"\"";

    foreach (Measure m in
        Model.AllMeasures.Where(
            x => x.DisplayFolder.IndexOf(summaryMeasuresDisplayFolder) == 0
                & x.Name.IndexOf(allMeasuresPrefix) == 0
                & x.Name.IndexOf(allTitleMeasuresPrefix) != 0))
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
                + " & " + m.DaxObjectFullName;

    }


    Measure alertValueMeasure = Selected.Table.AddMeasure(overallAlertValueMeasureName, overallAlertValueMeasureExpression);
    alertValueMeasure.FormatDax();
    alertValueMeasure.FormatString ="#,##0";

    Measure alertDescMeasure = Selected.Table.AddMeasure(overallAlertDescMeasureName, overallAlertDescMeasureExpression);
    alertDescMeasure.FormatDax();

};

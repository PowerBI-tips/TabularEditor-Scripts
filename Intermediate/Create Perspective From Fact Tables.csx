/*
 * Title: Create Perspective From Fact Tables
 * 
 * Author: Curtis Stallings, https://www.linkedin.com/in/curtisrs/
 * 
 * This script will take the selected tables and create a perspective that includes all the related tables, measures, & calc groups. 
 * It will take into consideration nested measures and tables. 
 * This script is useful for adding context to rather large models with multiple fact tables,
 * Letting you narrow down the view for an end user, so they are not overwhelmed
 *
 */

//You can select 1 or multiple tables, each table will get it's own perspective.
foreach(var table in Selected.Tables){
    
  //Check if Table Name already exists as a persp, Creates Perspective, Then Adds to Perspective
    if (!Model.Perspectives.Any(a => a.Name == table.Name))
        {
            Model.AddPerspective(table.Name);
        }
    Model.Tables[table.Name].InPerspective[table.Name] = true;
    
    //Gets and adds Related Tables to Perspective
    foreach(var related in table.RelatedTables){
        related.InPerspective[table.Name] = true;
    }
    
    //Gets and adds All Related Measures... Also includes Nested Measures.
    foreach(var measure in Model.AllMeasures){
        foreach(var measure_dependency in measure.DependsOn.Deep()){
            if(measure_dependency.ObjectType.ToString() == "Table" 
            && Model.Tables[measure_dependency.Name].InPerspective[table.Name] == true
            && Model.Tables[measure_dependency.Name] == Model.Tables[table.Name]){
                measure.InPerspective[table.Name] = true;
            }
        }
    }
    
    //Gets and adds All Calculation Groups... And the Calculation Group Dependencies
    foreach(var calc_group in Model.CalculationGroups){
        foreach(var calc_item in calc_group.CalculationItems){
            foreach(var calc_dependency in calc_item.DependsOn.Deep()){
                if(calc_dependency.ObjectType.ToString() == "Table"
                && Model.Tables[calc_dependency.Name].InPerspective[table.Name]){
                    calc_group.InPerspective[table.Name] = true;
                }
            }
        }
    }
}

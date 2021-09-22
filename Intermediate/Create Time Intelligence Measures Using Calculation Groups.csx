/*
 * Generate time intelligence measures based on calculation group items already created
 *
 * Author: Benoit Fedit, https://datakuity.com/ 
 *
 * You must have created the calculation group items beforehand (see link below)
 * https://docs.microsoft.com/en-us/analysis-services/tabular-models/calculation-groups?view=asallproducts-allversions
 * To add more measure simply copy/paste the YTD script and replace YTD by your calculation item name
 */


// For each selected measure create YTY, PY, PY YTD, YOY, YOY% measures
foreach(var m in Selected.Measures) {
    
    // YTD
    m.Table.AddMeasure(
    m.Name + " YTD",                                       // Name
    "Calculate(" + m.DaxObjectName + ", 'Time Intelligence'[Time Calculation]=\"YTD\")",    
    m.DisplayFolder                                        // Display Folder
    );
    
    // PY
    m.Table.AddMeasure(
    m.Name + " YTD",                                       // Name
    "Calculate(" + m.DaxObjectName + ", 'Time Intelligence'[Time Calculation]=\"PY\")",    
    m.DisplayFolder                                        // Display Folder
    );
    
    // PY YTD
    m.Table.AddMeasure(
    m.Name + " PY YTD",                                       // Name
    "Calculate(" + m.DaxObjectName + ", 'Time Intelligence'[Time Calculation]=\"PY YTD\")",    
    m.DisplayFolder                                        // Display Folder
    );
    
    // YOY
    m.Table.AddMeasure(
    m.Name + " YOY",                                       // Name
    "Calculate(" + m.DaxObjectName + ", 'Time Intelligence'[Time Calculation]=\"YOY\")",    
    m.DisplayFolder                                        // Display Folder
    ).FormatString = "0.0 %";
        
    // YOY%
    m.Table.AddMeasure(
    m.Name + " YOY%",                                       // Name
    "Calculate(" + m.DaxObjectName + ", 'Time Intelligence'[Time Calculation]=\"YOY%\")",    
    m.DisplayFolder                                        // Display Folder
    );
}

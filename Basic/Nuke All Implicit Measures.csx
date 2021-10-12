// Title: Nuke All Implicit Measures.csx
// Author: @JamesDBartlett3
// Description: Uses two methods to disable all Implicit Measures in the model

// 1. Set "Discourage Implicit Measures" option in Model = true
Model.DiscourageImplicitMeasures = true;
    
// 2. Set "Summarize By" property on all columns = AggregateFunction.None
foreach(var column in Model.Tables.SelectMany(t => t.Columns)) {
    column.SummarizeBy = AggregateFunction.None;
}

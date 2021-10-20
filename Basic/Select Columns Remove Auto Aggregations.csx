/*
 * Title: Add Prefix to Selected Measures
 *
 * Author: Mike Carlo, https://PowerBI.tips
 *
 * User can select columns. From the selection each column will be set to Aggregate = None or Summarize by = None, as observed in Power BI Desktop.
 * Only Selected columns will be modified.
 *
 */

   

foreach(var column in Selected.columns) {

    column.SummarizeBy = AggregateFunction.None;

}

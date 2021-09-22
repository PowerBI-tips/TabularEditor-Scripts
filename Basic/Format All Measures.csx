/*
 * Title: Format All Measures  
 *
 * Author: Matt Allington, https://exceleratorbi.com.au  
 *
 * This script loops through all the measures in your model and calls out to daxformatter.com
 * in order to format them.
 *
 * CAUTION: If your model has many measures (> 100) please use with care, as this will perform
 * many requests against the www.daxformatter.com web service. It will take some time to run,
 * and also, we don't want to DDoS attack daxformatter.com :-)
 */

//Format All Measures
foreach (var m in Model.AllMeasures)
{
    m.Expression = FormatDax(m.Expression);
    /* Cycle over all measures in model and format 
    them all using DAX Formatter */
}

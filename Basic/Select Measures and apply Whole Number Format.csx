/* For each selected measure change the formatting to a whole number
 *
 * Author: Mike Carlo, https://powerbi.tips
 *
 * Select measures using Control + Left Click
 * Script change the measure formatting to a whole number
 *
 */

// for each selected measure specifies a format.
foreach (var m in Selected.Measures)
{

    // Set the format string on the new measure:
    m.FormatString = "#,##0";

}

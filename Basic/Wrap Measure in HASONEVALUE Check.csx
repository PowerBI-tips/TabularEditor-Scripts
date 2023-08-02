/*
 * Title: Add "HASONEVALUE" check to Selected Measures
 *
 * Author: Dan Meissner
 * Last Modified: 2022DEC20
 *
 * This script loops through all the selected measures and adds an IF statement check to see if it "HASONEVALUE"
 * relative to the single selected column.

 * Only selected measures will be modified. 
 * Must select only one column in addition to any number of measures for which you want to check for one value.
 *
 */

/* Assign variables */
var measures = Selected.Measures;
var columns = Selected.Columns;

/* Check for at least one measure and only one column selected */
if (measures.Count == 0)
{
    Error("Select one or more measures");
    return;
};

if (columns.Count != 1)
{
    Error("Select only one column along with any measures");
    return;
};

/* Cycle over all Selected measures in model */
foreach (var c in columns)
    {
    foreach (var m in measures)
        {
            /* Wrap the current measure in a HASONEVALUE(column) check */
            m.Expression = "IF ( HASONEVALUE( " + c.DaxObjectFullName + " ), " + m.Expression + " )";
        };
    }   
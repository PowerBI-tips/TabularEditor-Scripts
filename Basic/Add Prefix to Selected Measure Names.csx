/*
 * Title: Add Prefix to Selected Measures
 *
 * Author: Mike Carlo, https://PowerBI.tips
 *
 * This script loops through all the selected measures and adds a Prefix String to the name.
 * Only Selected measures will be modified.
 *
 */

/* Cycle over all Selected measures in model */
foreach (var m in Selected.Measures)
    {
        /* Grab the current name as a variable, prepend some text */
        m.Name = "Test " + m.Name;
    }
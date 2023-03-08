/*
 * Title: Update/Replace the DAX Expression in a Measure's Description
 *
 * Author: Dan Meissner
 *
 * This script, when executed, will loop through all the measures in the model and
 * Either (1) add the DAX expression into the field's description for documentation purposes if it is missing
 * or (2) remove the existing description DAX expression and updated it with the current DAX expression.
 *
 * This script assumes the DAX expression is the last text in the description and is preceded with a single line of text "Expression:"
 */

foreach (var m in Selected.Measures)
{
    int index = m.Description.IndexOf("Expression:");
    if (index >= 0)
    {
        m.Description = m.Description.Substring(0, index+11) + System.Environment.NewLine + m.Expression;
    }
    else
    {
        m.Description = m.Description + System.Environment.NewLine + System.Environment.NewLine + "Expression:" + System.Environment.NewLine + m.Expression;
    };
}; 
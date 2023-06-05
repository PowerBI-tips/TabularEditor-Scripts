#r "Microsoft.VisualBasic"

/*
 * Title: Add prefix to selected measures
 *
 * Author: Adam Grisdale
 *
 * This script prompts the user for a string, then loops through all the selected measures and adds that string as a prefix to the measures name.
 * The code adds a space between the new prefix and the current measures name. Modify the code below if this does not suit your needs.
 * If no string is provided, or the user clicks cancel, nothing will happen.
 *
 */

// Ask the user to provide a prefix to add to the measure name.
string NewPrefix = Microsoft.VisualBasic.Interaction.InputBox("Enter the text you wish to add to the beginning of the selected measure names.\n\nA space will automatically be added between the text entered below and the existing measure name.", "Add prefix to selected measures", "");

// Check if a string has been entered
if (!string.IsNullOrEmpty(NewPrefix))
{
    // Loop through selected measures
    foreach (var m in Selected.Measures)
    {
        // Grab the current name as a variable and append the prefix entered
        m.Name = NewPrefix + " " + m.Name;
    }
}
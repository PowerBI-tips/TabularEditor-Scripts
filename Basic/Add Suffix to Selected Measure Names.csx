#r "Microsoft.VisualBasic"

/*
 * Title: Add suffix to selected measures
 *
 * Author: Adam Grisdale
 *
 * This script prompts the user for a string, then loops through all the selected measures and adds that string as a suffix to the measures name.
 * The code adds a space between the current measures name and the new suffix. Modify the code below if this does not suit your needs.
 * If no string is provided, or the user clicks cancel, nothing will happen.
 *
 */

// Ask the user to provide a suffix to add to the measure name.
string NewSuffix = Microsoft.VisualBasic.Interaction.InputBox("Enter the text you wish to add to the end of the selected measure names.\n\nA space will automatically be added between the existing measure name and the text entered below.", "Add suffix to selected measures", "");

// Check if a string has been entered
if (!string.IsNullOrEmpty(NewSuffix))
{
    // Loop through selected measures
    foreach (var m in Selected.Measures)
    {
        // Grab the current name as a variable and append the suffix entered
        m.Name = m.Name + " " + NewSuffix;
    }
}
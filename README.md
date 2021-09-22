# Tabular Editor Scripts
Community repository for sharing and discussing scripts for use with Tabular Editor. Originally forked from [TabularEditor Scripts](https://github.com/TabularEditor/Scripts).

## Using scripts from this repository
If you'd like to use a script from this repository, simply copy the script content into the Advanced Scripting pane of Tabular Editor. For more details about scripting, [read this article](https://github.com/otykier/TabularEditor/wiki/Advanced-Scripting). You may also [store scripts as Custom Actions](https://github.com/otykier/TabularEditor/wiki/Custom-Actions) that are integrated in Tabular Editor's UI.

**DISCLAIMER:** THE SCRIPTS IN THIS REPOSITORY ARE PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND. You are responsible for ensuring that any scripts you execute does not contain malicious code, or does not cause unwanted changes to your tabular models. If you do not understand what the code does, do not blindly execute it! A script has access to the full power of the .NET platform, meaning it can theoretically alter system files, download stuff from the internet, etc. Do not execute a script from sources you do not trust.

## How to contribute
Fork the repo, add your scripts and submit a pull request - it's that simple!

Scripts should use the `.csx` file extension. If you plan to submit a collection of multiple scripts, feel free to put them into a subfolder and provide a `README.md` file with additional documentation.

Please ensure that your script is thoroughly documented with a comment section at the top of the file. Feel free to use the following snippet as a template:

```csharp
/*
 * Title: Auto-generate SUM measures from columns
 * 
 * Author: Daniel Otykier, twitter.com/DOtykier
 * 
 * This script, when executed, will loop through the currently selected columns,
 * creating one SUM measure for each column and also hiding the column itself.
 */
 
// Loop through all currently selected columns:
foreach(var c in Selected.Columns)
{
    var newMeasure = c.Table.AddMeasure(
        "Sum of " + c.Name,                    // Name
        "SUM(" + c.DaxObjectFullName + ")",    // DAX expression
        c.DisplayFolder                        // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "0.00";

    // Provide some documentation:
    newMeasure.Description = "This measure is the sum of column " + c.DaxObjectFullName;

    // Hide the base column:
    c.IsHidden = true;
}
```

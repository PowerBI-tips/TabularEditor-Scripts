/*
 * Title: Build direct or indirect dependency tree of selected or all measures
 * Author: Daniel Otykier, twitter.com/DOtykier
 * Co-Author: nexus150, https://github.com/nexus150
 * 
 * This script, when executed, will loop through the currently selected measures (or all measures),
 * exporting all measures that depends on it, both direct or indirect way.
 *
 * This script was created in response to:
 * https://github.com/TabularEditor/TabularEditor/issues/897
 *
 */

string tsv = "Measure\tDependsOnMeasure"; // TSV file header row

// Loop through all measures: (Change to Model.AllMeasures to Selected.Measures if only want Loop through selected measures)
foreach(var m in Model.AllMeasures /**/) {

    // Get a list of all measures referenced directly by the current measure:
    var allReferences = m.DependsOn.Measures;

    // Get a list of ALL measures referenced by this measure (both directly and indirectly through other measures):
    //var allReferences = m.DependsOn.Deep().OfType<Measure>().Distinct();

    // Output TSV rows - one for each measure reference:
    foreach(var m2 in allReferences)
        tsv += string.Format("\r\n{0}\t{1}", m.Name, m2.Name);
}

tsv.Output();   
// SaveFile("c:\\MyProjects\\SSAS\\MeasureDependencies.tsv", tsv); // Uncomment this line to save output to a file
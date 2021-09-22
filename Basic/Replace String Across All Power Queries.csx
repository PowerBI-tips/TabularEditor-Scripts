// Title: ReplaceStringAcrossAllPowerQueries.csx
// Author: @JamesDBartlett3
// Description: Replaces a string in PowerQuery on all partitions in the model

var oldPQString = "";
var newPQString = "";

// Loop through all partitions on the model, replacing oldPQString with newPQString
foreach(var p in Model.AllPartitions.OfType<MPartition>())
{
    p.Expression = p.Expression
                      .Replace(oldPQString, newPQString);
}
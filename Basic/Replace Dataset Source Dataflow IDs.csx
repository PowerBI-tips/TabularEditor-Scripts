// Title: ReplaceDatasetSourceDataflowIDs.csx
// Author: @JamesDBartlett3
// Description: Replace the source DataflowID & WorkspaceID on all partitions in the model

var oldWorkspaceId = "";
var newWorkspaceId = "";
var oldDataflowId = "";
var newDataflowId = "";

// Loop through all partitions on the model, replacing the DataflowIDs & WorkspaceIDs
foreach(var p in Model.AllPartitions.OfType<MPartition>())
{
    p.Expression = p.Expression
                      .Replace(oldWorkspaceId, newWorkspaceId)
                      .Replace(oldDataflowId, newDataflowId);
}
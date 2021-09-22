// Title: ExcludeSelectedTablesFromModelRefresh.csx
// Author: @JamesDBartlett3
// Description: Exclude selected tables from model refresh

foreach(var table in Selected.Tables) {
    table.ExcludeFromModelRefresh = true;
}
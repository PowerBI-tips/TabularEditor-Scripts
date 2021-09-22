#r "System.Text.RegularExpressions"

/*
 * Title: Clean Object Names
 * 
 * Author: Darren Gosbell, twitter.com/DarrenGosbell
 * 
 * This script, when executed, will loop through your model and update the
 * names of any tables and columns with CamelCaseNames and insert spaces before upper
 * case characters so you end up with
 * 
 * before: CalendarYearNum
 * after:  Calendar Year Num
 *
 * This script ignores any columns that already have spaces in the names 
 * and any hidden columns. It also skips adjacent upper case characters
 * so "MyTXTColumn"  becomes "My TXT Column"
 */

// this regular expression splits strings on underscores and changes from lower to upper case
// so "my_column_name" becomes an array like {"my", "_", "column", "_", "name"}
// and "MyOtherColumnName" becomes an array like {"My", "Other", "Column", "Name"}
var rex = new System.Text.RegularExpressions.Regex( "(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+|[^A-Z,a-z]+|[_]|[a-z]+)");

// if any of the following are the first word of a table name they will be stripped out
List<string> tablePrefixesToIgnore = new List<string>() {"dim","fact", "vw","tbl","vd","td","tf","vf"};

// if any of the following are the last word of a table name they will be stripped out
List<string> tableSuffixesToIgnore = new List<string>() {"dim", "fact"};

foreach (var tbl in Model.Tables) 
{
    if (!tbl.IsHidden && !tbl.Name.Contains(" ")) 
    {
        string name = tbl.Name;
        var matches = rex.Matches(name);
        var firstWord = matches[0];
        var lastWord = matches[matches.Count-1];
        string[] words = matches
                        .OfType<System.Text.RegularExpressions.Match>()
                        .Where(m =>
                                // skip words that are just underscores so that they are replaced with spaces
                                m.Value != "_" 
                                // skip the first word if it matches one of the prefixes to ignore
                                && !(m == firstWord && tablePrefixesToIgnore.Contains(m.Value,System.StringComparer.OrdinalIgnoreCase)) 
                                // skip the last word if it matches one of the suffixes to ignore
                                && !(m == lastWord && tableSuffixesToIgnore.Contains(m.Value,System.StringComparer.OrdinalIgnoreCase )) 
                                )
                        .Select(m => char.ToUpper(m.Value.First()) + m.Value.Substring(1))
                        .ToArray();                
        string result = string.Join(" ", words);
        tbl.Name = result;
    }

    foreach (var col in tbl.Columns)
    {
        if (!col.IsHidden && !col.Name.Contains(" ")) 
        {
            string name = col.Name;
            string[] words = rex.Matches(name)
                            .OfType<System.Text.RegularExpressions.Match>()
                            // skip underscores 
                            .Where(m => m.Value != "_" )
                            .Select(m => char.ToUpper(m.Value.First()) + m.Value.Substring(1))
                            .ToArray();                
            string result = string.Join(" ", words);
            col.Name = result;
        }
    }
}
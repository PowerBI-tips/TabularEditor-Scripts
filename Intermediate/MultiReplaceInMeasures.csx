/*
 * Title: Multi-string Search and Replace in any selected measure
 * 
 * Author: Dan Meissner
 * 
 * This script will loop through a matched pair of strings held in a 2-D array 
 * and search and replace a FromString and ToString pair in the currently selected measures.
 *
 */

// Replace Text Strings Pairs that appear in selected measures

    // Update the value of the text arrays for your desired usecase. Make sure to list structure the list as:
    // var ReplacementPair  = new string[,] {{"FromString1","ToString1"},{"FromString2","ToString2"},{"FromString3","ToString3"}};
	
	// Add as many From and To pairs to the array as needed. 
	// (technically C# has a 2GB memory size and 4 Billion array item limit, but... really...)

    // If the string you are either searching for or replacing with contains a double quote " then you need to 'escape it' by 
    // proceeding it with a backslash \ (as in \") to have that quote character wihtin the respective text string

	var ReplacementPair  = new string[,] { {"FromString1","ToString1"},
                                           {"FromString2","ToString2"},
                                           {"FromString3","ToString3"} };

	foreach (var m in Selected.Measures)
    	{
           for (int i=0; i < ReplacementPair.GetLength(0);)
            {
                m.Expression = m.Expression.Replace(ReplacementPair[i,0],ReplacementPair[i,1]);
                i++;
            }
    	}
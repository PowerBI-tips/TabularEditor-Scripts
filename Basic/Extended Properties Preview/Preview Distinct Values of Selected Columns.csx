// Author: Tommy Puglia
// Website: https://pugliabi.com
// Check out the Explicit Measures Podcast on Apple, spotify, & YouTube! 
// https://www.youtube.com/playlist?list=PLn1m_aBmgsbHr83c1P6uqaWF5PLdFzOjj
//
//
// An incredibly simple script to easily show the distinct values based on selected columns. This is a great way to peak quickly over multiple columns when you are dealing with a new data model.
//
//
// Pre-Req:
// Save this Macro with the following:
// Name: Preview\Distinct Values
// Marco Context: Column
// (Hint, you can organize your macros in TE3 by grouping based on a "\" separator!")
//
// Instructions:
// 1. Select the columns you want to see the distinct values for
// 2. Run the script (Right Click, on the selected columns, and choose the Preview --> Distinct Values macro)
// 3. For every column chosen, a pop-up will appear


foreach(var c in Selected.Columns)
{
string cName = c.Name;
var cDAX = c.DaxObjectFullName;
string ev = "Evaluate \n";
string newli = "\n";
string DistinctN = "Distinct(";
var cResult = EvaluateDax(DistinctN + cDAX + ")");
cResult.Output();
}





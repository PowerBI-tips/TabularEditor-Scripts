// Author: Tommy Puglia
// Website: https://pugliabi.com
// Check out the Explicit Measures Podcast on Apple, spotify, & YouTube! 
// https://www.youtube.com/playlist?list=PLn1m_aBmgsbHr83c1P6uqaWF5PLdFzOjj
//
//
// A simple test that may change how we develop and share scripts and easily create tests, new measures, and sharing content.

// An all too column problem with sharing macros is usually we need to configure some of the variables (measure name, date table name, etc). This becomes especially tedious as this has to occur per data model, rather than for a given script. This greatly reduces the ability of utilizing macros in a more productive way.

// Enter Extended Properties. Tied to the model is the ability to create a key and value pair, which can is defined in the model properties. Further, we can use Tabular Editor 3's scripts to actually return the value of these properties. 

// This can solve and open up solutions such as:

// - Creating a macro that when multiple columns are chosen, will default to always summarize / group by a given measure
// - define the Date Table / Date Column in a model for Calculated Groups or time intelligence
// - define the date column in the fact table once for time intelligence
// - set other variables that are used in multiple macros

// This is simply a test to see what is returned. It will group a column and return a Summarize based on the "Global Measure" From the Extended Properties. 

// Instructions:
// 1. Open the model in Tabular Editor 3
// 2. Go to the properties of the model, and choose the Extended Properties field (under Annotations)
// 3. Add a new property with the the DAX Object Name of a measure in the model (such as [Total Sales])
// 4. Choose the Column in the object explorer, and run the script!



var s = Model.GetExtendedProperties();
var tt = Model.GetExtendedProperty(0);
string ev = "Evaluate \n";
string newli = "\n";
string Summa = "SUMMARIZE(";
string TableFromColumn = Selected.Column.Table.Name;
string newlinecomma = ", \n \n";
string ColumnChosen = Selected.Column.DaxObjectFullName;
string MeasureNameSummarize = ", \n \n \"YourMeasure!\" , ";
string MeasureFromProperty = tt;
string closeit = ")";
string orby = "Order By " + tt + " DESC";
string stringallTogether = newli + Summa + TableFromColumn + newlinecomma + ColumnChosen + MeasureNameSummarize 
+ MeasureFromProperty;
var result = EvaluateDax(stringallTogether + ")" + " \n" + orby);
result.Output();


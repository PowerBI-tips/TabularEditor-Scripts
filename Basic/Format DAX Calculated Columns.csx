/*
 * Title: Formats all the calculated columns
 * 
 * Author: Ricardo Rincón, twitter.com/nexus150 , www.bitodata.com
 * 
 * This script, when executed, formats all the calculated columns of the model
 * using the SQLBI.COM service https://www.daxformatter.com/.
 * please use the version with Selected.Columns as long as you can
 * to minimize the load on the service so that we can all enjoy it.
 */

// Format (Only Selected) Calculated Columns (if you don't need to format ALL calculated columns, please use this version)
foreach(var m in Selected.Columns)
{
    if(m.Type.ToString() == "Calculated"){   
      var y = m  as CalculatedColumn;   
      y.FormatDax();
    }
}   

// Format All Calculated Columns
//foreach(var m in Model.AllColumns)
//{
//    if(m.Type.ToString() == "Calculated"){   
//      var y = m  as CalculatedColumn;   
//      y.FormatDax();
//    }
//}   
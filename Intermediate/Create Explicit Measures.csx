// Title: Auto-create explicit measures from all columns in all tables that have qualifying aggregation functions assigned 
//  
// Author: Tom Martens, twitter.com/tommartens68
// 
// This script, when executed, will loop through all the tables and creates explicit measure for all the columns with qualifying
// aggregation functions.
// The qualifying aggregation functions are SUM, COUNT, MIN, MAX, AVERAGE.
// This script can create a lot of measures, as by default the aggregation function for columns with a numeric data type is SUM.
// So, it is a good idea to check all columns for the proper aggregation type, e.g. the aggregation type of id columns 
// should be set to None, as it does not make any sense to aggregate id columns.
// An annotation:CreatedThrough is created with a value:CreateExplicitMeasures this will help to identify the measures created
// using this script.
// What is missing, the list below shows what might be coming in subsequent iterations of the script:
// - the base column property hidden is not set to true
// - no black list is used to prevent the creation of unwanted measures

// ***************************************************************************************************************
//the following variables are allowing controling the script
var overwriteExistingMeasures = 0; // 1 overwrites existing measures, 0 preserves existing measures

var measureNameTemplate = "{0} ({1}) - {2}"; // String.Format is used to create the measure name. 
//{0} will be replaced with the columnname (c.Name), {1} will be replaced with the aggregation function, and last but not least
//{2} will be replaced with the tablename (t.Name). Using t.Name is necessary to create a distinction between measure names if
//columns with the same name exist in different tables.
//Assuming the column name inside the table "Fact Sale" is "Sales revenue" and the aggregation function is SUM 
//the measure name will be: "Sales revenue (Sum) - Fact Sale"

//store aggregation function that qualify for measure creation to the hashset aggFunctions
var aggFunctions = new HashSet<AggregateFunction>{
    AggregateFunction.Default, //remove this line, if you do not want to mess up your measures list by automatically created measures for all the columns that have the Default AggregateFunction assigned
    AggregateFunction.Sum,
    AggregateFunction.Count,
    AggregateFunction.Min,
    AggregateFunction.Max,
    AggregateFunction.Average
};

//You have to be aware that by default this script will just create measures using the aggregate functions "Sum" or "Count" if
//the column has the aggregate function AggregateFunction.Default assigned, this is checked further down below.
//Also, if a column has the Default AggregateFunction assigned and is of the DataType
//DataType.Automatic, DataType.Unknown, or DataType.Variant, no measure is created automatically, this is checked further down below.
//dictDataTypeAggregateFunction = new Dictionary<DataType, string>();
//see this article for all the available data types: https://docs.microsoft.com/en-us/dotnet/api/microsoft.analysisservices.tabular.datatype?view=analysisservices-dotnet
//Of course you can change the aggregation function that will be used for different data types,
//as long as you are using "Sum" and "Count"
//Please be careful, if you change the aggregation function you might end up with multiplemeasures
var dictDataTypeAggregateFunction = new Dictionary<DataType, AggregateFunction>();
dictDataTypeAggregateFunction.Add( DataType.Binary , AggregateFunction.Count ); //adding a key/value pair(s) to the dictionary using the Add() method
dictDataTypeAggregateFunction.Add( DataType.Boolean , AggregateFunction.Count );
dictDataTypeAggregateFunction.Add( DataType.DateTime , AggregateFunction.Count );
dictDataTypeAggregateFunction.Add( DataType.Decimal , AggregateFunction.Sum );
dictDataTypeAggregateFunction.Add( DataType.Double , AggregateFunction.Sum );
dictDataTypeAggregateFunction.Add( DataType.Int64 , AggregateFunction.Sum );
dictDataTypeAggregateFunction.Add( DataType.String , AggregateFunction.Count );

// ***************************************************************************************************************
//all the stuff below this line should not be altered 
//of course this is not valid if you have to fix my errors, make the code more efficient, 
//or you have a thorough understanding of what you are doing

//store all the existing measures to the list listOfMeasures
var listOfMeasures = new List<string>();
foreach( var m in Model.AllMeasures ) {
    listOfMeasures.Add( m.Name );
}

//loop across all tables
foreach( var t in Model.Tables ) {
    
    //loop across all columns of the current table t
    foreach( var c in t.Columns ) {
        
        var currAggFunction = c.SummarizeBy; //cache the aggregation function of the current column c
        var useAggFunction = AggregateFunction.Sum;
        var theMeasureName = ""; // Name of the new Measure
        var posInListOfMeasures = 0; //check if the new measure already exists <> -1
        
        if( aggFunctions.Contains(currAggFunction) ) //check if the current aggregation function qualifies for measure aggregation
        {
            //check if the current aggregation function is Default
            if( currAggFunction == AggregateFunction.Default )
            {
                // check if the datatype of the column is considered for measure creation
                if( dictDataTypeAggregateFunction.ContainsKey( c.DataType ) )
                {
                    
                    //some kind of sanity check
                    if( c.DataType == DataType.Automatic || c.DataType == DataType.Unknown || c.DataType == DataType.Variant )
                    {
                        Output("No measure will be created for columns with the data type: " + c.DataType.ToString() + " (" + c.DaxObjectFullName + ")");
                        continue; //moves to the next item in the foreach loop, the next colum in the current table
                    }
                  
                    //cache the aggregation function from the dictDataTypeAggregateFunction
                    useAggFunction = dictDataTypeAggregateFunction[ c.DataType ];
                    
                    //some kind of sanity check
                    if( useAggFunction != AggregateFunction.Count && useAggFunction != AggregateFunction.Sum ) 
                    {    
                        Output("No measure will be created for the column: " + c.DaxObjectFullName);
                        continue; //moves to the next item in the foreach loop, the next colum in the current table
                    }
                    theMeasureName = String.Format( measureNameTemplate , c.Name , useAggFunction.ToString() , t.Name ); // Name of the new Measure
                    posInListOfMeasures = listOfMeasures.IndexOf( theMeasureName ); //check if the new measure already exists <> -1
                    
                } else {
                   
                    continue; //moves to the next item in the foreach loop, the next colum in the current table
                }
                        
            } else {
                
                useAggFunction = currAggFunction;    
                theMeasureName = String.Format( measureNameTemplate , c.Name , useAggFunction.ToString() , t.Name ); // Name of the new Measure
                posInListOfMeasures = listOfMeasures.IndexOf( theMeasureName ); //check if the new measure already exists <> -1
                
            }
            
            //sanity check
            if(theMeasureName == "")
            {
                continue; //moves to the next item in the foreach loop, the next colum in the current table
            }
            
            // create the measure
            if( ( posInListOfMeasures == -1 || overwriteExistingMeasures == 1 )) 
            {    
                if( overwriteExistingMeasures == 1 ) 
                {
                    foreach( var m in Model.AllMeasures.Where( m => m.Name == theMeasureName ).ToList() ) 
                    {
                        m.Delete();
                    }
                }
                
                var newMeasure = t.AddMeasure
                (
                    theMeasureName                                                                      // Name of the new Measure
                    , "" + useAggFunction.ToString().ToUpper() + "(" + c.DaxObjectFullName + ")"        // DAX expression
                );
                
                newMeasure.SetAnnotation( "CreatedThrough" , "CreateExplicitMeasures" ); // flag the measures created through this script
                
            }
        }    
    }        
}
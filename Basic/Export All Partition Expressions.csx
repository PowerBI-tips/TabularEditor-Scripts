/*
 * Title: Export All Partition Expressions
 *
 * Author: Dan Meissner
 * Last Modified: 2023JAN09
 *
 * This script exports the partition expressions for all table partitions to the screen or a tab separated file of your choice.  
 * The output has one row per table partition.
 *
 * Change the filename and filepath as necessary below.
 *
 * Thanks to Kurt Buhler (Data Goblins) for most of the work and the inspiration.
 */

/* Assign variables */
var filename = "TableExpressionOutput.tsv";
var filepath = @"C:\Sandbox\";
var tsv = " ";

// Export Properties
tsv = tsv + ExportProperties(Model.AllPartitions, "Expression");

// Output the results to a dialog box pop up on the screen (can copy to clipboard from the dialog box)
tsv.Output();

// Output the results to a tab separated file at filepath/filename
//SaveFile( filepath + filename, tsv);
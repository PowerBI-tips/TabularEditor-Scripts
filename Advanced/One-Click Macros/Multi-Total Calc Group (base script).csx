// '2023-07-09 / B.Agullo / 
//
// Multi-Total Calc Group: Base script
//
// The development of this script is shown here https://www.esbrina-ba.com/industrializing-calculation-groups/
// store as macro and select only column as target object
// to use the right click on a single column you want to use to slice your data from the columns section of your matrix.
// to add custom totals to the matrix using this calc group as top level column field, you need to run 
// the script shared in the file "Multi-Total Calc Group (add total script).csx" in this same repository folder. 
//
// Follow Bernat on LinkedIn and Twitter
// https://www.linkedin.com/in/bernatagullo/
// https://twitter.com/AgulloBernat

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;

if (Selected.Columns.Count() != 1)
{
    Error("Select only 1 column and try again");
    return;
}
Column column = Selected.Column;
string suggestedCalcGroupName = column.Name + " Multi-Totals";
string calcGroupName = Interaction.InputBox(
    Prompt:"Please provide the name of the multi-total calc group.",
    DefaultResponse:suggestedCalcGroupName);
if (calcGroupName == "")
{
    Error("No name provided");
    return;
};
CalculationGroupTable calcGroup = 
    Model.AddCalculationGroup(
        calcGroupName);
string valuesCalcItemName = "Values";
string valuesCalcItemExpression =
    String.Format(
        @"IF(
            ISINSCOPE( {0} ),
            SELECTEDMEASURE()
        )", column.DaxObjectFullName);
CalculationItem valuesCalcItem =
    calcGroup.AddCalculationItem(
        name: valuesCalcItemName,
        expression: valuesCalcItemExpression);
valuesCalcItem.FormatDax();
valuesCalcItem.Description = "This calculation item is to show the breakdown by " + column.Name;
valuesCalcItem.Ordinal = 0;
string totalCalcItemName = "Total";
string totalCalcItemExpression =
    String.Format(
        @"IF(
            NOT ISINSCOPE( {0} ),
            SELECTEDMEASURE()
        )", column.DaxObjectFullName);
CalculationItem totalCalcItem =
    calcGroup.AddCalculationItem(
        name: totalCalcItemName,
        expression: totalCalcItemExpression);
totalCalcItem.FormatDax();
totalCalcItem.Description = "This calculation item is to show the regular total as a calculation item along with different totals that will be added to this calculation group";
totalCalcItem.Ordinal = 1; 
string calcGroupTypeLabel = "CalcGroupType";
string calcGroupTypeValue = "MultiTotal";
calcGroup.SetAnnotation(
    calcGroupTypeLabel,
    calcGroupTypeValue);
string calcGroupValuesFieldLabel = "ValuesField";
string calcGroupValuesFieldValue = column.DaxObjectFullName;
calcGroup.SetAnnotation(
    calcGroupValuesFieldLabel,
    calcGroupValuesFieldValue);

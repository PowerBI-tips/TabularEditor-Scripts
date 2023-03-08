#r "Microsoft.AnalysisServices.Core.dll"
#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;


/*
 This script was inspired by the article from SQLBI https://sql.bi/695263
 
 Code example for checking connection to Power BI Desktop was provided by Daniel Otykier 
   For more detail see the this link: https://github.com/TabularEditor/TabularEditor/issues/1010

    Requirements: Create What-If Parameter before running this code!

    Remove this comment to avoid bugs
 */



var ListOfTableAnnotations = new List<string>();
string RexExpPattern = "TopNScript_(.+?)_ShiyanovG";
foreach (Table t in Model.Tables)
{
    foreach (var a in t.GetAnnotations())
    {
        // a.Output();
        bool IsMatched =
        a.IndexOf("TopNScript_", StringComparison.OrdinalIgnoreCase) >= 0
        && a.IndexOf("_ShiyanovG", StringComparison.OrdinalIgnoreCase) >= 0
        ;
        if (IsMatched)
        {
            ListOfTableAnnotations.Add(a);
        }
    }
}

bool HasScriptAnnotations = ListOfTableAnnotations.Count() > 0;

if (HasScriptAnnotations == true)
{
    Info("TopN Script already implemented");
    return;
}


var server = Model.Database.TOMDatabase.Server as Microsoft.AnalysisServices.Tabular.Server;
var isLoadedFromFile = server == null;
var isPbiDesktop = server != null 
    && server.ServerLocation == Microsoft.AnalysisServices.ServerLocation.OnPremise
    && server.CompatibilityMode == Microsoft.AnalysisServices.CompatibilityMode.PowerBI;

if (isLoadedFromFile)
{
    Info("Metadata loaded from file");
}
else if (isPbiDesktop)
{
    // Info("Connected to PBI Desktop");
    //Info("Select TopN What-If Parameter");



    string ConfigInpuBoxText =
        "1 - Measures"
        + "\n"
        + "2 - Calculation Group"
        ;
    int ConfigDefaultInput = 1;
    var ConfigAnswer = Interaction.InputBox(
            ConfigInpuBoxText,
            "Choose configuration of execution",
            ConfigDefaultInput.ToString(),
            740,
            400);

    bool RunCode = 
                ConfigAnswer == '1'.ToString()
                || ConfigAnswer == '2'.ToString();
    string ConfigErrorMessage =  @"Please choose correct configuration!";
    if (RunCode == false)
    {
        // Force an Error if configuration is wrong
        Error(ConfigErrorMessage);
        // Do nothing
        return;
    }

    Table StartTableTopN = Model.SelectTable(null, "Select TopN What-If Parameter");
    bool CheckForTopN = StartTableTopN == null;
    if (CheckForTopN)
    {
        Error("No TopN What-If");
        return;
    }
    else
    {
        Info("OK");

        var ListOfFactTables = new List<Table>();
        var ListOfDimentionTables = new List<Table>();

        foreach (var r in Model.Relationships)
        {
            Table ManyColTable = r.FromColumn.Table;
            // Table ManyColTable = r.FromColumn.Table;
            bool notInListFact = ListOfFactTables.Contains(ManyColTable);
            if (notInListFact == false)
            {
                ListOfFactTables.Add(ManyColTable);
            }
        }

        foreach (var r in Model.Relationships)
        {
            Table OneColTable = r.ToColumn.Table;
            bool NotDate = r.ToColumn.DataType == DataType.DateTime;
            bool notInListDim = ListOfDimentionTables.Contains(OneColTable);
            if (notInListDim == false && NotDate == false)
            {
                ListOfDimentionTables.Add(OneColTable);
            }
        }

        // Set Annotation for object StartTable
        StartTableTopN.SetAnnotation(
            "TopNScript_StartTableTopN_ShiyanovG"
            , "TopNScript_StartTableTopN_ShiyanovG"
            );
        Measure MeasureTopN = StartTableTopN.SelectMeasure(null, "Select TopN measure");
        var MeasureTopNReference = MeasureTopN.DaxObjectName;
        Table StartTable = ListOfDimentionTables.SelectTable(null, "Select the table to implement TopN + Others for");
        string StartTableName = StartTable.DaxObjectFullName;
        // Set Annotation for object StartTable
        StartTable.SetAnnotation(
            "TopNScript_StartTable_ShiyanovG"
            , "TopNScript_StartTable_ShiyanovG"
            );

        Column StartColumn =
        StartTable.Columns
        .Where(c => c.UsedInRelationships.Any() == false)
        .SelectColumn(null, "Select the column to use for TopN + Others");


        Table FactTable = ListOfFactTables.SelectTable(null, "Where is your main Fact Table");
        Measure RankingMeasureReference = FactTable.SelectMeasure(null, "What is you base measure for the pattern");
        string RankingMeasureReferenceName = RankingMeasureReference.DaxObjectFullName;

        string TopNTableExpression =
        "UNION( "
        + "ALLNOBLANKROW( " + StartColumn.DaxObjectFullName + "),"
        + "\n"
        + "{ \"Others\" }"
        + "\n )";

        string TopNTableName = StartTable.Name + " Names";
        string FullPowerBITableExpression =
        TopNTableName + " = "
        + "\n"
        + TopNTableExpression;

        Info("Copy to Clipboard the following code and create new Calucated Table");
        FullPowerBITableExpression.Output();

        Table ReferenceTable = Model.Tables[TopNTableName];
        ReferenceTable.SetAnnotation(
            "TopNScript_ReferenceTable_ShiyanovG"
            , "TopNScript_ReferenceTable_ShiyanovG"
            );

        string ReferenceTableName = ReferenceTable.DaxObjectFullName;
        string ReferenceColumnName = ReferenceTable.Columns.First().DaxObjectFullName;
        string RankingMeasureName = "Ranking";
        string Others = " \"Others\" ";
        string RankingMeasureDax =
          @"
IF (
    ISINSCOPE ( {1} ),
    VAR ProductsToRank = [TopN Value] 
    VAR SalesAmount = {3} 
    VAR IsOtherSelected =
        SELECTEDVALUE ( {1} ) = {0}  
    RETURN
        IF(
            IsOtherSelected,
            -- Rank for Others
            ProductsToRank + 1,
            -- Rank for regular products
            IF (
                SalesAmount > 0,
                VAR VisibleProducts =
                    CALCULATETABLE(VALUES({1}), ALLSELECTED({2}))
                VAR Ranking =
                    RANKX(VisibleProducts, {3}, SalesAmount)
                RETURN
                    IF (Ranking > 0 && Ranking <= ProductsToRank, Ranking )
            )
        ) 
 ) 
 ";
        string RankingMeasureDaxFormatted = string.Format(
         RankingMeasureDax, Others, ReferenceColumnName,
         ReferenceTableName, RankingMeasureReferenceName
         );
        string VisibleRowMeasureDax =
           @"
VAR Ranking = [Ranking] 
VAR TopNValue = [TopN Value]
VAR Result =  
    IF( 
        NOT ISBLANK(Ranking), 
        (Ranking <= TopNValue) - (Ranking = TopNValue + 1) 
    ) 
RETURN  Result ";

       // Generate code for desired measure (for example Sales Amount)
        string AddColsColumn = "@SalesAmount";
        string q = "\"";
        string AmountNAMeasureDax = @"
 VAR SalesOfAll =
 CALCULATE ( {6}, REMOVEFILTERS ( {5} ) )
RETURN
    IF (
        NOT ISINSCOPE ( {3} ),
        -- Calculation for a group of products 
        SalesOfAll,
        -- Calculation for one product name
        VAR ProductsToRank = [TopN Value]
        VAR SalesOfCurrentProduct = {6}
        VAR IsOtherSelected =
            SELECTEDVALUE ( {3} ) = {0}
        RETURN
            IF(
                NOT IsOtherSelected,
                -- Calculation for a regular product
                SalesOfCurrentProduct,
                -- Calculation for Others
                VAR VisibleProducts =
                    CALCULATETABLE(
                        VALUES({4}),
                        REMOVEFILTERS({3})
                    )
                VAR ProductsWithSales =
                    ADDCOLUMNS(VisibleProducts, {1}{2}{1} , [Sales Amount])
                VAR FilterTopProducts =
                TOPN(ProductsToRank, ProductsWithSales, [{2}])
                VAR FilterOthers =
                    EXCEPT(ProductsWithSales, FilterTopProducts)
                VAR SalesOthers =
                    CALCULATE(
                        {6},
                        FilterOthers,
                        REMOVEFILTERS ( {3} )
                    )
                RETURN
                    SalesOthers
            )
            )"
                    ;
        string AmountNAMeasureDaxFormatted =
            string.Format(
                AmountNAMeasureDax
                , Others  // 0
                , q // 1
                , AddColsColumn // 2
                , ReferenceColumnName // 3
                , StartTableName // 4
                , ReferenceTableName // 5
                , RankingMeasureReferenceName // 6
                );
        string AmountNAMeasureName = RankingMeasureReference.Name + " NA";

        if (ConfigAnswer == "1")
        {
            // Add Ranking measure to the table with Formatted code
            ReferenceTable.AddMeasure(RankingMeasureName, RankingMeasureDaxFormatted, null);
            // Add Visible Row measure to the table 
            ReferenceTable.AddMeasure("Visible Row", VisibleRowMeasureDax, null);
            // Add AmountNA measure to the table 
            ReferenceTable.AddMeasure(
                AmountNAMeasureName
                , AmountNAMeasureDaxFormatted
                , null
                );
            // Format all created measures
            ReferenceTable.Measures.FormatDax();
        }
        else if (ConfigAnswer=="2")
        {
            string Config2Text = @"
            Corrently code doesn't support Calc Group implementation.
            Press OK to create measures.";
            Info(Config2Text);

            // Add Ranking measure to the table with Formatted code
            ReferenceTable.AddMeasure(RankingMeasureName, RankingMeasureDaxFormatted, null);
            // Add Visible Row measure to the table 
            ReferenceTable.AddMeasure("Visible Row", VisibleRowMeasureDax, null);
            // Add AmountNA measure to the table 
            ReferenceTable.AddMeasure(
                AmountNAMeasureName
                , AmountNAMeasureDaxFormatted
                , null
                );
            // Format all created measures
            ReferenceTable.Measures.FormatDax();
        }
    }

}
else
{
    Info("Not connected to PBI Desktop");
}



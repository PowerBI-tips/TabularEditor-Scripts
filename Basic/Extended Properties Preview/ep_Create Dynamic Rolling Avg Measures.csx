//
// Author: Tommy Puglia
// Website: https://pugliabi.com
// Check out the Explicit Measures Podcast on Apple, spotify, & YouTube!
// https://www.youtube.com/playlist?list=PLn1m_aBmgsbHr83c1P6uqaWF5PLdFzOjj
// TITLE!!
// Create Dynamic Rolling Days Measures based on Extended Properties

// !!!! NOTE
// This script should only be run after you have set up your Global Extended Properties (see the Global Model Extended Properties Template Generator  script)
// !!! NOTE

// This script will create 3 measures per selected measure in your model. The 3 measures are:
// 1. Rolling Days current period 
//  This is based on your date config (extended properties) and what the default rolling days are (also extended properties)

// 2. Rolling Days previous period
//  This is based on your date config (extended properties) and what the default rolling days are (also extended properties)   \

// 3. Rolling Days period over period
//  This is based on your date config (extended properties) and what the default rolling days are (also extended properties)   


// Instructions
// pre-requisites: ensure your model extended properties are part of the model and saved back (see top)
// 1. Select measures in your model
// 2. Run the script
// 3. Done!


var dateExtended = Model.GetExtendedProperty("DateColumn");
var factDateColumn = Model.GetExtendedProperty("FactDateColumn");
string newline = Environment.NewLine;
var displayfolder = "__RollingDays";
var DefaultDays = Model.GetExtendedProperty("DefaultRollingDays");
var defaultDaysNumber = Int16.Parse(DefaultDays);

foreach (var m in Selected.Measures)
{
    // Rolling Days
    var RollingFirst = m.Table.AddMeasure(
        m.Name + " Rolling Days",
        "VAR DatesWOFirst =    CALCULATE(        LASTNONBLANK( "
            + dateExtended
            + " , "
            + m.DaxObjectName
            + " ), ALL( "
            + factDateColumn
            + "   ))"
            + newline
            + " VAR firstDay = ( DatesWOFirst -"
            + defaultDaysNumber
            + " )"
            + newline
            + "VAR FirstType =    DATESBETWEEN("
            + dateExtended
            + " , firstDay, DatesWOFirst )"
            + newline
            + "VAR LastPrev =    FIRSTDATE( FirstType ) - 1 "
            + newline
            + " VAR FirstPrev = LastPrev - DatesWOFirst "
            + newline
            + "RETURN "
            + newline
            + " CALCULATE( "
            + m.DaxObjectName
            + " , DATESBETWEEN( "
            + dateExtended
            + ", firstDay, DatesWOFirst - 1 )    ) ",
        displayfolder
    );


    // Previous Rolling
  var RollingPrevious =   m.Table.AddMeasure(
        m.Name + " Rolling Days Prev",
        "VAR DaysRolling = "
            + defaultDaysNumber
            + newline
            + "VAR DatesWOFirst =    CALCULATE(        LASTNONBLANK( "
            + dateExtended
            + " , "
            + m.DaxObjectName
            + " ), ALL( "
            + factDateColumn
            + "   ) )"
            + newline
            + " VAR firstDay = ( DatesWOFirst - DaysRolling )"
            + newline
            + "VAR FirstType =    DATESBETWEEN("
            + dateExtended
            + " , firstDay, DatesWOFirst )"
            + newline
            + "VAR LastPrev =    FIRSTDATE( FirstType ) - 1 "
            + newline
            + " VAR FirstPrev = LastPrev - DaysRolling "
            + newline
            + "RETURN "
            + newline
            + " CALCULATE( "
            + m.DaxObjectName
            + " , DATESBETWEEN( "
            + dateExtended
            + ", FirstPrev, LastPrev - 1 )    ) ",
        displayfolder
    );

    // Pop
  var RollingPercent =   m.Table
        .AddMeasure(
            m.Name + " PoP",
            "Divide(("
                + m.DaxObjectName
                + " - ["
                + m.Name
                + " Rolling Days Prev]), + ["
                + m.Name
                + " Rolling Days Prev],Blank())",
            displayfolder
        );
RollingFirst.FormatDax();
RollingPrevious.FormatDax();
RollingPercent.FormatDax();

}


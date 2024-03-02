#r "Microsoft.VisualBasic"
using System.Windows.Forms;

using Microsoft.VisualBasic;
string cgAnnotationLabel = "MadeWith";
string cgAnnotationValue = "NumberFormatCalcGroup";
if (Selected.Measures.Count == 0)
{
    Error("Select one or more measures and try again");
    return;
};
string measureList = string.Join(",", Selected.Measures.Select(x => x.DaxObjectFullName));
string measureListName = string.Join(",", Selected.Measures.Select(x => x.Name));
CalculationGroupTable cg = null as CalculationGroupTable;
if (Model.Tables.Any(t => t.GetAnnotation(cgAnnotationLabel) == cgAnnotationValue)) 
{ 
    cg = (CalculationGroupTable) Model.Tables.Where(t => t.GetAnnotation(cgAnnotationLabel) == cgAnnotationValue).First();
} 
else
{
    string calcGroupName = Fx.GetNameFromUser("Choose name for the number format Calculation Group", "Atention", "Number Format");
    if (calcGroupName == "") return; // in case user cancelled
    cg = Model.AddCalculationGroup(name: calcGroupName);
    cg.Columns[0].Name = cg.Name;
    cg.SetAnnotation(cgAnnotationLabel, cgAnnotationValue);
}
List<string> formatList = new List<string>();
formatList.Add("in milions");
formatList.Add("in thousands");
string selectedFormat = Fx.ChooseString(formatList);
if (selectedFormat == null) return;
string formatString = "";
switch (selectedFormat)
{
    case "in milions":
        // code block
        formatString = @"""#,##0,,.0""";
        break;
    case "in thousands":
        // code block
        formatString = @"""#,##0,.0""";
        break;
    default:
        // code block
        break;
}
string ciValueExpression = "SELECTEDMEASURE()";
string ciFormatStringExpression =
    string.Format(
        @"IF(
            ISSELECTEDMEASURE({0}),
            {1},
            SELECTEDMEASUREFORMATSTRING()
        )",
        measureList,
        formatString
    );
string ciName = string.Format("{1} ({0})", measureListName, selectedFormat);
CalculationItem ci = cg.AddCalculationItem(name:ciName ,expression:ciValueExpression);
ci.FormatStringExpression = ciFormatStringExpression;
ci.FormatDax();

public static class Fx
{
    public static Table CreateCalcTable(Model model, string tableName, string tableExpression)
    {
        if(!model.Tables.Any(t => t.Name == tableName))
        {
            return model.AddCalculatedTable(tableName, tableExpression);
        }
        else
        {
            return model.Tables.Where(t => t.Name == tableName).First();
        }
    }
    public static string GetNameFromUser(string Prompt, string Title, string DefaultResponse)
    {    
        string response = Interaction.InputBox(Prompt, Title, DefaultResponse, 740, 400);
        return response;
    }
    public static string ChooseString(IList<string> OptionList)
    {
        Func<IList<string>, string, string> SelectString = (IList<string> options, string title) =>
        {
            var form = new Form();
            form.Text = title;
            var buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 30;
            var okButton = new Button() { DialogResult = DialogResult.OK, Text = "OK" };
            var cancelButton = new Button() { DialogResult = DialogResult.Cancel, Text = "Cancel", Left = 80 };
            var listbox = new ListBox();
            listbox.Dock = DockStyle.Fill;
            listbox.Items.AddRange(options.ToArray());
            listbox.SelectedItem = options[0];
            form.Controls.Add(listbox);
            form.Controls.Add(buttonPanel);
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            var result = form.ShowDialog();
            if (result == DialogResult.Cancel) return null;
            return listbox.SelectedItem.ToString();
        };
        //let the user select the name of the macro to copy
        String select = SelectString(OptionList, "Choose a macro");
        //check that indeed one macro was selected
        if (select == null)
        {
            Info("You cancelled!");
        }
        return select;
    }
}

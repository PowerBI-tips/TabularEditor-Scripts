//Name: Default Measure Description
//Tooltip: Adds expression to the description
//Enable: Model
//Created by Mihaly Kavasi (updated by Ed Hansberry's idea)
//Tabular Editor version 2.16.0

//It is better to format DAX before adding it into the descriptions
foreach(var m in Model.AllMeasures)
{
    if(m.Description == "")
    {    
        m.Description =  "Expression:" + "\n" + m.Expression;
    }
    else if (!m.Description.Contains("Expression"))
    {
        m.Description = m.Description + "\n" + "Expression:" + "\n" + m.Expression;
    }
    else
    {
        // '2021-07-05 / B.Agullo / reset expressions already added
        int pos = m.Description.IndexOf("Expression",0); 
        bool onlyExpression = (pos == 0);
        
        if (onlyExpression) {
            m.Description = "Expression:" + "\n" + m.Expression;
        } else {
            m.Description = m.Description.Substring(0,pos-1)  + "\n" + "Expression:" + "\n" + m.Expression;
        }
    }
}

Model.AllMeasures.FormatDax();
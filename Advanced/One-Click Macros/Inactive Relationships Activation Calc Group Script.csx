#r "Microsoft.VisualBasic"
using System.Windows.Forms; 
using Microsoft.VisualBasic

//2022-09-14 / B.Agullo / first release
//CREATE CALCULATION GROUP TO ENABLE INACTIVE RELEATIONSHIPS
//Select releationships of your model and run the script

//initialize calc group variable
CalculationGroupTable cg = null as CalculationGroupTable;

//create calc group (only first time)
if(!CalculationGroupUtils.CreateCalculationGroup(
    model:Model,
    myCalculationGroup: out cg,
    defaultCalculationGroupName: "Model",annotationValue:"Model modifications")) return;

//goes through selected relationships
foreach (SingleColumnRelationship r in Selected.SingleColumnRelationships)
{
    //and only for those inactive
    if (!r.IsActive)
    {   
        //prepare name and expression for the calculation item
        string calcItemName = r.FromColumn.DaxObjectFullName + " - " + r.ToColumn.DaxObjectFullName;
        string calcItemExpression =
            String.Format(
                "CALCULATE(SELECTEDMEASURE(),USERELATIONSHIP({0},{1}))",
                r.FromColumn.DaxObjectFullName,
                r.ToColumn.DaxObjectFullName
            );
        
        //add calculation item
        CalculationItem calcItem = null as CalculationItem; 
        if(!CalculationGroupUtils.CreateCalculationItem(
            out calcItem,
            cg: cg,
            defaultCalculationItemName: calcItemName,
            promptUser: false, 
            calcItemExpression:calcItemExpression,
            regenerateIfPresent: false)) return;
    }
}
 }
        public static class StringUtils
    {
        public enum CheckType
        {
            Table,
            Measure,
            Column,
            CalculationItem,
            None
        }
        public static bool InitString
            (
                out string stringToInit,
                string label = "Name",
                string defaultValue = "Default Name",
                string errorMessage = "No name provided",
                string alreadyUsedNameErrorMessage = "There is already another {0} called {1}", //collision with names alerady in the model
                string invalidNameErrorMessage = "Name cannot be any of the following: {0}", //arbitrary list of forbidden names
                string[] invalidNames = null,
                Model model = null,
                Table table = null,
                CalculationGroupTable cg = null,
                CheckType checkType = CheckType.None,
                string prompt = "Default Prompt",
                string valueOnError = "",
                bool promptUser = true //if false, validate only default name is not forbidden, ask for input if it is
            )
        {


            bool validName;
            string userValue;
            stringToInit = valueOnError;

            do
            {
                if (promptUser)
                {
                    //get name from user
                    //userValue = Interaction.InputBox(prompt, label, defaultValue, 740, 400);
                    if (!StringUtils.Input(
                        userString: out userValue,
                        prompt: prompt,
                        label: label,
                        defaultValue: defaultValue)
                    )
                    {
                        stringToInit = valueOnError;
                        return false;
                    };
                }
                else
                {
                    //bypass interaction
                    userValue = defaultValue;
                }

                switch (checkType)
                {
                    case CheckType.None:
                        //nothing to check all is good 
                        validName = true;
                        break;

                    case CheckType.Table:
                        if (model == null)
                        {
                            throw new ArgumentNullException();
                        }
                        //if it's going to be a table, there cannot be any table with the same name
                        validName = !model.Tables.Any(x => x.Name == userValue);

                        if (!validName)
                        {
                            MessageUtils.ErrorMessage(
                                String.Format(
                                    alreadyUsedNameErrorMessage,
                                    "table",
                                    userValue
                                )
                            );
                        }
                        break;

                    case CheckType.Column:
                        if (model == null)
                        {
                            throw new ArgumentNullException();
                        }

                        //if its a column, there cannot be any other column on that table with the same name
                        validName = !table.Columns.Any(x => x.Name == userValue);

                        if (!validName)
                        {
                            MessageUtils.ErrorMessage(
                                String.Format(
                                    alreadyUsedNameErrorMessage,
                                    "column", userValue
                                )
                            );
                        };

                        break;

                    case CheckType.Measure:
                        if (model == null)
                        {
                            throw new ArgumentNullException();
                        }
                        if (table == null)
                        {
                            throw new ArgumentNullException();
                        }


                        //if its a measure, there cannot be any measure with the same name, or any column on that table with the same name 
                        validName = !table.Columns.Any(x => x.Name == userValue)
                                    && !model.AllMeasures.Any(x => x.Name == userValue);

                        if (!validName)
                        {
                            MessageUtils.ErrorMessage(
                                String.Format(
                                    alreadyUsedNameErrorMessage,
                                    "measure in the model or column in " + table.Name,
                                    userValue
                                )
                            );
                        };
                        break;


                    case CheckType.CalculationItem:
                        if (cg == null)
                        {
                            throw new ArgumentNullException();
                        }
                        //if its a calculation item, there cannot be any other calculation item with the same name
                        validName = !cg.CalculationItems.Any(x => x.Name == userValue);

                        if (!validName)
                        {
                            MessageUtils.ErrorMessage(
                                String.Format(
                                    alreadyUsedNameErrorMessage,
                                    "calculation item",
                                    userValue
                                )
                            );
                        };
                        break;


                    default:
                        validName = true;
                        break;


                }

                //if we got a valid name so far check it's not in the invalid name list
                if (validName)
                {
                    //if no invalid names then.. 
                    if (invalidNames == null || invalidNames.Length == 0)
                    {
                        //..all good
                        validName = true;
                    }
                    else
                    {
                        //otherwise check if the name is any of the invalid ones
                        validName = !invalidNames.Contains(userValue);
                        if (!validName)
                        {
                            //tell user this name cannot be used.
                            MessageUtils.ErrorMessage(
                                String.Format(invalidNameErrorMessage, String.Join(", ", invalidNames))
                            );
                        }
                    }
                }

                if (!validName)
                {
                    //enable user interaction to fix it
                    promptUser = true;
                }


            } while (!validName);

            //emptystring will be counted as valid string, but is not a valid name ever and we take it as a key to abort
            if (string.IsNullOrWhiteSpace(userValue))
            {
                MessageUtils.ErrorMessage(errorMessage);
                stringToInit = valueOnError;
                return false;
            };

            //if we reach this point then is all good
            stringToInit = userValue;
            return true;
        }

        public static bool Input
            (
                out string userString,
                string prompt = "Enter string",
                string label = "String",
                string defaultValue = "some text",
                int xPosition = 740,
                int yPosition = 400,
                bool nullOrWhiteSpaceAccepted = false,
                string errorMessage = "Null or Empty String provided",
                string unexpectedErrorMessage = "Unexpected Error in StringUtils.Input",
                string valueOnError = ""
            )
        {
            try
            {
                string tempUserString = Microsoft.VisualBasic.Interaction.InputBox(prompt, label, defaultValue, XPos: xPosition, YPos: yPosition);

                if (string.IsNullOrWhiteSpace(tempUserString) && !nullOrWhiteSpaceAccepted)
                {
                    MessageUtils.ErrorMessage(errorMessage);
                    userString = valueOnError;
                    return false;
                }
                else
                {
                    userString = tempUserString;
                    return true;
                };
            }
            catch
            {
                MessageUtils.ErrorMessage(unexpectedErrorMessage);
                userString = valueOnError;
                return false;
            }

        }
    }

    public static class MessageUtils
    {
        public static void ErrorMessage
            (
                string errorMessage,
                bool showMessage = true,
                string caption = "Error"
            )
        {
            try
            {
                if (showMessage)
                {
                    MessageBox.Show( errorMessage,  caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                };

                return;
            }
            catch
            {
                //can't really call error message right? 
                return;
            };
        }



        public static bool IsAnswerOK
            (
                string question,
                string caption = "Before we proceed"

            )
        {
            try
            {
                DialogResult dialogResult = MessageBox.Show(question,  caption, MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    return true;
                }
                else if (dialogResult == DialogResult.No)
                {
                    return false;
                }
                else
                {
                    return false; //??
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool SelectFromList
            (
                List<string> selectionList,
                out List<string> selectedItems,
                SelectionMode selectionMode = SelectionMode.One,
                string title = "Select from this list",
                string cancelMessage = "You cancelled the process",
                bool selectionRequired = true,
                string selectionRequiredMessage = "Select item or cancel",
                bool skipDialogIfSingleItem = false,
                bool preselectFirstItem = false,
                bool showEmptyListError = true,
                string emptyListErrorMessage = "Empty selection list"

            )
        {

            selectedItems = new List<string>(); //initialize return list 

            if (selectionList.Count == 0)
            {
                if (showEmptyListError)
                    MessageUtils.ErrorMessage(emptyListErrorMessage);
                return false;
            };

            //general stuff
            var form = new Form();
            form.Text = title;

            //it shows on top of the list box??
            //var titleLabel = new Label();
            //titleLabel.Text = title;
            //titleLabel.Dock = DockStyle.Top;

            //button pannel at the bottom
            var buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 30;

            //individual buttons
            var okButton = new Button() { DialogResult = DialogResult.OK, Text = "OK" };
            var cancelButton = new Button() { DialogResult = DialogResult.Cancel, Text = "Cancel", Left = 80 };

            //listbox
            var listBox = new ListBox();
            listBox.Dock = DockStyle.Fill;
            listBox.SelectionMode = selectionMode;

            //fills listbox with options from selectionList
            listBox.Items.AddRange(selectionList.ToArray());

            //preselects first item by default
            if (preselectFirstItem)
                listBox.SelectedItem = selectionList[0];

            //putting pieces together
            //form.Controls.Add(titleLabel);
            form.Controls.Add(listBox);
            form.Controls.Add(buttonPanel);
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            //make sure it shows up in the middle of the screen
            form.StartPosition = FormStartPosition.CenterScreen;

            bool oneOrMoreSelected; //flag to check listbox selection

            bool askAgain; //flag to ask again if no selection done but selection is required
            DialogResult result; //variable to store the result of the dialog box

            if (!skipDialogIfSingleItem || selectionList.Count > 1)
            {
                do
                {
                    //shows the form
                    result = form.ShowDialog();

                    if (result != DialogResult.OK)
                    {
                        //user cancelled (or aborted),  return false, selectedItems is empty list
                        MessageUtils.ErrorMessage(cancelMessage);
                        return false;
                    }

                    //true if one or more items are selected in the listbox
                    oneOrMoreSelected = ListBoxUtils.OneOrMoreSelected(listBox);

                    //if selection required and non present, raise flag
                    askAgain = selectionRequired && !oneOrMoreSelected;

                    //if flag is raised show message to select something or cancel
                    if (askAgain)
                        MessageUtils.ErrorMessage(selectionRequiredMessage);

                } while (askAgain);

                //if we reached this point user selected ok and selected something if so required

                // Loop through all items the ListBox.
                for (int x = 0; x < listBox.Items.Count; x++)
                {
                    // Determine if the item is selected and add to the list.
                    if (listBox.GetSelected(x) == true)
                        selectedItems.Add(listBox.Items[x].ToString());
                }

            }
            else
            {
                //if we reach this point is because
                //skipDialogIfSingleItem && selectionList.Count == 1
                selectedItems.Add(selectionList.First());

            };



            //return true that all went fine
            return true;


        }


    }

    public static class ListBoxUtils
    {
        public static bool OneOrMoreSelected(ListBox listBox)
        {
            for (int x = 0; x < listBox.Items.Count; x++)
            {
                // Determine if the item is selected.
                if (listBox.GetSelected(x) == true)
                    return true;
            }
            return false;
        }
    }

    public static class MeasureUtils
    {

        public enum CreateMode
        {
            EnforceNewName,
            DeleteAndCreate,
            UseExisting
        }
        public static Measure CreateMeasure
            (
                Table baseTable,
                string defaultMeasureName = "New Measure",
                string measureExpression = "",
                string formatString = "",
                string measureNameLabel = "Measure Name",
                string displayFolder = null,
                CreateMode createMode = CreateMode.EnforceNewName,
                bool allowCustomName = true

            )
        {
            /*test*/
            if (baseTable == null)
            {
                throw new ArgumentNullException();
            }

            Measure returnMeasure;

            string userMeasureName;

            StringUtils.CheckType check;

            if (createMode.Equals(CreateMode.EnforceNewName))
            {
                check = StringUtils.CheckType.Measure;
            }
            else
            {
                check = StringUtils.CheckType.None;
            };


            if (allowCustomName)
            {
                //allow user to select name
                if (
                    !StringUtils.InitString(
                        stringToInit: out userMeasureName,
                        label: measureNameLabel,
                        defaultValue: defaultMeasureName,
                        checkType: check)) { return null; }
            }
            else
            {
                userMeasureName = defaultMeasureName;
            }

            returnMeasure =
                GetMeasure(
                    measureName: userMeasureName,
                    model: baseTable.Model);

            if (returnMeasure != null)
            {
                if (createMode.Equals(CreateMode.DeleteAndCreate))
                {
                    //delete the measure found, will be recreated in a sec below
                    returnMeasure.Delete();
                }
                else if (createMode.Equals(CreateMode.UseExisting))
                {
                    //do not recreate anything, just return the measure found (process does not continue)
                    return returnMeasure;
                }
                else
                {
                    new Exception("this should not happen");
                };
            };

            //we reach this point if it's a new measure, or we are deleting and recreating the measure
            returnMeasure = baseTable.AddMeasure(name: userMeasureName, expression: measureExpression, displayFolder: displayFolder);
            returnMeasure.FormatString = formatString;

            return returnMeasure;

        }
        public static Measure GetMeasure(string measureName, Model model)
        {
            var matchingMeasures = model.AllMeasures.Where(x => x.Name == measureName);

            if (matchingMeasures.Count() == 0)
            {
                return null;
            }
            else
            {
                return matchingMeasures.First();
            }
        }

    }


    public static class TableUtils

    {
        public static int TableCount(Model model)
        {
            MessageBox.Show(model.Name);
            return model.Tables.Count;
        }

        public static Table GetTable(string tableName, Model Model)
        {
            var tbls = Model.Tables.Where(x => x.Name == tableName);

            if (tbls.Count() == 0)
            {
                return null;
            }
            else
            {
                return tbls.First();
            }

        }

        public static bool CreateMeasureTable
            (
                out Table createdTable,
                Model model,
                string defaultTableName = "Some Measures",
                string label = "Measure Table Name",
                string prompt = "Provide a name for the measure Table"
            )
        {

            createdTable = null;

            if (model == null)
            {
                throw new ArgumentNullException();
            };

            string tableName;
            if (
                !StringUtils.InitString(
                    stringToInit: out tableName,
                    label: label, prompt: prompt,
                    checkType: StringUtils.CheckType.Table,
                    defaultValue: defaultTableName,
                    model: model
                )
               ) return false;



            string tableExpression = "{0}";

            createdTable = model.AddCalculatedTable(name: tableName, expression: tableExpression);

            return true;

        }

        public static Table CreateMeasureTable2
            (
                Model model,
                string defaultTableName = "Some Measures",
                string label = "Measure Table Name",
                string prompt = "Provide a name for the measure Table"
            )
        {
            if (model == null)
            {
                throw new ArgumentNullException();
            }



            return model.AddCalculatedTable(name: defaultTableName, expression: "{0}");

        }


    }

    public static class CalculationGroupUtils
    {

        public enum CreateMode
        {
            EnforceNewName,
            DeleteAndCreate,
            UseExistingWithConfirmation,
            UseExistingWithoutConfirmation
        }


        // 
        public static bool CreateCalculationGroup
            (
                out CalculationGroupTable myCalculationGroup,
                Model model,
                string defaultCalculationGroupName = "myCalcGroup",
                string defaultCalculationGroupColumnName = "myCalcItems",
                bool matchColumnAndCalculationGroupName = true,
                string annotationName = "ExtendedTOMWrapper",
                string annotationValue = "Default Calculation Group",
                string prompt = "Provide name for the calculation group",
                string columnPrompt = "Provide a name for the column of the calculation group",
                string inputFieldLabel = "Name",
                string calcGroupDescription = ""


            )
        {


            string calcGroupName;
            string calcGroupColumnName;

            var ts = model.Tables.Where(x => x.GetAnnotation(annotationName) == annotationValue);

            myCalculationGroup = null as CalculationGroupTable;

            if (ts.Count() == 1)
            {
                myCalculationGroup = ts.First() as CalculationGroupTable;
            }
            else if (ts.Count() < 1)
            {

                if (
                    !StringUtils.InitString(
                        stringToInit: out calcGroupName,
                        prompt: prompt,
                        label: inputFieldLabel,
                        defaultValue: defaultCalculationGroupName,
                        checkType: StringUtils.CheckType.Table,
                        model: model
                    )
                ) { return false; }

                myCalculationGroup = model.AddCalculationGroup(calcGroupName);
                myCalculationGroup.Description = calcGroupDescription;
                myCalculationGroup.SetAnnotation(annotationName, annotationValue);


                if (matchColumnAndCalculationGroupName)
                {
                    defaultCalculationGroupColumnName = calcGroupName;
                }
                else
                {
                    if (
                        !StringUtils.InitString(
                           stringToInit: out calcGroupColumnName,
                           prompt: columnPrompt
                        )
                    ) { return false; };
                };

                model.Tables[calcGroupName].Columns["Name"].Name = defaultCalculationGroupColumnName;

            }
            else
            {
                //this should never happen -- who needs two calc groups for time intelligence? 
                //myCalculationGroup = SelectTable(ts, label: "Select your 'Measure Group' Calculation Group") as CalculationGroupTable;
                myCalculationGroup = ts.First() as CalculationGroupTable;
            };

            if (myCalculationGroup == null)
            {

                return false;
            } // doesn't work in TE3 as cancel button doesn't return null in TE3

            return true;

        }


        public enum GetByMode
        {
            ByName,
            ByAnnotation
        }

        //2022-02-19 / B.Agullo / 
        public static bool GetCalculationGroup
        //get reference to an existing calcualtion group (if found)
            (
                out CalculationGroupTable myCalculationGroup,
                Model model,
                string selectionPrompt = "Select calculation group",
                bool filterByAnnotation = false,
                string annotationName = null,
                string annotationValue = null,
                string calcGroupName = null,
                bool skipIfOnlyOneMatches = true

            )
        {
            if (model == null)
            {
                throw new ArgumentNullException();
            }

            myCalculationGroup = null;

            if (model.CalculationGroups.Count() == 0) return false;

            List<string> matchingCalcGroupNames = new List<string>();

            List<string> selectedCalcGroupNames = new List<string>();

            if (annotationName != null && annotationValue != null)
            {
                matchingCalcGroupNames =
                    (List<string>)model.CalculationGroups
                        .Where(x => x.GetAnnotation(annotationName) == annotationValue)
                        .Select(x => x.Name).ToList();
            }
            else if (calcGroupName != null)
            {
                matchingCalcGroupNames =
                    (List<string>)model.CalculationGroups
                    .Where(x => x.Name == calcGroupName)
                    .Select(x => x.Name).ToList();
            }
            else
            {
                matchingCalcGroupNames =
                    model.CalculationGroups
                        .Select(x => x.Name).ToList();
            }




            if (!MessageUtils.SelectFromList(selectionList: matchingCalcGroupNames,

                selectedItems: out selectedCalcGroupNames
                )) return false;


            myCalculationGroup = model.CalculationGroups.Where(x => x.Name == matchingCalcGroupNames.First()).First();


            return true;
        }

        public static bool CreateCalculationItem
             (
                out CalculationItem myCalcItem,
                CalculationGroupTable cg,
                string defaultCalculationItemName = "New Calc Item",
                bool promptUser = false,
                string prompt = "Provide name for the calculation item",
                string inputFieldLabel = "Name",
                string calcItemDescription = "",
                string calcItemExpression = "",
                bool regenerateIfPresent = true

            )
        {
            myCalcItem = null as CalculationItem;
            
            if(calcItemExpression == String.Empty)
            {
                MessageUtils.ErrorMessage("No calc Item Expression provided.");
                return false;
            };

            string calcItemName;

            if (promptUser)
            {
                if (
                        !StringUtils.InitString
                            (
                                out calcItemName, 
                                label: "Calculation Item Name", 
                                defaultValue: defaultCalculationItemName,
                                cg:cg,
                                checkType: StringUtils.CheckType.CalculationItem,
                                prompt: prompt,
                                promptUser: promptUser
                                
                            )
                ) return false; //the naming step didn't go well
            }
            else
            {
                calcItemName = defaultCalculationItemName;
            }

            if(cg.CalculationItems.Any(x=> x.Name == calcItemName))
            {
                if(regenerateIfPresent)
                {
                    cg.CalculationItems.Where(x => x.Name == calcItemName).First().Delete();
                }
                else
                {
                    return true; //all is good, nothing to do
                }
                
            }
            
            CalculationItem calcItem = cg.AddCalculationItem(name: calcItemName, expression: calcItemExpression);
            calcItem.Description = calcItemExpression;
            calcItem.FormatDax();

            return true;



        }
    }
    private class __nobracket {
        
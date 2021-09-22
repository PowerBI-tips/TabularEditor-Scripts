/*
    Best Practice Analyzer enables options in Tools to check best practices in models and apply fixes 
    to items in the model which may hinder performance. Past code in advanced editor and run, 
    then close tabular editor, re-open and click Tools at the top.
*/

 

System.Net.WebClient w = new System.Net.WebClient();  
 
string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData); 
string url = "https://raw.githubusercontent.com/microsoft/Analysis-Services/master/BestPracticeRules/BPARules.json"; 
string downloadLoc = path+@"\TabularEditor\BPARules.json"; 
w.DownloadFile(url, downloadLoc); 
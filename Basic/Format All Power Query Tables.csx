
//
// Author: Tommy Puglia
// Original Author: Kurt Buhler 
// Much Thanks goes to Kurt based on this article on a selected partition, only updated I made was all m partitions in the model.

// Please see Kurt's original article:
// https://data-goblins.com/power-bi/format-power-query-automatically


// Check out the Explicit Measures Podcast on Apple, spotify, & YouTube!
// https://www.youtube.com/playlist?list=PLn1m_aBmgsbHr83c1P6uqaWF5PLdFzOjj
//
// Format all Power Query tables in the model
//
// Simply run this on on the model, and done!

#r "C:\Program Files\Tabular Editor 3\TabularEditor3.exe" // *** Needed for C# scripting, remove in TE3 ***
#r "C:\Program Files (x86)\Tabular Editor 3\TabularEditor3.exe" // *** Needed for C# scripting, remove in TE3 ***

using TabularEditor.TOMWrapper; // *** Needed for C# scripting, remove in TE3 ***
using TabularEditor.Scripting; // *** Needed for C# scripting, remove in TE3 ***
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

Model Model; // *** Needed for C# scripting, remove in TE3 ***
TabularEditor.Shared.Interaction.Selection Selected; // *** Needed for C# scripting, remove in TE3 ***


// URL of the powerqueryformatter.com API
string powerqueryformatterAPI = "https://m-formatter.azurewebsites.net/api/v2";

// HttpClient method to initiate the API call POST method for the URL
HttpClient client = new HttpClient();
HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, powerqueryformatterAPI);

int _partitions = 0;
int _whatifparameters = 0;
int _fieldparameters = 0;
foreach (  var _table  in Model.Tables )
{
    foreach (  var _partition  in _table.Partitions )
    {
        string _type = Convert.ToString(_partition.SourceType);
        string _exp = Convert.ToString(_partition.Expression);
        if ( _type == "M" )
        {
            _partitions = _partitions + 1;
            string partitionExpression = _partition.Expression;


          var requestBody = JsonConvert.SerializeObject(
    new { 
        code = partitionExpression,             // Mandatory config
        resultType = "text",                    // Mandatory config
        lineWidth = 40                          // Optional config
        // alignLineCommentsToPosition = true,  // Optional config
        // includeComments = true               // Optional config
    });

// Set the "Content-Type" header of the request to "application/json" and the encoding to UTF-8
var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

// Retrieve the response
var response = client.PostAsync(powerqueryformatterAPI, content).Result;

// If the response is successful
if (response.IsSuccessStatusCode)
{
    // Get the result of the response
    var result = response.Content.ReadAsStringAsync().Result;

    // Parse the response JSON object from the string
    JObject data = JObject.Parse(result.ToString());

    // Get the formatted Power Query response
    string formattedPowerQuery = (string)data["result"];

    ////////////////////////////////////////////////////////////////////////// Can remove everything in this section
    // OPTIONAL MANUAL FORMATTING                                           // Additional formatting on top of API config
    // Manually add a new line and comment to each step                     //
    var replace = new Dictionary<string, string>                            //
    {                                                                       //
        { "\n//", "\n\n//" },                                               // New line at comment
        { "\n  #", "\n\n  // Step\n  #" },                                  // New line & comment at new standard step
        { "\n  Source", "\n\n  // Data Source\n  Source" },                 // New line & comment at Source step
        { "\n  Dataflow", "\n\n  // Dataflow Connection Info\n  Dataflow" },// New line & comment at Dataflow step
        {"\n  Data =", "\n\n  // Step\n  Data ="},                          // New line & comment at Data step
        {"\n  Navigation =", "\n\n  // Step\n  Navigation ="},              // New line & comment at Navigation step
        {"in\n\n  // Step\n  #", "in\n  #"},                                // 
        {"\nin", "\n\n// Result\nin"}                                       // Format final step as result
    };                                                                      //
                                                                            //
    // Replace the first string in the dictionary with the second           //
    var manuallyformattedPowerQuery = replace.Aggregate(                    //
        formattedPowerQuery,                                                //
        (before, after) => before.Replace(after.Key, after.Value));         //
                                                                            //
    // Replace the auto-formatted code with the manually formatted version  //
    formattedPowerQuery = manuallyformattedPowerQuery;                      //
    //////////////////////////////////////////////////////////////////////////

    // Replace the unformatted M expression with the formatted expression
    _partition.Expression = formattedPowerQuery;

    // Pop-up to inform of completion
  
}

// Otherwise return an error message
else
{
Info(
    "API call unsuccessful." +
    "\nCheck that you are selecting a partition with a valid M Expression."
    );
}





        }
        else if ( _type == "Calculated" && _exp.Contains("NAMEOF") )
        {
            _fieldparameters = _fieldparameters + 1;
        }
        else if ( _type == "Calculated" && _exp.Contains("GENERATESERIES") )
        {
            _whatifparameters = _whatifparameters + 1;
        }
            
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Formatting;

namespace ImageRecognitionFunction
{
    public static class ImageRecognitionFunction
    {
        private static readonly string subscriptionKey = Environment.GetEnvironmentVariable("ENTITY_SEARCH_KEY");
        private static readonly string endpoint = Environment.GetEnvironmentVariable("ENTITY_SEARCH_ENDPOINT");


        [FunctionName("AnalyseImage")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "options", Route = null)] HttpRequest req, ILogger log)
        {

            req.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // Add your Computer Vision subscription key and endpoint to your environment variables.


            log.LogInformation("C# HTTP trigger function processed a request.");

            string imageUrl = req.Query["url"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            imageUrl ??= data?.imageUrl;

            try
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                // The Analyze Image method returns information about the following
                // visual features:
                // Categories:  categorizes image content according to a
                //              taxonomy defined in documentation.
                // Description: describes the image content with a complete
                //              sentence in supported languages.
                // Color:       determines the accent color, dominant color, 
                //              and whether an image is black & white.
                string requestParameters = "visualFeatures=Categories,Description,Color";

                // Assemble the URI for the REST API method.
                string uri = endpoint + "?" + requestParameters;

                HttpResponseMessage response;

                WebClient webClient = new WebClient();
                // Read the contents of the specified local image
                // into a byte array.
                log.LogInformation(imageUrl);
                log.LogInformation(uri);


                    // The other content types you can use are "application/json"
                    // Asynchronously call the REST API method.
                    var formatter = new JsonMediaTypeFormatter();

                    response = await client.PostAsync(uri, new { url = imageUrl }, formatter);
      

                // Asynchronously get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();
                // Display the JSON response.
                log.LogInformation("\nResponse:\n\n{0}\n", JToken.Parse(contentString).ToString());
                return (ActionResult)new OkObjectResult(JToken.Parse(contentString));
            }

            catch (Exception e)
            {
                log.LogInformation("\n" + e.Message);
            }

            return imageUrl != null
                ? (ActionResult)new OkObjectResult("Success")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }

}

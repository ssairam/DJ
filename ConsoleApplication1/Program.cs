using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;


namespace ConsoleApplication1
{
    class Program
    {
        //private static string base_URL = "https://api.dowjones.com/";
        //private static string folders = "alpha/analytics";
        private static string base_URL = "https://api.dowjones.com/";
        private static string folders = "alpha/extractions/documents";
        
        private static string headers = "{'content-type': 'application/json', 'user-key': '{8b25a22cc8ea7c69148194ea2aae4ecf}'}";
        private static HttpClient client = new HttpClient();
        private static string request_body = @"{
  query: {
    where: language_code = 'en' ,
    includes: {
      company_codes: ['ubrti']
    },
    limit: 1000
  }
}";

        static void Main(string[] args)
        {
            var response = PostRequest();
            string results = string.Empty;

            if (response.IsSuccessStatusCode)
            {
                results = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                Console.WriteLine("Cannot perform the get requests as the post request failed.");
                return;
            }

            var linkForGetRequest = getLink(results);//Result is expected to be JSON. cannot test because getting an error. 
            var state = getState(response.ToString());

            if (string.Empty == linkForGetRequest)
            {
                Console.WriteLine("There was an error with the Post request. Can't perform the get request because the getLink is empty");
                return;
            }

            CheckJobStatusByGetRequests(linkForGetRequest, state);
        }

        private static void CheckJobStatusByGetRequests(string url, string state)
        {
            try
            {
                while (state != "JOB_STATE_DONE") 
                {
                    HttpResponseMessage response = client.GetAsync(folders).Result;
                    JObject o = JObject.Parse(response.ToString());
                    state = o["data"]["attributes"]["current_state"].ToString();
                }
                Console.WriteLine("The job is completed and data can be downloaded using Curl command. ");
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        /// <summary>
        /// Following code should initiate a request.
        /// </summary>
        private static HttpResponseMessage PostRequest()
        {
            try
            {
                client.BaseAddress = new Uri(base_URL);
                // Add an Accept header for JSON format.  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // Start the job
                return client.PostAsJsonAsync(folders, "").Result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// The response will be in the following format
        /**{
              "data": {
                "attributes": {
                  "current_state": "JOB_QUEUED",
                  "extraction_type": "documents"
                },
                "id": "dj-synhub-extraction-mixoflettersandnumber1234567890"
              },
              "links": {
                "self": "https://api.dowjones.com/alpha/extractions/documents/dj-synhub-extraction-sample-extraction"
              }
            }
        **/
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static string getLink(string response)
        {
            JObject  o= JObject.Parse(response);
            return o["links"]["self"].ToString();
        }

        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static string getState(string response)
        {
            JObject o = JObject.Parse(response);
            return o["data"]["attributes"]["current_state"].ToString();
        }

        #region testMethods
        private static string initial_response = @"{
              data: {
                attributes: {
                  current_state: 'JOB_QUEUED',
                  extraction_type: 'documents'
                },
                id: 'dj-synhub-extraction-mixoflettersandnumber1234567890'
              },
              links: {
                self: 'https://api.dowjones.com/alpha/extractions/documents/dj-synhub-extraction-sample-extraction'
              }
            }"
;
        private static void testResponse()
        {
            var url = getLink(initial_response);
            Console.WriteLine(url);
            Console.WriteLine();
        }

        private static void testCurrentState()
        {
            var url = getState(initial_response);
            Console.WriteLine(url);
            Console.WriteLine();
        }

        #endregion
    }
}

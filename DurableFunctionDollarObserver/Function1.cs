using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DurableFunctionDollarObserver
{
    public static class Function1
    {
        [FunctionName("DollarOb")]
        public static async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {

            var expiryTime = DateTime.Now.AddMinutes(30);

            List<string> consumers = new List<string>()
            {
                "ferrerallan@gmail.com"
            };

            while (context.CurrentUtcDateTime < expiryTime)
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("https://openexchangerates.org/api/latest.json?app_id=4e74513fc6bd41d9804ea4b259fb5a21");
                var json = await response.Content.ReadAsStringAsync();
                var content = JsonConvert.DeserializeObject<dynamic>(json);

                var jobStatus = await context.CallActivityAsync<string>("GetJobStatus", consumers);
                if (jobStatus == "Completed")
                {
                    // Perform an action when a condition is met.
                    await context.CallActivityAsync("SendMessage", "");
                    break;
                }

                // Orchestration sleeps until this time.
                var nextCheck = context.CurrentUtcDateTime.AddSeconds(60);
                await context.CreateTimer(nextCheck, CancellationToken.None);
            }
        }

        [FunctionName("SendMessage")]
        public static string SayHello([ActivityTrigger] List<string> name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("HttpStart")]
        public static async Task<HttpResponseMessage> Run(
      [HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "DollarOb")] HttpRequestMessage req,
      [DurableClient] IDurableClient starter,
      string functionName,
      ILogger log)
        {
            // Function input comes from the request content.
            object eventData = await req.Content.ReadAsAsync<object>();
            string instanceId = await starter.StartNewAsync(functionName, eventData);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


    }
}
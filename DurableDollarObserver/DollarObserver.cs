using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using DurableDollarObserver.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DurableDollarObserver
{
    public static class Function1
    {
        [FunctionName("DollarObserver")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var expiryTime = DateTime.Now.AddMinutes(1);

            List<string> consumers = new List<string>()
            {
                "ferrerallan@gmail.com"
            };

            while (DateTime.Now < expiryTime)
            {
                using var httpClient = new HttpClient();
                var response = httpClient.GetAsync($"https://openexchangerates.org/api/latest.json?app_id={Environment.GetEnvironmentVariable("APIKEY")}").Result;
                var json = response.Content.ReadAsStringAsync().Result;
                var content = JsonConvert.DeserializeObject<CurrenciesResponse>(json);

                if (content.rates.BRL > 5.0)
                {
                    await context.CallActivityAsync("DollarObserver_SendMessage", consumers);
                   
                }
                
                // Orchestration sleeps until this time.
                var nextCheck = context.CurrentUtcDateTime.AddSeconds(60);
                await context.CreateTimer(nextCheck, CancellationToken.None);
            }
            //returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return consumers;
        }

        [FunctionName("DollarObserver_SendMessage")]
        public static string SendMail([ActivityTrigger] List<string> consumers, ILogger log)
        {
            var userEmail = (Environment.GetEnvironmentVariable("USER_EMAIL"));
            var userPassword = (Environment.GetEnvironmentVariable("USER_PASSWORD"));
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 465;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(userEmail, userPassword);

            consumers.ForEach(consumer =>
            {
                MailMessage message = new MailMessage(userEmail,
                                                        consumer,
                                                        "dollar price has been reached!", 
                                                        "advise from azure durable function");
                client.Send(message);
                log.LogInformation($"Sending mail to {consumer}.");
            });
            
            return $"OK";
        }

        [FunctionName("DollarObserver_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DollarObserver", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
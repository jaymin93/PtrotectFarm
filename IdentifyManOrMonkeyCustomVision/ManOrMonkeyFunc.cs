using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net.Http;
using System.Diagnostics;

namespace IdentifyManOrMonkeyCustomVision
{
    public static class ManOrMonkeyFunc
    {

        private static string trainingEndpoint = "enter correct end point from azure portal";
        private static string trainingKey = "enter correct key from azure portal";

        private static string predictionEndpoint = "enter correct end point from azure portal";
        private static string predictionKey = "enter correct key from azure portal";


        private static string publishedModelName = "Iteration1";

        private static string StorageAccountURIWithConatinerName = "enter correct storage account uri with container name";

        private static string ProjectGUID = "enter correct project guid from custom vision portal";

        private static string ManTagname = "man";

        private static string MonkeyTagname = "monkey";

        private static string TableName = "helpfarmer";

        private const string playsound = "playing";

        private const string status = "status";

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        static ManorMonkeyDeatails CurrentManorMonkeyDeatails;


        static CloudStorageAccount storageAccount = CloudStorageAccount.Parse("Please enter correct connection string from azure portal");

        static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        static CloudTable table = tableClient.GetTableReference(TableName);


        [FunctionName("ManOrMonkeyFunc")]
        public async static Task Run([BlobTrigger("manormonkey/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob, string name, ILogger log)
        {

            CustomVisionTrainingClient trainingApi = AuthTraining(trainingEndpoint, trainingKey);
            CustomVisionPredictionClient predictionApi = AuthPrediction(predictionEndpoint, predictionKey);

            Project project = GetExistingProject(trainingApi);

            var response = await TestManORMonkeyPrediction(predictionApi, project, $"{StorageAccountURIWithConatinerName}{name}");

            if (response.monkey > response.man)
            {
                await InsertIncidentgDeatilsTOAzureTable($"Please review recent image looks like monkeys are entering into the farm probability is {response.monkey:P1}", $"{StorageAccountURIWithConatinerName}{name}", log);

                await InsertIncidentgDeatilsTOAzureTable(string.Empty, string.Empty, log, true);
            }

        }




        public static async Task<bool> InsertIncidentgDeatilsTOAzureTable(string message, string imageurl, ILogger log, bool insertstatus = false)
        {
            try
            {


                ManorMonkeyDeatails details= null;

                if (insertstatus)
                {
                    await GetCurrentSoundPlayingStatusAsync();

                    CurrentManorMonkeyDeatails.SoundPlayingStatus = playsound;

                    CurrentManorMonkeyDeatails.IncidentTime = DateTime.Now;

                    TableOperation updateoperation = TableOperation.Replace(CurrentManorMonkeyDeatails);
                }
                else
                {

                    details = new ManorMonkeyDeatails($"{TableName}", $"{TableName}{DateTime.Now:dd-MM-yyyy-HH-mm-ss}");

                    details.IncidentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                    details.Message = message;
                    details.ImageURL = imageurl;
                }



                TableOperation tblops = null;

                TableResult operationresult = null;

                if (insertstatus)
                {
                    tblops = TableOperation.Replace(CurrentManorMonkeyDeatails);

                    operationresult = await table.ExecuteAsync(tblops);
                }
                else
                {
                    tblops = TableOperation.Insert(details);

                    operationresult = await table.ExecuteAsync(tblops);

                }

                var sts = operationresult.HttpStatusCode;

                return true;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return default;
            }
        }



        public static async Task GetCurrentSoundPlayingStatusAsync()
        {
            try
            {
                TableQuery<ManorMonkeyDeatails> query;




                query = new TableQuery<ManorMonkeyDeatails>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, status));


                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<ManorMonkeyDeatails> resultSegment = await table.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);
                    token = resultSegment.ContinuationToken;

                    CurrentManorMonkeyDeatails = resultSegment.Results.FirstOrDefault();


                } while (token != null);



            }
            catch (Exception exp)
            {
                Debug.Write(exp);

            }
        }


        private static CustomVisionTrainingClient AuthTraining(string endpoint, string trainingKey)
        {

            CustomVisionTrainingClient trainingApi = new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(trainingKey))
            {
                Endpoint = endpoint
            };
            return trainingApi;
        }
        private static CustomVisionPredictionClient AuthPrediction(string endpoint, string predictionKey)
        {

            CustomVisionPredictionClient predictionApi = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = endpoint
            };
            return predictionApi;
        }


        private static Project GetExistingProject(CustomVisionTrainingClient trainingApi)
        {
            return trainingApi.GetProject(Guid.Parse(ProjectGUID));
        }

        private async static Task<(double man, double monkey)> TestManORMonkeyPrediction(CustomVisionPredictionClient predictionApi, Project project, string bloburi)
        {

            Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.ImageUrl imageUrl = new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.ImageUrl(bloburi);
            var result = await predictionApi.ClassifyImageUrlAsync(project.Id, publishedModelName, imageUrl);

            double manprob = result.Predictions.Where(x => x.TagName == ManTagname).FirstOrDefault().Probability;

            double monkeyprob = result.Predictions.Where(x => x.TagName == MonkeyTagname).FirstOrDefault().Probability;

            return (manprob, monkeyprob);

        }

    }
}


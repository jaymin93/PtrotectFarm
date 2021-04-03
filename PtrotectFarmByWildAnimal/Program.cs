using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ProtectFarm.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Media;
using System.Linq.Expressions;

namespace ProtectFarmers
{
    class Program
    {
        static IConfiguration config;

        internal static string TableName = "helpfarmer";

        const string status = "status";

        private const string playsound = "playing";

        private const string stopsound = "stopped";


        public static string CurrentSoundPlayingStatus
        {
            get; set;
        }

        public static string connectionstring { get; set; }

        public static string storageaccounturi { get; set; }

        public static string containername { get; set; }

        public static Timer tmr;

        public static SoundPlayer soundPlayer;

        public static object obj = new object();

        public async static Task Main(string[] args)
        {
            HostBuilder builder = new HostBuilder();

            config = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json", true, true)
             .Build();

            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
            fileSystemWatcher.Path = "G:\\cameradir";
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.EnableRaisingEvents = true;
            await builder.RunConsoleAsync();
        }

        private async static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            using (FileStream fileStream = new FileStream(e.FullPath, FileMode.Open))
            {
                connectionstring = config["connectionstring"];
                storageaccounturi = config["storageaccounturi"];
                containername = config["containername"];

                await UploadFileToAzureStorageAsync(fileStream, storageaccounturi, connectionstring, containername, DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + e.Name);
            }
           
            if (tmr == null)
            {
                tmr = new Timer();
                tmr.Enabled = true;
                tmr.Interval = 15000;
                tmr.Elapsed += Tmr_Elapsed;
            }
        }

        private async static void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            await GetCurrentSoundPlayingStatusAsync(connectionstring);


            if (CurrentSoundPlayingStatus == playsound)
            {
                lock (obj)
                {
                    PlayLionRoarSound(true);
                }
            }
            else
            {
                lock (obj)
                {
                    PlayLionRoarSound(false);
                }
                tmr.Enabled = false;
                tmr.Elapsed -= Tmr_Elapsed;
            }

        }


        public static void PlayLionRoarSound(bool play)
        {
            if (soundPlayer == null)
            {

                soundPlayer = new SoundPlayer($"{System.Reflection.Assembly.GetEntryAssembly().Location.Replace("PtrotectFarmByWildAnimal.dll", "lionroar.wav")}");
            }
            if (play)
            {
                soundPlayer.PlayLooping();
                Console.WriteLine($"{DateTime.Now.ToString("dd-MM-yyy-HH-mm-ss")} Playing lion roar sound");

            }
            else if (play == false)
            {
                soundPlayer.Stop();
                Console.WriteLine($"{DateTime.Now.ToString("dd-MM-yyy-HH-mm-ss")} Stopped lion roar sound");

            }
        }


        public static async Task GetCurrentSoundPlayingStatusAsync(string connectionstring)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionstring);

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable _linkTable = tableClient.GetTableReference(TableName);

                ManorMonkeyDeatails manorMonkeyDeatails = new ManorMonkeyDeatails();

                TableQuery<ManorMonkeyDeatails> query = new TableQuery<ManorMonkeyDeatails>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, status));


                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<ManorMonkeyDeatails> resultSegment = await _linkTable.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);
                    token = resultSegment.ContinuationToken;

                    manorMonkeyDeatails = resultSegment.Results.FirstOrDefault();


                    CurrentSoundPlayingStatus = manorMonkeyDeatails.SoundPlayingStatus;


                } while (token != null);


            }
            catch (Exception exp)
            {
                Debug.Write(exp);

            }
        }


        public static async Task<bool> UploadFileToAzureStorageAsync(Stream filestream, string storageaccounturi, string connectionstring, string containername, string filename)
        {
            Uri bloburi = new Uri($"{storageaccounturi}/{containername}/{filename}");
            BlobClient blobClient = new BlobClient(connectionstring, containername, filename);
            await blobClient.UploadAsync(filestream);
            return await Task.FromResult(true);
        }

    }
}


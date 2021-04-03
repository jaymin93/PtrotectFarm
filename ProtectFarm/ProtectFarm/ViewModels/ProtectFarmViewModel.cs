using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Linq;
using System.Linq.Expressions;
using ProtectFarm.Models;

namespace ProtectFarm.ViewModels
{
    public class ProtectFarmViewModel : BaseViewModel
    {

        private string curretstatus;

        public string CurrentSoundPlayingStatus
        {
            get => curretstatus;
            set => SetProperty(ref curretstatus, value);
        }


        private DateTime incdt;

        public DateTime IncidentDateTime
        {
            get => incdt;
            set => SetProperty(ref incdt, value);
        }

        const string status = "status";

        private const string playsound = "playing";

        private const string stopsound = "stopped";

        public ProtectFarmViewModel()
        {
            Title = "Protect Farm";
            PlaySoundCommand = new Command(async () => await UpdateSoundPlayingStatustoAzureTable(playsound).ConfigureAwait(false));
            StopSoundCommand = new Command(async () => await UpdateSoundPlayingStatustoAzureTable(stopsound).ConfigureAwait(false));

            Task.Run(async () => await GetCurrentSoundPlayingStatusAsync().ConfigureAwait(false));
        }

        public ICommand PlaySoundCommand { get; }

        public ICommand StopSoundCommand { get; }



        public async Task GetCurrentSoundPlayingStatusAsync()
        {
            try
            {
                TableQuery<ManorMonkeyDeatails> query;


                query = new TableQuery<ManorMonkeyDeatails>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, status));


                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<ManorMonkeyDeatails> resultSegment = await _linkTable.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);
                    token = resultSegment.ContinuationToken;

                    manorMonkeyDeatails = resultSegment.Results.FirstOrDefault();


                    CurrentSoundPlayingStatus = manorMonkeyDeatails.SoundPlayingStatus;
                    IncidentDateTime = manorMonkeyDeatails.IncidentTime;

                } while (token != null);

            }
            catch (Exception exp)
            {
                Debug.Write(exp);

            }
        }


        public async Task UpdateSoundPlayingStatustoAzureTable(string command)
        {
            if (manorMonkeyDeatails == null)
            {
                await GetCurrentSoundPlayingStatusAsync().ConfigureAwait(false);
            }

            manorMonkeyDeatails.SoundPlayingStatus = command;

            manorMonkeyDeatails.IncidentTime = DateTime.Now;

            TableOperation updateoperation = TableOperation.Replace(manorMonkeyDeatails);

            var insertoperationresult = await _linkTable.ExecuteAsync(updateoperation);

            CurrentSoundPlayingStatus = command;

        }

    }
}
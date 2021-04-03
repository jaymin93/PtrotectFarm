using Microsoft.WindowsAzure.Storage.Table;
using ProtectFarm.Models;
using ProtectFarm.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProtectFarm.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {

        private string imgurl;

        public string ImgURL
        {
            get => imgurl;
            set => SetProperty(ref imgurl, value);
        }


        private string msg;

        public string MSG
        {
            get => msg;
            set => SetProperty(ref msg, value);
        }



        private DateTime     incdt;

        public DateTime Incident
        {
            get => incdt;
            set => SetProperty(ref incdt, value);
        }




        private ManorMonkeyDeatails _selectedItem;

        public ObservableCollection<ManorMonkeyDeatails> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command<ManorMonkeyDeatails> ItemTapped { get; }

        public ItemsViewModel()
        {
            Title = "History";
            Items = new ObservableCollection<ManorMonkeyDeatails>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<ManorMonkeyDeatails>(OnItemSelected);

            //AddItemCommand = new Command(OnAddItem);
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await GetHistoryAsync().ConfigureAwait(false);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public ManorMonkeyDeatails SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        

        async void OnItemSelected(ManorMonkeyDeatails item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.IMGURL)}={item.ImageURL}");
        }



        public async Task<List<ManorMonkeyDeatails>> GetHistoryAsync()
        {
            try
            {
                List<ManorMonkeyDeatails> manorMonkeyDeatailslist = new List<ManorMonkeyDeatails>();

                TableQuery<ManorMonkeyDeatails> query;


                query = new TableQuery<ManorMonkeyDeatails>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{TableName}"));


                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<ManorMonkeyDeatails> resultSegment = await _linkTable.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);
                    token = resultSegment.ContinuationToken;

                    foreach (var entity in resultSegment.Results)
                    {
                        ManorMonkeyDeatails details = new ManorMonkeyDeatails
                        {
                            IncidentTime = entity.IncidentTime,
                            ImageURL = entity.ImageURL,
                            Message = entity.Message

                        };

                        manorMonkeyDeatailslist.Add(details);
                    }
                } while (token != null);


                return manorMonkeyDeatailslist;

            }
            catch (Exception exp)
            {
                Debug.Write(exp);
                return default;
            }
        }
    }
}
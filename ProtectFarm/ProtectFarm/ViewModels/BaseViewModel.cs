using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ProtectFarm.Models;
using ProtectFarm.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace ProtectFarm.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        internal static string TableName = "helpfarmer";


        //please enter correct values from azure portal
        internal static CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=deafaid;AccountKey=ch8fbsZmxgSbFGfLw5lGNXgGlRUBN+Actts2M09bIUgomisMHySQv7xuiVDbj5k//BwpVF7V6TMGniOkhFn17Q==;EndpointSuffix=core.windows.net");

        internal static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        internal static CloudTable _linkTable = tableClient.GetTableReference(TableName);

        internal static ManorMonkeyDeatails manorMonkeyDeatails = new ManorMonkeyDeatails();

        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>();

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

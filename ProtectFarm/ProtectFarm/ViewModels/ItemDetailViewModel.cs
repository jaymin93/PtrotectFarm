using ProtectFarm.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProtectFarm.ViewModels
{
    [QueryProperty(nameof(IMGURL), nameof(IMGURL))]
    public class ItemDetailViewModel : BaseViewModel
    {
        
        private string imgurl;
        
        public string Id { get; set; }

        public string IMGURL
        {
            get => imgurl;
            set => SetProperty(ref imgurl, value);
        }

    }
}

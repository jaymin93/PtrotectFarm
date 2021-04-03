using ProtectFarm.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace ProtectFarm.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}
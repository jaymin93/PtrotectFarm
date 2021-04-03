using ProtectFarm.ViewModels;
using ProtectFarm.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ProtectFarm
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            
        }

    }
}

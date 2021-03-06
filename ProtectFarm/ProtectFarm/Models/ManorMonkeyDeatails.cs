using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace ProtectFarm.Models
{
    public class ManorMonkeyDeatails : TableEntity
    {
        public ManorMonkeyDeatails()
        {

        }
        public ManorMonkeyDeatails(string skey, string srow)
        {
            this.PartitionKey = skey;
            this.RowKey = srow;
        }
        public DateTime IncidentTime { get; set; }

        public string Message { get; set; }

        public string ImageURL { get; set; }

        public string SoundPlayingStatus { get; set; }

    }
}
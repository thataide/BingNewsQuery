using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingNewsQuery
{
    public class StockNews : TableEntity
    {
        public StockNews(String PartitionKey, String RowKey, String Category, String Description, String ImageURL, String Name, String ProviderName, String ProviderType, String URL)
        {
            this.RowKey = RowKey;
            this.PartitionKey = PartitionKey;
            this.Category = Category;
            this.Description = Description;
            this.ImageURL = ImageURL;
            this.Name = Name;
            this.ProviderName = ProviderName;
            this.ProviderType = ProviderType;
            this.URL = URL;
        }

        public StockNews() { }

        public String Category { get; set; }

        public String Description { get; set; }

        public String ImageURL { get; set; }

        public String Name { get; set; }

        public String ProviderName { get; set; }

        public String ProviderType { get; set; }

        public String URL { get; set; }
    }
}

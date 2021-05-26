using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace BingNewsQuery
{
    class Program
    {
        static void Main(string[] args) // Receives stock symbol and query (multiple words should be spaced with + sign) - Ex: MSFT Microsoft+stock+price
        {
            // BING
            // Add your Azure Bing Search V7 subscription key to your environment variables
            string BingAccessKey = ConfigurationManager.AppSettings["BingAccessKey"];
            // Add your Azure Bing Search V7 endpoint to your environment variables.
            string BingEndpoint = ConfigurationManager.AppSettings["BingEndpoint"];
            // Reads stock symbol and query from args - using a standard one here to make testing easier without args
            string stock = "MSFT";
            string query = "Microsoft+stock+price";
            try
            {
                stock = args[0].ToString();
                query = args[1].ToString();
            }
            catch(Exception) { }
            // Assemble and get Bing News API query
            var uriQuery = BingEndpoint + "?sortby=date&count=100&q=" + Uri.EscapeDataString(query);
            WebRequest request = HttpWebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = BingAccessKey;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
            dynamic parsedJson = JsonConvert.DeserializeObject(json);


            // STORAGE
            // Initiate table operators
            string storageConnection = ConfigurationManager.AppSettings["StorageConnection"];
            var account = CloudStorageAccount.Parse(storageConnection);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(ConfigurationManager.AppSettings["StorageTableName"].ToString());
            table.CreateIfNotExists();


            // INTEGRATION (SAVE BING ON STORAGE)
            int count = 0;
            foreach (var bingArticle in parsedJson.value)
            {
                // These are optional, so retrieves it before in case there isn't one
                string imageURL = "";
                try
                {
                    imageURL = Convert.ToString(bingArticle.image.thumbnail.contentUrl);
                }
                catch (Exception) { }
                string providerName = "";
                string providerType = "";
                try
                {
                    providerName = Convert.ToString(bingArticle.provider[0].name);
                    providerType = Convert.ToString(bingArticle.provider[0]._type);
                }
                catch (Exception) { }

                StockNews article = new StockNews(
                    stock,
                    Convert.ToString(bingArticle.datePublished).Replace("/", "-"),
                    Convert.ToString(bingArticle.category),
                    Convert.ToString(bingArticle.description),
                    imageURL,
                    Convert.ToString(bingArticle.name),
                    providerName,
                    providerType,
                    Convert.ToString(bingArticle.url)
                );
                TableOperation insertOperation = TableOperation.Insert(article);
                try
                {
                    table.Execute(insertOperation);
                    count++;
                }
                catch (Exception) { } // Ignores conflicts, as they come from the same news article published in multiple providers or same article showing up on another's day query
            }

            Console.WriteLine("Storage Table was populated with " + count.ToString() + " Bing News results for " + stock + " stock with query " + query);
        }
    }
}


/*
// HOW TO READ NEWS FROM STORAGE TABLE PROGRAMATICALLY
string stock = "MSFT";
string storageConnection = ConfigurationManager.AppSettings["StorageConnection"];
var account = CloudStorageAccount.Parse(storageConnection);
var client = account.CreateCloudTableClient();
var table = client.GetTableReference(ConfigurationManager.AppSettings["StorageTableName"].ToString());
string condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, stock);
var query = new TableQuery<StockNews>().Where(condition).OrderBy("RowKey");
var lst = table.ExecuteQuery(query);

//OPTION 1 - RUN THROUGH EACH ARTICLE IN lst
foreach (StockNews article in lst)
{
    // Now use article.Name... article.URL...
}

//OPTION 2 - SEND lst TO PAGE
*/
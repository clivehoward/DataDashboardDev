using DataDashboardWebLib.Interfaces;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DataDashboardWebLib
{
    public class DataAccess : IDataAccess
    {
        private string ApiBaseUri;
        private string BearerToken;

        public DataAccess(string apiBaseUri)
        {
            ApiBaseUri = apiBaseUri;
        }

        public DataAccess(string apiBaseUri, string bearerToken)
        {
            ApiBaseUri = apiBaseUri;
            BearerToken = bearerToken;
        }

        public async Task<ChartData> GetChartData(int questionId, string location = "0", string companySize = "0", string industry = "0")
        {
            // Get ChartData object from API
            ChartData chartData = new ChartData();

            // Call the API to get a ChartData object
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUri)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            // Build the URI, adding querystring params as applicable
            string uri = BuildUri(questionId, location, companySize, industry);

            HttpResponseMessage response = await client.GetAsync(uri);

            // If we have a successful response then get the return object
            // Note: ReadAsAsync requires NuGet package Microsoft.AspNet.WebApi.Client !!
            if (response.IsSuccessStatusCode)
            {
                chartData = await response.Content.ReadAsAsync<ChartData>();
            }

            return chartData;
        }

        public async Task<int> GetCoverage(string location = "0", string companySize = "0", string industry = "0")
        {
            int coverage = 0;

            // Call the API to get data coverage given filters
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUri)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

            // Build the URI, adding querystring params as applicable
            string uri = "coverage" + BuildUri(0, location, companySize, industry);

            HttpResponseMessage response = await client.GetAsync(uri);

            // If we have a successful response then get the return object
            if (response.IsSuccessStatusCode)
            {
                var results = await response.Content.ReadAsAsync<dynamic>();
                if (results != null)
                {
                    // The result actually gives us each question with answers 
                    // plus the total number of entries for each question
                    // based on the current filters 
                    // But, all we are going to return at this time is the number of questions 
                    // as a percentage of the possible total that we know to be 54
                    int total = results.Count;
                    coverage = (int)Math.Round((100M / 54M) * total);
                }
            }

            return coverage;
        }

        public async Task<ChartJS> GetChartJSData(int questionId, string dataSetName = "Default", string location = "0", string companySize = "0", string industry = "0")
        {
            // Get ChartData object from the API and then convert it to be used by Chart.js in the website
            ChartData chartData = await GetChartData(questionId, location, companySize, industry);

            // Now return this data as ChartJS object
            return CreateChartJSData(chartData, questionId, dataSetName);
        }

        public ChartJS GetChartJSData(ChartData chartData, int questionId, string dataSetName = "Default")
        {
            // Now return this ChartData as ChartJS object
            return CreateChartJSData(chartData, questionId, dataSetName);
        }

        private ChartJS CreateChartJSData(ChartData chartData, int questionId, string dataSetName)
        {
            // Lookup the index of the dataset...
            // Use the index to get a color
            int dataSetIndex = 0;
            if (chartData.Results.Count == 1)
            {
                dataSetIndex = chartData.Labels.Count;
            }
            else
            {
                foreach (var dataset in chartData.Results)
                {
                    if (dataset.Name == dataSetName) break;
                    dataSetIndex++;
                }
            }

            // Convert this data to an object that can be used by the Chart.js code
            ChartJS chartJS = new ChartJS
            {
                QuestionId = questionId,
                DataSetName = dataSetName,
                Labels = GetLabels(chartData), // Get the labels
                DataValues = GetDataValues(chartData, dataSetName), // Get the actual values for the dataset specified
                Colors = GetColors(chartData), // Get the list of colors to use in the chart
                SingleColor = ChartColors.Colors[dataSetIndex] //Colors[dataSetIndex]
            };

            return chartJS;
        }

        private static string GetColors(ChartData chartData)
        {
            // Get a list of colors based on the number of data points
            string dataColors = string.Empty;
            foreach (var color in ChartColors.Colors.Take(chartData.Labels.Count()))
            {
                dataColors = dataColors + "'" + color + "',";
            }
            if (dataColors.Length > 0)
            {
                dataColors = dataColors.Remove(dataColors.Length - 1, 1);
            }

            return dataColors;
        }

        private static string GetDataValues(ChartData chartData, string dataSetName = "Default")
        {
            // Get the data for this dataset as comma separated list
            string dataValues = string.Empty;
            var x = chartData.Results.Where(r => r.Name == dataSetName).Select(r => r.Values).SelectMany(r => r.Select(m => m.Value));

            foreach (var value in x)
            {
                dataValues = dataValues + value.ToString() + ",";
            }
            if (dataValues.Length > 0)
            {
                dataValues = dataValues.Remove(dataValues.Length - 1, 1);
            }

            return dataValues;
        }

        private static string GetLabels(ChartData chartData)
        {
            // Build a comma separated list of labels, each wrapped in quotes
            string labels = string.Empty;
            foreach (var label in chartData.Labels)
            {
                labels = labels + "\"" + label + "\",";
            }
            if (labels.Length > 0)
            {
                labels = labels.Remove(labels.Length - 1, 1);
            }

            return labels;
        }

        private static string BuildUri(int questionId, string location, string companySize, string industry)
        {
            // Build the URI with any appropriate querystring parameters
            bool isQ = true;
            string uri = string.Empty;

            if (questionId > 0)
            {
                uri = "question/" + questionId.ToString();
            }

            if (location != "0" && location != string.Empty) // Location
            {
                if (isQ)
                {
                    uri = uri + "?";
                    isQ = false;
                }
                uri = uri + "Location=" + location;
            }

            if (companySize != "0" && companySize != string.Empty) // CompanySize
            {
                if (isQ)
                {
                    uri = uri + "?";
                    isQ = false;
                }
                else
                {
                    uri = uri + "&";
                }
                uri = uri + "CompanySize=" + companySize;
            }

            if (industry != "0" && industry != string.Empty) // Industry
            {
                if (isQ)
                {
                    uri = uri + "?";
                    isQ = false;
                }
                else
                {
                    uri = uri + "&";
                }
                uri = uri + "Industry=" + industry;
            }

            return uri;
        }
    }
}

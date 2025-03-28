using System.Collections.Generic;

namespace DataDashboardWebLib
{
    public class ChartData
    {
        // Class structure...
        // A ChartData object holds the percentage of respondents plus a list of labels
        // It then includes a list of one or more result sets (Results)
        // Each results set has a name (based on table column name) and then 
        // a list of results (Result) that include each label along with its total for that set

        public int TotalResults { get; set; }
        public List<string> Labels { get; set; }
        public List<Results> Results { get; set; }
    }
}

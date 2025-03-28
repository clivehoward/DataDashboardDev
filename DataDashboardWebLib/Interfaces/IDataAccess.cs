using System.Threading.Tasks;

namespace DataDashboardWebLib.Interfaces
{
    public interface IDataAccess
    {
        Task<ChartData> GetChartData(int questionId, string location = "0", string companySize = "0", string industry = "0");
        Task<int> GetCoverage(string location = "0", string companySize = "0", string industry = "0");
        Task<ChartJS> GetChartJSData(int questionId, string dataSetName = "Default", string location = "0", string companySize = "0", string industry = "0");
        ChartJS GetChartJSData(ChartData chartData, int questionId, string dataSetName = "Default");
    }
}

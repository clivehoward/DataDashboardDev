using System.Collections.Generic;

namespace DataDashboardInsightLib.Interfaces
{
    public interface IDataAccess
    {
        ChartData GetChartDataForQuestion(int questionId, string locationId = "0", string companySizeId = "0", string industryId = "0");
        List<CoverageResult> GetDataCoverage(string locationId = "0", string companySizeId = "0", string industryId = "0");
    }
}

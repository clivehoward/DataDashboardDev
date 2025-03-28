using System.Collections.Generic;

namespace DataDashboardWebLib
{
    public class Results
    {
        public string Name { get; set; }
        public List<Result> Values { get; set; }
        public int TotalResults { get; set; }
    }
}

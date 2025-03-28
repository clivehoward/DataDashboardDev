using DataDashboardInsightLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;

namespace DataDashboardInsightLib
{
    public class DataAccess : IDataAccess
    {
        private readonly string _dbConnString;

        public DataAccess(string dbConnectionString)
        {
            // Constructor takes a database connection string
            _dbConnString = dbConnectionString;
        }

        public ChartData GetChartDataForQuestion(int questionId, string locationId = "0", string companySizeId = "0", string industryId = "0")
        {
            // NOTE /////////////////////////////////////////////////////////////////////////////////////////////////////
            // Labels are ordered according to the first - default - resultset.
            // However, where there are multiple resultset the first - default - may not be shown.
            // Instead, the others are shown.
            // Result is that the chart may show labels in different order to the data table
            // Can't use labels from other resultsets because only the default can be trusted to have ALL possible labels
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////

            PrepareFilters(ref locationId, ref companySizeId, ref industryId);

            int totalResults = 0;
            List<string> labels = new List<string>();
            List<Results> allResults = new List<Results>();

            // Get all of the resultsets for this Question - there maybe more than just one
            // For example, if a Question has multiple choices against each option such as important, not important, crucial etc
            List<List<DataResult>> allDataResults = GetAllResultLists(questionId, locationId, companySizeId, industryId);

            // Now turn these resultsets into what we need to send back to the client
            if (allDataResults.Count > 0)
            {
                // Of all those who could have answered this Question in its entirety, how many did?
                totalResults = allDataResults.First().First().TotalResponses;

                // Get the list of labels based on the order of the first resultset
                labels = allDataResults.First().Select(r => r.OptionText).ToList();

                // Get all of the resultsets in the appropriate structure
                foreach (var resultSet in allDataResults)
                {
                    Results results = new Results
                    {
                        Name = resultSet.First().ResultName,
                        TotalResults = resultSet.First().TotalResponses,
                        Values = new List<Result>()
                    };

                    // Get each resultset in order of the default sets labels
                    foreach (var label in labels)
                    {
                        var newresult = resultSet.Where(l => l.OptionText == label).FirstOrDefault();
                        if (newresult != null) // If there is no data for this option then return it as 0
                        {
                            results.Values.Add(new Result() { Label = newresult.OptionText, Value = newresult.OptionPercent });
                        }
                        else
                        {
                            results.Values.Add(new Result() { Label = label, Value = 0 });
                        }
                    }

                    allResults.Add(results);
                }
            }

            ChartData chartData = new ChartData()
            {
                TotalResults = totalResults,
                Labels = labels,
                Results = allResults
            };

            return chartData;
        }

        public List<CoverageResult> GetDataCoverage(string locationId = "0", string companySizeId = "0", string industryId = "0")
        {
            // Want to know how much data is covered given that there may be filters applied
            // Want to know:
            //  - Which questions have answers
            //  - How many entries for each question with answers
            List<CoverageResult> results = new List<CoverageResult>();

            string sql = "";
            string query = "";
            string whereClause = "";

            bool isUnion = false;

            if (locationId != "0" || companySizeId != "0" || industryId != "0")
            {
                whereClause = BuildWhereClause(locationId, companySizeId, industryId);
            }

            // Iterate all of the questions and build a SQL string that will tell us
            for (int i = 1; i <= 55; i++)
            {
                // We do not have a question 10
                if (i != 10)
                {
                    query = "SELECT AnswerOptions.QuestionId, COUNT(DISTINCT Entries.EntryId) AS Total ";
                    query = query + " FROM Entries ";
                    query = query + string.Format(" INNER JOIN Question{0} ON Entries.EntryId = Question{0}.EntryId ", i.ToString());
                    query = query + string.Format(" INNER JOIN AnswerOptions ON Question{0}.OptionId = AnswerOptions.OptionId ", i.ToString());
                    if (whereClause != "")
                    {
                        query = query + "WHERE " + whereClause;
                    }
                    query = query + " GROUP BY AnswerOptions.QuestionId ";

                    // UNION the queries
                    if (isUnion)
                    {
                        sql = sql + " UNION " + query;
                    }
                    else
                    {
                        sql = sql + query;
                        isUnion = true;
                    }
                }
            }

            // Now execute the SQL
            using (DataContext context = new DataContext(_dbConnString))
            {
                results = context.ExecuteQuery<CoverageResult>(sql).ToList();
            }

            return results;
        }

        private static void PrepareFilters(ref string locationId, ref string companySizeId, ref string industryId)
        {
            // Locations requires some work
            string[] ids = locationId.Split(',');

            foreach (string id in ids)
            {
                if (id == "1") locationId = locationId + ",2,13,18,25,21,7"; // Asia
                if (id == "3") locationId = locationId + ",6,7,14,15,16,17,19,20,23,24,26,27,28,29"; // Europe
                if (id == "4") locationId = locationId + ",12,30"; // North America
                if (id == "5") locationId = locationId + ",11"; // South America
                //if (id == "6") locationId = locationId + ",24"; // Eastern Europe
            }

            // Remove any duplicates
            ids = locationId.Split(',');
            ids = ids.Distinct().ToArray();

            locationId = string.Join(",", ids);

            if (locationId == "0") locationId = string.Empty;
            if (companySizeId == "0") companySizeId = string.Empty;
            if (industryId == "0") industryId = string.Empty;
        }

        private List<List<DataResult>> GetAllResultLists(int questionId, string location, string companySize, string industry)
        {
            // Run Stored Procedure to return ALL resultsets for this question and any filters
            List<List<DataResult>> allDataResults = new List<List<DataResult>>();

            using (SqlConnection connection = new SqlConnection(_dbConnString))
            {
                connection.Open();

                SqlCommand cmd = new SqlCommand("usp_GetChartDataForQuestion", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@QuestionID", SqlDbType.Int).Value = questionId;
                cmd.Parameters.Add("@Location", SqlDbType.VarChar).Value = location;
                cmd.Parameters.Add("@CompanySize", SqlDbType.VarChar).Value = companySize;
                cmd.Parameters.Add("@Industry", SqlDbType.VarChar).Value = industry;

                SqlDataReader reader = cmd.ExecuteReader();

                allDataResults.Add(GetDataResults(reader));

                while (reader.NextResult())
                {
                    allDataResults.Add(GetDataResults(reader));
                }
            }

            return allDataResults;
        }

        private List<DataResult> GetDataResults(SqlDataReader reader)
        {
            // Iterate the reader and get all of the results to assign to a new List
            List<DataResult> dataResults = new List<DataResult>();

            dataResults.Clear();

            while (reader.Read())
            {
                var optionPercent = reader["OptionPercent"];
                var optionText = reader["OptionText"];
                var totalResponses = reader["TotalResponses"];
                var resultName = reader["ResultName"];

                DataResult dataResult = new DataResult()
                {
                    OptionPercent = Int32.Parse(optionPercent.ToString()),
                    OptionText = optionText.ToString(),
                    TotalResponses = Int32.Parse(totalResponses.ToString()),
                    ResultName = resultName.ToString()
                };

                dataResults.Add(dataResult);
            }

            return dataResults;
        }

        private string BuildWhereClause(string locationId, string companySizeId, string industryId)
        {
            // If there are optional filters then build a WHERE clause to use in all SQL queries
            // Each of these is passed a comma separated string ("1,2,3")
            string whereClause = "";
            bool isAnd = false;

            if (companySizeId != "0") // Company Size
            {
                whereClause = whereClause + $" (Entries.CompanySizeId IN ({companySizeId})) ";
                isAnd = true;
            }

            if (industryId != "0") // Industry
            {
                if (isAnd)
                {
                    whereClause = whereClause + $" AND ";
                }
                whereClause = whereClause + $" (Entries.IndustryId IN ({industryId})) ";
                isAnd = true;
            }

            if (locationId != "0") // Location
            {
                // Location is challenging because a location may need to include others
                if (isAnd)
                {
                    whereClause = whereClause + $" AND ";
                }

                string[] ids;
                ids = locationId.Split(',');

                foreach (string id in ids)
                {
                    if (id == "1") locationId = locationId + ",2,13,18,25,21,7"; // Asia
                    if (id == "3") locationId = locationId + ",6,7,14,15,16,17,19,20,23,24,26,27,28,29"; // Europe
                    if (id == "4") locationId = locationId + ",12,30"; // North America
                    if (id == "5") locationId = locationId + ",11"; // South America
                    //if (id == "6") locationId = locationId + ",24"; // Eastern Europe
                }

                // Remove any duplicates
                ids = locationId.Split(',');
                ids = ids.Distinct().ToArray();

                locationId = string.Join(",", ids);

                whereClause = whereClause + $" (Entries.LocationId IN ({locationId})) ";
                isAnd = true;
            }

            return whereClause;
        }
    }

    public class DataResult
    {
        public int OptionPercent { get; set; }
        public string OptionText { get; set; }
        public int TotalResponses { get; set; }
        public string ResultName { get; set; }
    }
}

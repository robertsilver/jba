using jba.Models;
using JBAUtils;
using System.Collections.Generic;
using System.Data;

namespace jba.Services
{
    public class JBAStore : IJBAStore
    {
        DataAccessService _dbContext = new DataAccessService();

        public JBAStore()
        {
            _dbContext.ExecStoredProcedure<PrecipitationData>("CreateTablesIfNecessary", new { });
        }

        public void StoreData(List<PrecipitationData> data)
        {
            DataTable rainDt = _dbContext.CreateAndPopulateTableForList(data, new
            {
                Xref = "Xref",
                Yref = "Yref",
                Date = "Date",
                Value = "Value"
            });

            _dbContext.ExecStoredProcedure<PrecipitationData>("SavePrecipitationData", new { rainData = rainDt });

        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace JBAUtils
{
    public class DataAccessService
    {
        public DataAccessService()
        {
            _dataConversionService = new DataConversionService();
            _configurationManager = new ConfigurationManager();
        }

        protected DataConversionService _dataConversionService { get; }

        protected ConfigurationManager _configurationManager { get; }

        public IEnumerable<T> ExecStoredProcedure<T>(string storedProcName, dynamic parameters, int intTimeOut = 30)
        {
            DataTable dataTable = new DataTable();
            bool success = true;

            using (SqlConnection connection = new SqlConnection(_configurationManager.GetValue<string>("dbConnectionString")))
            using (SqlCommand sqlCommand = new SqlCommand(storedProcName, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = intTimeOut })
            {
                try
                {
                    foreach (SqlParameter parameter in ParseParameters(parameters))
                    {
                        sqlCommand.Parameters.Add(parameter);
                    }

                    sqlCommand.Connection.Open();

                    using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                    {
                        if (dataReader.HasRows)
                        {
                            dataTable.Load(dataReader);
                        }
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                }
            }

            return success ? _dataConversionService.DataTableToEnumerable<T>(dataTable) : new List<T>();
        }

        private IEnumerable<SqlParameter> ParseParameters(dynamic parameters)
        {
            foreach (PropertyInfo property in parameters.GetType().GetProperties())
            {
                string parameterName = "@" + property.Name as string;
                object paramValue = property.GetValue(parameters, null);

                if (paramValue != null)
                {
                    if (paramValue is string)
                    {
                        string stringParam = paramValue as string;

                        if (string.IsNullOrWhiteSpace(stringParam))
                        {
                            yield return new SqlParameter(parameterName, DBNull.Value);
                        }
                        else
                        {
                            yield return new SqlParameter(parameterName, stringParam);
                        }
                    }
                    else if (paramValue.GetType().IsEnum)
                    {
                        yield return new SqlParameter(parameterName, (int)paramValue);
                    }
                    else
                    {
                        yield return new SqlParameter(parameterName, paramValue);
                    }
                }
                else
                {
                    yield return new SqlParameter(parameterName, DBNull.Value);
                }
            }
        }
    }
}

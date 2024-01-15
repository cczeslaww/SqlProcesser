using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SqlProcesser
{
    public static class Db
    {
        public static void ExecuteNonQueryProcedure(string procName, SqlParameter[] parameters)
        {
            using (SqlConnection sql = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                sql.Open();
                SqlCommand sp = sql.CreateCommand();
                sp.CommandText = string.Concat(ConfigurationProvider.DbPrefix, procName);
                sp.CommandType = CommandType.StoredProcedure;
                sp.Parameters.AddRange(parameters);
                WriteCall(procName, parameters);
                sp.ExecuteNonQuery();
            }
        }

        public static DataTable ExecuteProcedure(string procName, SqlParameter[] parameters)
        {
            using (SqlConnection sql = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                sql.Open();
                SqlCommand sp = sql.CreateCommand();
                sp.CommandText = string.Concat(ConfigurationProvider.DbPrefix, procName);
                sp.CommandType = CommandType.StoredProcedure;
                sp.Parameters.AddRange(parameters);
                WriteCall(procName, parameters);
                DataTable result = new DataTable();
                using (SqlDataReader sdr = sp.ExecuteReader())
                {
                    result.Load(sdr);
                }
                return result;
            }
        }

        public static SqlParameterCollection ExecuteProcedureWithOutParameters(string procName, SqlParameter[] inputParameters, SqlParameter[] outputParameters)
        {
            using (SqlConnection sql = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                sql.Open();
                SqlCommand sp = sql.CreateCommand();
                sp.CommandText = string.Concat(ConfigurationProvider.DbPrefix, procName);
                sp.CommandType = CommandType.StoredProcedure;
                sp.Parameters.AddRange(inputParameters);
                WriteCall(procName, inputParameters);
                for (int i = 0; i < outputParameters.Length; i++)
                {
                    outputParameters[i].Direction = ParameterDirection.Output;
                }
                sp.Parameters.AddRange(outputParameters);
                sp.ExecuteNonQuery();
                WriteCall(string.Concat("OUT: ", procName), outputParameters);
                return sp.Parameters;
            }
        }

        public static KeyValuePair<DataTable, SqlParameterCollection> ExecuteProcedureWithResultAndOutParameters(string procName, SqlParameter[] inputParameters, SqlParameter[] outputParameters)
        {
            using (SqlConnection sql = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                sql.Open();
                SqlCommand sp = sql.CreateCommand();
                sp.CommandText = string.Concat(ConfigurationProvider.DbPrefix, procName);
                sp.CommandType = CommandType.StoredProcedure;
                sp.Parameters.AddRange(inputParameters);
                //WriteCall(procName, inputParameters);
                for (int i = 0; i < outputParameters.Length; i++)
                {
                    outputParameters[i].Direction = ParameterDirection.Output;
                }
                sp.Parameters.AddRange(outputParameters);
                DataTable result = new DataTable();
                using (SqlDataReader sdr = sp.ExecuteReader())
                {
                    result.Load(sdr);
                }
                WriteCall($"OUT: {procName}", outputParameters);
                return new KeyValuePair<DataTable, SqlParameterCollection>(result, sp.Parameters);
            }
        }

        public static object ExecuteScalarFunction(string functionName, SqlParameter[] parameters)
        {
            int j = parameters.Length - 1;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < j; i++)
            {
                sb.Append(string.Concat(parameters[i].ParameterName, ", "));
            }

            using (SqlConnection sql = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                sql.Open();
                SqlCommand sc = sql.CreateCommand();
                sc.CommandText = string.Concat("SELECT dbo.", ConfigurationProvider.DbPrefix, functionName, " (", sb, parameters[j].ParameterName, ")");
                sc.CommandType = CommandType.Text;
                sc.Parameters.AddRange(parameters);
                if (sql.State != ConnectionState.Open)
                {
                    sql.Open();
                }
                WriteCall(functionName, parameters);
                return sc.ExecuteScalar();
            }
        }

        public static object ExecuteScalarFunction(string functionName)
        {
            using (SqlConnection sql = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                sql.Open();
                SqlCommand sc = sql.CreateCommand();
                sc.CommandText = string.Concat("SELECT dbo.", ConfigurationProvider.DbPrefix, functionName, "()");
                sc.CommandType = CommandType.Text;
                if (sql.State != ConnectionState.Open)
                {
                    sql.Open();
                }
                return sc.ExecuteScalar();
            }
        }

        public static DataView ExecuteProcedureToDataView(string procName, SqlParameter[] parameters)
        {
            using (SqlConnection sql = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                sql.Open();
                SqlCommand sp = sql.CreateCommand();
                sp.CommandText = string.Concat(ConfigurationProvider.DbPrefix, procName);
                sp.CommandType = CommandType.StoredProcedure;
                sp.Parameters.AddRange(parameters);
                WriteCall(procName, parameters);
                using (SqlDataReader sdr = sp.ExecuteReader())
                {
                    DataTable dataReaderTable = new DataTable("Table");
                    try
                    {
                        for (int count = 0; count < sdr.FieldCount; count++)
                        {
                            Type columnType = sdr.GetFieldType(count);
                            if (columnType != null)
                            {
                                dataReaderTable.Columns.Add(new DataColumn(sdr.GetName(count), columnType));
                            }
                        }
                        while (sdr.Read())
                        {
                            DataRow dr = dataReaderTable.NewRow();
                            for (int i = 0; i < sdr.FieldCount; i++)
                            {
                                dr[i] = sdr.GetValue(sdr.GetOrdinal(sdr.GetName(i)));
                            }
                            dataReaderTable.Rows.Add(dr);
                        }
                        return dataReaderTable.DefaultView;
                    }
                    catch
                    {
                        return null;
                    }
                }
                //using (SqlDataAdapter sda = new SqlDataAdapter(sp))
                //{
                //    DataSet ds = new DataSet();
                //    sda.Fill(ds);
                //    return ds.Tables.Count > 0 ? ds.Tables[0].DefaultView : null;
                //}
            }
        }

        public static DataView ExecuteProcedureToDataViewWithOutParameters(string procName, SqlParameter[] inputParameters, SqlParameter[] outputParameters)
        {
            using (SqlConnection sql = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                sql.Open();
                SqlCommand sp = sql.CreateCommand();
                sp.CommandText = string.Concat(ConfigurationProvider.DbPrefix, procName);
                sp.CommandType = CommandType.StoredProcedure;
                sp.Parameters.AddRange(inputParameters);
                WriteCall(procName, inputParameters);
                for (int i = 0; i < outputParameters.Length; i++)
                {
                    outputParameters[i].Direction = ParameterDirection.Output;
                }
                sp.Parameters.AddRange(outputParameters);
                DataTable result = new DataTable();
                using (SqlDataReader sdr = sp.ExecuteReader())
                {
                    result.Load(sdr);
                }
                WriteCall($"OUT: {procName}", outputParameters);
                using (SqlDataReader sdr = sp.ExecuteReader())
                {
                    DataTable dataReaderTable = new DataTable("Table");
                    try
                    {
                        for (int count = 0; count < sdr.FieldCount; count++)
                        {
                            Type columnType = sdr.GetFieldType(count);
                            if (columnType != null)
                            {
                                dataReaderTable.Columns.Add(new DataColumn(sdr.GetName(count), columnType));
                            }
                        }
                        while (sdr.Read())
                        {
                            DataRow dr = dataReaderTable.NewRow();
                            for (int i = 0; i < sdr.FieldCount; i++)
                            {
                                dr[i] = sdr.GetValue(sdr.GetOrdinal(sdr.GetName(i)));
                            }
                            dataReaderTable.Rows.Add(dr);
                        }
                        return dataReaderTable.DefaultView;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        public static DataSet ExecuteProcedureAndReceiveDataSet(string procName, SqlParameter[] inputParameters)
        {
            using (SqlConnection sql = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                sql.Open();
                SqlCommand sp = sql.CreateCommand();
                sp.CommandText = string.Concat(ConfigurationProvider.DbPrefix, procName);
                sp.CommandType = CommandType.StoredProcedure;
                sp.Parameters.AddRange(inputParameters);
                //WriteCall(procName, inputParameters);
                DataSet result = new DataSet();
                using (SqlDataAdapter sda = new SqlDataAdapter(sp))
                {
                    sda.Fill(result);
                }
                return result;
            }
        }

        public static KeyValuePair<DataSet, SqlParameterCollection> ExecuteProcedureWithOutParametersAndReceiveDataSet(string procName, SqlParameter[] inputParameters, SqlParameter[] outputParameters)
        {
            using (SqlConnection sql = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                sql.Open();
                SqlCommand sp = sql.CreateCommand();
                sp.CommandText = string.Concat(ConfigurationProvider.DbPrefix, procName);
                sp.CommandType = CommandType.StoredProcedure;
                sp.Parameters.AddRange(inputParameters);
                for (int i = 0; i < outputParameters.Length; i++)
                {
                    outputParameters[i].Direction = ParameterDirection.Output;
                }
                sp.Parameters.AddRange(outputParameters);

                //WriteCall(procName, inputParameters);
                DataSet result = new DataSet();
                using (SqlDataAdapter sda = new SqlDataAdapter(sp))
                {
                    sda.Fill(result);
                }
                return new KeyValuePair<DataSet, SqlParameterCollection>(result, sp.Parameters);
            }
        }

        private static void WriteCall(string objectName, IEnumerable<SqlParameter> parameters)
        {
            //if (ConfigurationProvider.WriteProcLog)
            //{
            //    using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "procLog.txt"), FileMode.Append)))
            //    {
            //        sw.WriteLine(objectName);
            //        foreach (SqlParameter t in parameters)
            //        {
            //            sw.WriteLine(string.Concat(t.ParameterName, " ", t.SqlDbType, " ", t.Value));
            //        }
            //        sw.WriteLine();
            //    }
            //}
        }
    }
}
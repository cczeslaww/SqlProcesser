using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlProcesser
{
    public static class DbProvider
    {
        public static DataRowCollection FetchProcedures(List<string> objectsToSave)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Email", typeof(string));
            foreach (string o in objectsToSave)
            {
                dt.Rows.Add(new object[] { o });
            }
            return Db.ExecuteProcedure("spDBA_GenerateScript", new[]
            {
                new SqlParameter("@ObjectsToSave", SqlDbType.Structured) { TypeName = "dbo.ReceiverType", Value = dt }
            }).Rows;
        }
    }
}

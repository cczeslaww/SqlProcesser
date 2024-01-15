using System.Configuration;

namespace SqlProcesser
{
    public static class ConfigurationProvider
    {
        /* Connection with appropriate database */
        public static string ConnectionString => ConfigurationManager.ConnectionStrings["SQLPROCESSER"].ConnectionString;
        public static string DbPrefix => ConfigurationManager.AppSettings["DbPrefix"];

        /* Application settings */
        public static string FileName => ConfigurationManager.AppSettings["InputFileName"];
        public static bool SearchWord => int.Parse(ConfigurationManager.AppSettings["SearchWord"]) == 1;
        public static string Needle => ConfigurationManager.AppSettings["Needle"];
        public static bool AlterOption => int.Parse(ConfigurationManager.AppSettings["AlterOption"]) == 1;
    }
}
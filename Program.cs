using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlProcesser
{
    public class Program
    {
        private static List<string> _objectsToScript = new List<string>(); //TODO: scripting tables
        private static bool _generateContents;// = !ConfigurationProvider.SearchWord || string.IsNullOrEmpty(ConfigurationProvider.Needle);
        private static string[] _keyWords = new string[] { "CREATE PROCEDURE ", "ALTER PROCEDURE ", "CREATE FUNCTION ", "ALTER FUNCTION ", "CREATE TABLE ", "ALTER TABLE ", "CREATE TYPE ", "ALTER TYPE " };
        private static string[] _excludedKeyWords = new string[] { "DEFAULT", "CONSTRAINT" };

        public static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
#if DEBUG
            //GenerateQuery();
            InputProcessing(ref sb, ConfigurationProvider.FileName, ConfigurationProvider.SearchWord, ConfigurationProvider.Needle);
            OutputProcessing(ref sb, ConfigurationProvider.FileName, ConfigurationProvider.SearchWord, ConfigurationProvider.Needle);
#else
            // args[0]: command ("list" or "search")
            // args[1] is the fileName
            // if args[0] == "search", args[2] is the needle to search; otherwise args[2] does not exist
            bool searchWord = false;
            string needle = null;
            switch (args[0])
            {
                case "list":
                    break;
                case "search":
                    searchWord = true;
                    needle = args[2];
                    break;
                default:
                    throw new ArgumentException("List of commands handled: 'list', 'search'. Please provide the correct command name as a first parameter.");
            }
            _generateContents = !searchWord || string.IsNullOrEmpty(needle);
            InputProcessing(ref sb, args[1], searchWord, needle);
            OutputProcessing(ref sb, args[1], searchWord, needle);
#endif
        }

        private static void GenerateQuery()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 4; i <= 20; i++)
            {
                for (int j = 0; j <= i - 4; j++)
                {
                    sb.Append(", CAST(EC1.ExchangeRate AS DECIMAL(");
                    sb.Append(i);
                    sb.Append(",");
                    sb.Append(j);
                    sb.Append(")) / CAST(EC2.ExchangeRate AS DECIMAL(");
                    sb.Append(i);
                    sb.Append(",");
                    sb.Append(j);
                    sb.Append(")) * P.Price AS D");
                    sb.Append(i);
                    sb.Append(j);
                }
            }

            Console.WriteLine(sb.ToString());
            Console.ReadKey();
        }

        private static void InputProcessing(ref StringBuilder sb, string fileName, bool searchWord, string needle)
        {
            //StringBuilder sb = new StringBuilder();

            using (StreamReader sr = new StreamReader(new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), FileMode.Open)))
            {
                try
                {
                    string line;
                    string previousObjectName = "";
                    bool saveNeeded = false;
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();

                        foreach (string keyWord in _keyWords)
                        {
                            if (line.Contains(keyWord))
                            {
                                SaveIfNeeded(previousObjectName, ref saveNeeded, ref sb);
                                previousObjectName = line.Replace(keyWord, "").Replace("[dbo].", "").Replace("[", "").Replace("]", "").Trim();
                                int bracketIndex = previousObjectName.IndexOf("(");
                                if (bracketIndex != -1)
                                {
                                    previousObjectName = previousObjectName.Substring(0, bracketIndex);
                                }
                            }
                            if (!searchWord || string.IsNullOrEmpty(needle) || line.Contains(needle))
                            {
                                saveNeeded = true;
                            }
                        }
                    }
                    SaveIfNeeded(previousObjectName, ref saveNeeded, ref sb);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private static void OutputProcessing(ref StringBuilder sb, string fileName, bool searchWord, string needle)
        {
            if (!_generateContents)
            {
                foreach (DataRow dr in DbProvider.FetchProcedures(_objectsToScript))
                {
                    string definition = new Regex(@"([A-Za-z]+) \[dbo\].\[([A-Za-z0-9_]+)\]").Replace((string)dr["Definition"], @"$1 dbo.$2");
                    if (ConfigurationProvider.AlterOption)
                    {
                        for (int i = 0; i < _keyWords.Length; i += 2)
                        {
                            definition = definition.Replace(_keyWords[i], _keyWords[i + 1]);
                        }
                    }
                    sb.AppendLine(definition);
                }
            }
            string fullFileName = string.Concat(fileName.Replace(".sql", ""), "_", _generateContents ? "l.txt" : string.Concat("o_", needle, "_s.sql"));
            WriteToFile(fullFileName, sb);
        }

        private static void SaveIfNeeded(string objectToSave, ref bool saveNeeded, ref StringBuilder sb)
        {
            if (saveNeeded && !string.IsNullOrEmpty(objectToSave))
            {
                foreach (string excludedKeyWord in _excludedKeyWords)
                {
                    if (objectToSave.Contains(excludedKeyWord))
                    {
                        saveNeeded = false;
                    }
                }
                if (saveNeeded)
                {
                    sb.AppendLine(objectToSave);
                    saveNeeded = false;
                    if (!_generateContents)
                    {
                        _objectsToScript.Add(objectToSave);
                    } 
                }
            }
        }

        private static void WriteToFile(string fullFileName, StringBuilder text)
        {
            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fullFileName), FileMode.Create))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text.ToString());
                fs.Write(bytes, 0, bytes.Length);
            }
        }
    }
}

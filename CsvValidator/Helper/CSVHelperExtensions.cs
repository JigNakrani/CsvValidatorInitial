using CsvHelper;
using System.Reflection;

namespace CsvValidator.Helper
{
    public static class CSVHelperExtensions
    {
        public static bool IsAllHeaderColumnsPresent(this CsvReader csv, Type type)
        {
            string[]? headerColumns = csv.HeaderRecord?.Select(p => p.ToLower()).ToArray();

            PropertyInfo[] properties = type.GetProperties();
            string[] propertyNames = properties.Select(p => p.Name.ToLower()).ToArray();

            //Check if property is not found in column list from csv file
            foreach (string propertyName in propertyNames)
            {
                if (headerColumns?.Any(x => x == propertyName) == false)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsCorrectColumnOrder(this CsvReader csv, Type type)
        {
            string[]? headerColumns = csv.HeaderRecord?.Select(p => p.ToLower()).ToArray();

            PropertyInfo[] properties = type.GetProperties();
            string[] propertyNames = properties.Select(p => p.Name.ToLower()).ToArray();

            if (headerColumns?.Length != propertyNames.Length)
                return false;

            for (int i = 0; i < headerColumns.Length; i++)
            {
                if (headerColumns[i] != propertyNames[i])
                    return false;
            }

            return true;
        }
    }
}

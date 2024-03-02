using CsvHelper;
using CsvHelper.Configuration;
using CsvValidator.Enum;
using CsvValidator.Helper;
using CsvValidator.Models;
using System.Globalization;
using System.Reflection;

namespace CsvValidator
{
    public class Validator
    {
        #region class members
        private bool _requireHeaderRecord;
        private ValidationType _headerRecordValidationType;

        private bool _validateColumnsOrder;
        private ValidationType _columnsOrderValidationType;

        private List<string> _notNullColumns;
        private ValidationType _nullColumnsValidationType;

        private AllowedYesNoValues _yesNo;
        private List<string> _yesNoColumns;
        private ValidationType _yesNoColumnsValidationType;

        private List<string> _uniqueDataColumns;
        private ValidationType _uniqDataColumnsValidationType;

        private (string columnName, string value)[]? _dataValidationColumns;
        private ValidationType _dataValidationColumnsValidationType;

        private (string columnName, int length)[]? _dataLengthValidationColumns;
        private ValidationType _dataLengthColumnsValidationType;

        private List<ValidationMessage> _validationMessages;
        private int _maxOfMessagesToReturn = 0;
        #endregion

        public Validator()
        {
            //by default, all validations are of type Error. 
            //Set to ValidationType.Information from the Add Validation methods if needed
            _requireHeaderRecord = true;
            _headerRecordValidationType = ValidationType.Error;

            _validateColumnsOrder = true;
            _columnsOrderValidationType = ValidationType.Error;

            _notNullColumns = new List<string>();
            _nullColumnsValidationType = ValidationType.Error;

            _yesNoColumns = new List<string>();
            _yesNoColumnsValidationType = ValidationType.Error;

            _uniqueDataColumns = new List<string>();
            _uniqDataColumnsValidationType = ValidationType.Error;

            _dataValidationColumnsValidationType = ValidationType.Error;
            _dataLengthColumnsValidationType = ValidationType.Error;

            _validationMessages = new List<ValidationMessage>();
        }

        //public Validator RequireHeaderRecord(bool isHeaderRequired = true)
        //{
        //    _requireHeaderRecord = isHeaderRequired;

        //    return this;
        //}

        public Validator AddColumnOrderValidation(ValidationType validationType = ValidationType.Error, bool shouldColumnsBeInOrder = true)
        {
            _validateColumnsOrder = shouldColumnsBeInOrder;
            _columnsOrderValidationType = validationType;

            return this;
        }

        public Validator AddNotNullValidation(ValidationType validationType = ValidationType.Error, params string[] columnNames)
        {
            _notNullColumns.AddRange(columnNames.ToList());
            _nullColumnsValidationType = validationType;

            return this;
        }

        public Validator AddYesNoValidation(ValidationType validationType = ValidationType.Error, AllowedYesNoValues yesNo = AllowedYesNoValues.YN, params string[] columnNames)
        {
            _yesNo = yesNo;
            _yesNoColumns.AddRange(columnNames.ToList());
            _yesNoColumnsValidationType = validationType;

            return this;
        }

        public Validator AddDataValidation(ValidationType validationType = ValidationType.Error, params (string columnName, string value)[] columns)
        {
            _dataValidationColumns = columns;
            _dataValidationColumnsValidationType = validationType;

            return this;
        }

        public Validator AddDataLengthValidation(ValidationType validationType = ValidationType.Error, params (string columnName, int length)[] columns)
        {
            _dataLengthValidationColumns = columns;
            _dataLengthColumnsValidationType = validationType;

            return this;
        }

        public Validator AddUniqueDataValidation(ValidationType validationType = ValidationType.Error, params string[] columnName)
        {
            _uniqueDataColumns.AddRange(columnName.ToList());
            _uniqDataColumnsValidationType = validationType;

            return this;
        }

        public Validator MaxNoOfMessages(int maxOfMessagesToReturn)
        {
            _maxOfMessagesToReturn = maxOfMessagesToReturn;

            return this;
        }

        private void badDataFound(BadDataFoundArgs args)
        {
            _validationMessages.Add(new ValidationMessage() { Message = $"Bad data: {args.Field}. (Replace \" with \"\")", ValidationType = ValidationType.Error });
        }

        public List<ValidationMessage> Validate<T>(string filePath)
        {
            try
            {
                using (FileStream fstream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return Validate<T>(fstream);
                }
            }
            catch (Exception ex)
            {
                return new List<ValidationMessage>() { new ValidationMessage() { Message = $"Unknown error occurred during validation: {ex.Message}", ValidationType = ValidationType.Error } };
            }
        }

        public List<ValidationMessage> Validate<T>(FileStream fstream)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    NewLine = Environment.NewLine,
                    PrepareHeaderForMatch = args => args.Header.ToLower(),
                    BadDataFound = badDataFound
                };

                using (var csv = new CsvReader(new StreamReader(fstream), config))
                {

                    csv.Read();
                    csv.ReadHeader();

                    /*
                                Check if Header Record is required
                    */
                    if (_requireHeaderRecord)
                    {
                        if (csv.IsAllHeaderColumnsPresent(typeof(T)) == false)
                        {
                            string properties = string.Join(", ", typeof(T).GetProperties().Select(p => p.Name));
                            _validationMessages.Add(new ValidationMessage() { Message = $"Missing one ore more column headers. Required columns: {properties}", ValidationType = ValidationType.Error });

                            //If all columns are not present, no further checking is needed
                            return _validationMessages;
                        }
                    }


                    /*
                                Check for correct column order (if column headers are not missing)
                    */
                    if (_validateColumnsOrder == true)
                    {
                        if (csv.IsCorrectColumnOrder(typeof(T)) == false)
                        {
                            string properties = string.Join(", ", typeof(T).GetProperties().Select(p => p.Name));

                            _validationMessages.Add(new ValidationMessage() { Message = $"Incorrect column order. Please make sure columns are in this order: {properties}", ValidationType = _columnsOrderValidationType });
                        }
                    }

                    /*
                                If all good with header, retrieve the records from csv
                    */
                    var records = csv.GetRecords<T>().ToList();

                    /*
                                Check for NOT NULL validation
                    */
                    if (_notNullColumns.Count() > 0)
                    {
                        foreach (string notNullColumn in _notNullColumns)
                        {
                            PropertyInfo? property = typeof(T).GetProperty(notNullColumn);

                            if (records.Any(x => string.IsNullOrEmpty(property?.GetValue(x)?.ToString())))
                            {
                                _validationMessages.Add(new ValidationMessage() { Message = $"Value cannot be empty or null in column {notNullColumn}", ValidationType = _yesNoColumnsValidationType });
                            }
                        }
                    }

                    /*
                                Data validation  (i.e. if you specify CountryCode column and value 'US', then all rows must have value 'US')
                    */
                    if (_dataValidationColumns != null)
                    {
                        foreach ((string columnName, string value) in _dataValidationColumns)
                        {
                            PropertyInfo? property = typeof(T).GetProperty(columnName);

                            if (records.Any(x => property?.GetValue(x)?.ToString() != value))
                            {
                                _validationMessages.Add(new ValidationMessage() { Message = $"Invalid data in column {columnName}. Allowed value: {value}", ValidationType = _dataValidationColumnsValidationType });
                            }
                        }
                    }

                    /*
                                Data Length validation for columns
                    */
                    if (_dataLengthValidationColumns != null)
                    {
                        foreach ((string columnName, int length) in _dataLengthValidationColumns)
                        {
                            PropertyInfo? property = typeof(T).GetProperty(columnName);

                            if (records.Any(x => property?.GetValue(x)?.ToString()?.Length > length))
                            {
                                _validationMessages.Add(new ValidationMessage() { Message = $"Data length in column {columnName} exceeds maximum length of {length}", ValidationType = _dataLengthColumnsValidationType });
                            }
                        }
                    }

                    /*
                                Yes/No column validation  (allows only Y or N as column value, case insensitive)
                    */
                    foreach (string columnName in _yesNoColumns)
                    {
                        PropertyInfo? property = typeof(T).GetProperty(columnName);

                        if (_yesNo == AllowedYesNoValues.YN)
                        {
                            if (records.Any(x => property?.GetValue(x)?.ToString()?.ToLower() != "y"
                                    && property?.GetValue(x)?.ToString()?.ToLower() != "n"))
                            {
                                _validationMessages.Add(new ValidationMessage() { Message = $"Invalid data in column {columnName}. Allowed values: Y/N", ValidationType = _yesNoColumnsValidationType });
                            }
                        }
                        else
                        {
                            if (records.Any(x => property?.GetValue(x)?.ToString()?.ToLower() != "yes"
                                    && property?.GetValue(x)?.ToString()?.ToLower() != "no"))
                            {
                                _validationMessages.Add(new ValidationMessage() { Message = $"Invalid data in column {columnName}. Allowed values: Yes/No", ValidationType = _yesNoColumnsValidationType });
                            }
                        }
                    }

                    /*
                            Validate Unique Data columns
                    */
                    if (_uniqueDataColumns.Count > 0)
                    {
                        HashSet<string> set = new HashSet<string>();

                        //Get property objects for unique columns
                        PropertyInfo?[] uniqueColumnProps = _uniqueDataColumns.Select(x => typeof(T).GetProperty(x)).ToArray();

                        foreach (T p in records)
                        {
                            string uniqueVal = "";

                            //Get values for all unique columns, and join them to make a string
                            uniqueVal = string.Join("", uniqueColumnProps.Select(x => x?.GetValue(p)?.ToString()).ToArray());

                            if (set.Contains(uniqueVal))
                            {
                                _validationMessages.Add(new ValidationMessage() { Message = $"Duplicate values found for column(s): {string.Join(", ", _uniqueDataColumns)}", ValidationType = _uniqDataColumnsValidationType });

                                break;
                            }
                            else
                            {
                                set.Add(uniqueVal);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _validationMessages.Add(new ValidationMessage() { Message = $"Unknown error occurred during validation: {ex.Message}", ValidationType = ValidationType.Error });
            }

            if (_maxOfMessagesToReturn == 0)
            {
                return _validationMessages;
            }
            else
            {
                return _validationMessages.Select(x => x).Take(_maxOfMessagesToReturn).ToList();
            }
        }
    }
}

// See https://aka.ms/new-console-template for more information

using CsvValidator.Enum;
using System.Reflection;
using ValidatorTester;


//Configure product validator to add validations
CsvValidator.Validator csvValidator = new CsvValidator.Validator()
                        .AddColumnOrderValidation(ValidationType.Error, true)
                        .AddNotNullValidation(ValidationType.Error, "CustomerId", "CustomerName", "Email", "Active")
                        .AddYesNoValidation(ValidationType.Error, AllowedYesNoValues.YN, "Active")
                        .AddDataLengthValidation(ValidationType.Error, ("CustomerName", 30), ("Phone", 20), ("Email", 50), ("Active", 1))
                        .AddDataValidation(ValidationType.Error, ("Country", "USA"))
                        .AddUniqueDataValidation(ValidationType.Information, "CustomerId", "CustomerName", "Email");


//Validate Products csv file
string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//string fileToValidate = $@"{path}\TestFiles\Valid-CustomerFile.csv";
string fileToValidate = $@"{path}\TestFiles\Invalid-CustomerFile.csv";

var validationMessages = csvValidator.Validate<Customer>(fileToValidate);


//Log validation messages to Console
LogValidationErrorsToConsole(fileToValidate, validationMessages);







void LogValidationErrorsToConsole(string fileName, List<CsvValidator.Models.ValidationMessage> validattionMessages)
{
    //If there are errors, display the list of messages on Console.
    Console.Clear();
    if (validattionMessages.Count > 0)
    {
        Console.WriteLine("-------------------------------------------------------------");
        Console.WriteLine($"Inalid File: {fileName}");
        Console.WriteLine("-------------------------------------------------------------");
        foreach (CsvValidator.Models.ValidationMessage message in validattionMessages)
        {
            Console.WriteLine(message.ValidationType.ToString() + " - " + message.Message);
        }
    }
    else
    {
        Console.WriteLine($"Valid File: {fileName}");
    }
    Console.WriteLine(); Console.WriteLine();
    Console.ReadKey();
}
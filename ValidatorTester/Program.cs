// See https://aka.ms/new-console-template for more information

using CsvValidator;
using CsvValidator.Enum;
using ValidatorTester;

Console.WriteLine("Hello, World!");

//Configure product validator to add validations
CsvValidator.Validator csvValidator = new CsvValidator.Validator()
                        .AddColumnOrderValidation(ValidationType.Error, true)
                        .AddNotNullValidation(ValidationType.Error, "LocationCountry", "LocationCode", "ProductName", "ProductContact")
                        .AddYesNoValidation(ValidationType.Error, AllowedYesNoValues.YN, "Active")
                        .AddDataLengthValidation(ValidationType.Error, ("LocationCountry", 2), ("LocationCode", 5), ("ProductID", 100), ("ProductName", 200), ("ProductReference", 300), ("ProductMatrix", 200), ("ProductDescription", 2000), ("ProductContact", 50), ("ProductCertified", 100));
                        //.AddDataValidation(ValidationType.Error, ("ProductUploadKey", "6FAF1450-F4CE-450F-9656-29FAF2D456B4"), ("LocationCountry", "NL"), ("LocationCode", "NL019"))
                        //.AddUniqueDataValidation(ValidationType.Information, "ProductUploadKey", "ProductID", "ProductName", "ProductReference", "ProductMatrix");


//Validate Products csv file
string fileName = @"C:\dev\Docs\ServicesMap\SFTP\InvalidFile - 1.csv";
var validationMessages = csvValidator.Validate<Product>(fileName);


LogValidationErrorsToConsole(fileName, validationMessages);

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
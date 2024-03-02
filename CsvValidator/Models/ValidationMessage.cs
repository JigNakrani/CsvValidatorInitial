using CsvValidator.Enum;

namespace CsvValidator.Models
{
    public class ValidationMessage
    {
        public ValidationType ValidationType { get; set; }
        public string? Message { get; set; }
    }
}

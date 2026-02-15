using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ContactManager.Web.Backend.Models
{
    /// <summary>
    /// Model for updating contact via API
    /// </summary>
    public class UpdateContactRequest
    {
        [Required(ErrorMessage = "Contact ID is required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public bool Married { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Phone must be between 5 and 20 characters")]
        [RegularExpression(@"^[+]?[0-9\s\-\(\)]+$", ErrorMessage = "Invalid phone format")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Salary is required")]
        [Range(0, 10000000, ErrorMessage = "Salary must be between 0 and 10,000,000")]
        [DataType(DataType.Currency)]
        public decimal Salary { get; set; }
    }

    /// <summary>
    /// Response for contact operation
    /// </summary>
    public class ContactOperationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ContactsAdded { get; set; }
        public ContactDto? Contact { get; set; }
    }

    /// <summary>
    /// DTO for contact
    /// </summary>
    public class ContactDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool Married { get; set; }
        public string Phone { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public int Age => DateTime.Now.Year - DateOfBirth.Year;
    }

    /// <summary>
    /// Model for CSV file upload
    /// </summary>
    public class CsvUploadRequest
    {
        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; } = null!;
    }

    /// <summary>
    /// Contact statistics
    /// </summary>
    public class ContactsStatisticsResponse
    {
        public int TotalContacts { get; set; }
        public int MarriedContacts { get; set; }
        public int SingleContacts => TotalContacts - MarriedContacts;
        public double AverageSalary { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public double AverageAge { get; set; }
        public double MarriedPercentage => TotalContacts > 0 ? Math.Round((double)MarriedContacts / TotalContacts * 100, 2) : 0;
    }
}
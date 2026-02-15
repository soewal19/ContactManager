using System;
using System.ComponentModel.DataAnnotations;

namespace ContactManager.Core.Models
{
    public class ContactCsvRecord
    {
        public string Name { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string Married { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
    }

    public class Contact
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        public bool Married { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }
    }
}

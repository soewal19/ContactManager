using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactManager.Core.Interfaces;
using ContactManager.Core.Models;
using ContactManager.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;

namespace ContactManager.Web.Controllers
{
    public class ContactsController : Controller
    {
        private readonly IContactService _contactService;
        private readonly ICsvService _csvService;
        private readonly IValidator<Contact> _validator;

        public ContactsController(IContactService contactService, ICsvService csvService, IValidator<Contact> validator)
        {
            _contactService = contactService;
            _csvService = csvService;
            _validator = validator;
        }

        public async Task<IActionResult> Index()
        {
            var contacts = await _contactService.GetAllContactsAsync();
            return View(contacts);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a CSV file to upload.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                Console.WriteLine($"Uploading file: {file.FileName}, Size: {file.Length}");
                using var stream = file.OpenReadStream();
                var contacts = _csvService.ParseContacts(stream).ToList();
                Console.WriteLine($"Parsed {contacts.Count} contacts from CSV");
                
                var validContacts = new List<Contact>();
                var errors = new List<string>();

                foreach (var contact in contacts)
                {
                    var validationResult = await _validator.ValidateAsync(contact);
                    if (validationResult.IsValid)
                    {
                        validContacts.Add(contact);
                    }
                    else
                    {
                        errors.Add($"Contact {contact.Name}: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
                    }
                }

                if (validContacts.Any())
                {
                    await _contactService.AddContactsAsync(validContacts);
                }

                if (errors.Any())
                {
                    TempData["ErrorMessage"] = $"Some contacts were skipped due to validation errors: {string.Join("; ", errors)}";
                }
                
                if (validContacts.Any())
                {
                    TempData["SuccessMessage"] = $"{validContacts.Count} contacts uploaded successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] Contact contact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _contactService.UpdateContactAsync(contact);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _contactService.DeleteContactAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

using ContactManager.Core.Interfaces;
using ContactManager.Core.Models;
using ContactManager.Web.Backend.Services;
using ContactManager.Web.Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ContactManager.Web.Backend.Controllers
{
    /// <summary>
    /// Backend controller for contact management
    /// Handles API requests for contact operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsApiController : ControllerBase
    {
        private readonly ContactBackendService _backendService;
        private readonly ILogger<ContactsApiController> _logger;

        public ContactsApiController(
            ContactBackendService backendService,
            ILogger<ContactsApiController> logger)
        {
            _backendService = backendService;
            _logger = logger;
        }

        /// <summary>
        /// Get all contacts
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactDto>>> GetAllContacts()
        {
            try
            {
                _logger.LogInformation("API request to get all contacts");
                var contacts = await _backendService.GetAllContactsAsync();
                
                var contactDtos = contacts.Select(c => new ContactDto
                {
                    Id = c.Id,
                    Name = c.Name ?? string.Empty,
                    DateOfBirth = c.DateOfBirth,
                    Married = c.Married,
                    Phone = c.Phone ?? string.Empty,
                    Salary = c.Salary
                }).ToList();

                _logger.LogInformation($"Returned {contactDtos.Count} contacts via API");
                return Ok(contactDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contacts via API");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get contact by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ContactDto>> GetContact(int id)
        {
            try
            {
                _logger.LogInformation($"API request to get contact with ID: {id}");
                var contact = await _backendService.GetContactByIdAsync(id);

                var contactDto = new ContactDto
                {
                    Id = contact.Id,
                    Name = contact.Name ?? string.Empty,
                    DateOfBirth = contact.DateOfBirth,
                    Married = contact.Married,
                    Phone = contact.Phone ?? string.Empty,
                    Salary = contact.Salary
                };

                return Ok(contactDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Contact with ID {id} not found: {ex.Message}");
                return NotFound(new { error = "Contact not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving contact with ID: {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Update contact
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ContactOperationResponse>> UpdateContact(int id, [FromBody] UpdateContactRequest request)
        {
            try
            {
                _logger.LogInformation($"API request to update contact with ID: {id}");

                if (id != request.Id)
                {
                    _logger.LogWarning($"ID mismatch in path and body: {id} != {request.Id}");
                    return BadRequest(new ContactOperationResponse
                    {
                        Success = false,
                        Message = "IDs in path and body do not match"
                    });
                }

                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    _logger.LogWarning($"Invalid model when updating contact: {errors}");
                    return BadRequest(new ContactOperationResponse
                    {
                        Success = false,
                        Message = $"Validation errors: {errors}"
                    });
                }

                var contact = new Contact
                {
                    Id = request.Id,
                    Name = request.Name?.Trim() ?? string.Empty,
                    DateOfBirth = request.DateOfBirth,
                    Married = request.Married,
                    Phone = request.Phone?.Trim() ?? string.Empty,
                    Salary = request.Salary
                };

                var success = await _backendService.UpdateContactAsync(contact);

                if (success)
                {
                    _logger.LogInformation($"Contact with ID {id} successfully updated via API");
                    return Ok(new ContactOperationResponse
                    {
                        Success = true,
                        Message = "Contact successfully updated",
                        Contact = new ContactDto
                        {
                            Id = contact.Id,
                            Name = contact.Name ?? string.Empty,
                            DateOfBirth = contact.DateOfBirth,
                            Married = contact.Married,
                            Phone = contact.Phone ?? string.Empty,
                            Salary = contact.Salary
                        }
                    });
                }
                else
                {
                    _logger.LogWarning($"Failed to update contact with ID: {id}");
                    return StatusCode(500, new ContactOperationResponse
                    {
                        Success = false,
                        Message = "Failed to update contact"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating contact with ID: {id}");
                return StatusCode(500, new ContactOperationResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Delete contact
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ContactOperationResponse>> DeleteContact(int id)
        {
            try
            {
                _logger.LogInformation($"API request to delete contact with ID: {id}");

                var success = await _backendService.DeleteContactAsync(id);

                if (success)
                {
                    _logger.LogInformation($"Contact with ID {id} successfully deleted via API");
                    return Ok(new ContactOperationResponse
                    {
                        Success = true,
                        Message = "Contact successfully deleted"
                    });
                }
                else
                {
                    _logger.LogWarning($"Failed to delete contact with ID: {id}");
                    return NotFound(new ContactOperationResponse
                    {
                        Success = false,
                        Message = "Contact not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting contact with ID: {id}");
                return StatusCode(500, new ContactOperationResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get contact statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ContactsStatisticsResponse>> GetStatistics()
        {
            try
            {
                _logger.LogInformation("API request to get contact statistics");
                var statistics = await _backendService.GetContactStatisticsAsync();

                var response = new ContactsStatisticsResponse
                {
                    TotalContacts = statistics.TotalContacts,
                    MarriedContacts = statistics.MarriedContacts,
                    AverageSalary = Math.Round(statistics.AverageSalary, 2),
                    MinSalary = statistics.MinSalary,
                    MaxSalary = statistics.MaxSalary,
                    AverageAge = Math.Round(statistics.AverageAge, 1)
                };

                _logger.LogInformation($"Statistics received: {response.TotalContacts} contacts");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact statistics");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Upload CSV file
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<ContactOperationResponse>> UploadCsv([FromForm] CsvUploadRequest request)
        {
            try
            {
                _logger.LogInformation($"API request to upload CSV file: {request.File?.FileName ?? "no name"}");

                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new ContactOperationResponse
                    {
                        Success = false,
                        Message = "File not selected or empty"
                    });
                }

                var result = await _backendService.ProcessCsvUploadAsync(request.File);

                if (result.contactsAdded > 0)
                {
                    return Ok(new ContactOperationResponse
                    {
                        Success = true,
                        Message = result.message,
                        ContactsAdded = result.contactsAdded
                    });
                }
                else
                {
                    return BadRequest(new ContactOperationResponse
                    {
                        Success = false,
                        Message = result.message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading CSV file via API");
                return StatusCode(500, new ContactOperationResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }
    }
}
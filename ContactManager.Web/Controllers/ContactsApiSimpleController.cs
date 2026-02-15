using Microsoft.AspNetCore.Mvc;
using ContactManager.Core.Interfaces;
using ContactManager.Core.Models;

namespace ContactManager.Web.Controllers
{
    /// <summary>
    /// Contacts API for Swagger documentation
    /// </summary>
    [Route("api/contacts")]
    [ApiController]
    public class ContactsApiSimpleController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactsApiSimpleController(IContactService contactService)
        {
            _contactService = contactService;
        }

        /// <summary>
        /// Get all contacts
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contact>>> GetAll()
        {
            var contacts = await _contactService.GetAllContactsAsync();
            return Ok(contacts);
        }

        /// <summary>
        /// Get contact by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetById(int id)
        {
            var contact = await _contactService.GetContactByIdAsync(id);
            if (contact == null) return NotFound();
            return Ok(contact);
        }

        /// <summary>
        /// Create new contact
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Contact>> Create([FromBody] Contact contact)
        {
            await _contactService.AddContactAsync(contact);
            return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
        }

        /// <summary>
        /// Update contact
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Contact contact)
        {
            if (id != contact.Id) return BadRequest();
            await _contactService.UpdateContactAsync(contact);
            return NoContent();
        }

        /// <summary>
        /// Delete contact
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _contactService.DeleteContactAsync(id);
            return NoContent();
        }
    }
}

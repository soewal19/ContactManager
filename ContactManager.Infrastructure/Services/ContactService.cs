using System.Collections.Generic;
using System.Threading.Tasks;
using ContactManager.Core.Interfaces;
using ContactManager.Core.Models;
using ContactManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactManager.Infrastructure.Services
{
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contact>> GetAllContactsAsync()
        {
            return await _context.Contacts.ToListAsync();
        }

        public async Task AddContactsAsync(IEnumerable<Contact> contacts)
        {
            _context.Contacts.AddRange(contacts);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateContactAsync(Contact contact)
        {
            _context.Update(contact);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteContactAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
        }
    }
}

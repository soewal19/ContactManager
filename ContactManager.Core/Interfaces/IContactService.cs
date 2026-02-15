using System.Collections.Generic;
using System.Threading.Tasks;
using ContactManager.Core.Models;

namespace ContactManager.Core.Interfaces
{
    public interface IContactService
    {
        Task<IEnumerable<Contact>> GetAllContactsAsync();
        Task AddContactsAsync(IEnumerable<Contact> contacts);
        Task UpdateContactAsync(Contact contact);
        Task DeleteContactAsync(int id);
    }
}

using System.Collections.Generic;
using System.IO;
using ContactManager.Core.Models;

namespace ContactManager.Core.Interfaces
{
    public interface ICsvService
    {
        IEnumerable<Contact> ParseContacts(Stream csvStream);
    }
}

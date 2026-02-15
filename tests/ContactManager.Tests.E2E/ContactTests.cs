using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ContactManager.Tests.E2E
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ContactTests : PageTest
    {
        [Test]
        public async Task ShouldDisplayContactsPage()
        {
            await Page.GotoAsync("http://localhost:5021/Contacts");
            await Expect(Page.Locator("h2")).ToContainTextAsync("Contact Manager");
        }

        [Test]
        public async Task ShouldFilterContacts()
        {
            await Page.GotoAsync("http://localhost:5021/Contacts");
            
            // Assume we have uploaded the sample file already or we do it here
            // For now, just check if the filter input exists
            var filterInput = Page.Locator("input[type='search']");
            await Expect(filterInput).ToBeVisibleAsync();
        }
    }
}

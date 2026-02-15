using Microsoft.AspNetCore.Mvc;

namespace ContactManager.Web.Controllers
{
    public class DocumentationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

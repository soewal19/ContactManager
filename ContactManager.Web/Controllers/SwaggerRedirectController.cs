using Microsoft.AspNetCore.Mvc;

namespace ContactManager.Web.Controllers
{
    public class SwaggerRedirectController : Controller
    {
        // Redirect old swagger path to our new documentation page
        [Route("swagger")]
        [Route("swagger/index.html")]
        public IActionResult RedirectSwagger()
        {
            return RedirectToAction("Index", "Documentation");
        }
    }
}

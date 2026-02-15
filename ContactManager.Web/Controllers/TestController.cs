using Microsoft.AspNetCore.Mvc;

namespace ContactManager.Web.Controllers
{
    /// <summary>
    /// Simple API controller for testing Swagger
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Gets a simple test message
        /// </summary>
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "API is working!";
        }

        /// <summary>
        /// Gets a test value by ID
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<string> GetById(int id)
        {
            return $"Test item {id}";
        }
    }
}

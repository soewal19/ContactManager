namespace ContactManager.Web.Config
{
    /// <summary>
    /// Configuration for Swagger documentation
    /// </summary>
    public static class SwaggerConfiguration
    {
        /// <summary>
        /// Adds Swagger services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        public static void AddSwaggerServices(IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                // Include working API controllers
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var controllerName = apiDesc.ActionDescriptor.RouteValues["controller"];
                    // Include all API controllers
                    return controllerName == "Test" || 
                           controllerName == "Contacts" ||
                           controllerName == "ContactsApiSimple" ||
                           controllerName == "ContactsApi";
                });
            });
        }

        /// <summary>
        /// Configures the application to use Swagger middleware
        /// </summary>
        /// <param name="app">Web application</param>
        public static void UseSwaggerMiddleware(WebApplication app)
        {
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.)
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Contact Manager API v1");
                options.RoutePrefix = "swagger-api";
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();
        }
    }
}
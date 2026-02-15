using ContactManager.Core.Interfaces;
using ContactManager.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using ContactManager.Core.Validators;
using ContactManager.Web.Backend.Services;

namespace ContactManager.Web.Config
{
    /// <summary>
    /// Centralized application configuration class
    /// Organizes all service registrations and configurations
    /// </summary>
    public static class ApplicationConfiguration
    {
        /// <summary>
        /// Configures all application services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Application configuration</param>
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure MVC with validation
            ConfigureMvc(services);
            
            // Configure database
            DatabaseConfiguration.ConfigureDatabase(services, configuration);
            
            // Configure business services
            ConfigureBusinessServices(services);
            
            // Configure external services
            ConfigureExternalServices(services);
            
            // Configure Swagger documentation
            SwaggerConfiguration.AddSwaggerServices(services);
        }

        /// <summary>
        /// Configures MVC and validation services
        /// </summary>
        private static void ConfigureMvc(IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
            {
                // Configure MVC options for better performance and security
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                    _ => "The value must not be null.");
                options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
                    (value, fieldName) => $"The value '{value}' is not valid for {fieldName}.");
            });

            // Configure FluentValidation
            services.AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters()
                    .AddValidatorsFromAssemblyContaining<ContactValidator>();
        }

        /// <summary>
        /// Configures business layer services
        /// </summary>
        private static void ConfigureBusinessServices(IServiceCollection services)
        {
            // Register core services
            services.AddScoped<ICsvService, CsvService>();
            services.AddScoped<IContactService, ContactService>();
            
            // Register backend services
            services.AddScoped<ContactBackendService>();
            
            // Register repositories (if any)
            // services.AddScoped<IContactRepository, ContactRepository>();
        }

        /// <summary>
        /// Configures external services (logging, caching, etc.)
        /// </summary>
        private static void ConfigureExternalServices(IServiceCollection services)
        {
            // Configure logging (already configured by default in ASP.NET Core)
            
            // Configure memory cache for better performance
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1000; // Maximum number of cache entries
                options.CompactionPercentage = 0.25; // Compact when 25% of entries are expired
            });
            
            // Configure response caching
            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024; // Maximum response size to cache (1MB)
                options.UseCaseSensitivePaths = false;
            });
        }

        /// <summary>
        /// Configures the HTTP request pipeline
        /// </summary>
        /// <param name="app">Web application</param>
        public static void ConfigurePipeline(WebApplication app)
        {
            // Configure error handling
            ConfigureErrorHandling(app);
            
            // Configure security
            ConfigureSecurity(app);
            
            // Configure routing and static files
            ConfigureRouting(app);
            
            // Configure caching
            ConfigureCaching(app);
            
            // Configure Swagger documentation
            SwaggerConfiguration.UseSwaggerMiddleware(app);
        }

        /// <summary>
        /// Configures error handling middleware
        /// </summary>
        private static void ConfigureErrorHandling(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts(); // HTTP Strict Transport Security
            }
        }

        /// <summary>
        /// Configures security middleware
        /// </summary>
        private static void ConfigureSecurity(WebApplication app)
        {
            app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
            
            // Configure security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                
                await next();
            });
        }

        /// <summary>
        /// Configures routing and static files
        /// </summary>
        private static void ConfigureRouting(WebApplication app)
        {
            app.UseStaticFiles(); // Serve static files
            app.UseRouting(); // Enable routing
            app.UseAuthorization(); // Enable authorization
            
            // Configure default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }

        /// <summary>
        /// Configures caching middleware
        /// </summary>
        private static void ConfigureCaching(WebApplication app)
        {
            app.UseResponseCaching(); // Enable response caching
            
            // Configure cache control for static files
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    const int durationInSeconds = 60 * 60 * 24 * 7; // 7 days
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public,max-age={durationInSeconds}");
                    ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddSeconds(durationInSeconds).ToString("R"));
                }
            });
        }
    }
}
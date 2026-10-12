using Microsoft.OpenApi;
using ResumeManagement.Api.Helpers;
using ResumeManagement.Api.Interfaces;
using ResumeManagement.Api.Repositories;
using ResumeManagement.Api.Services;
using ResumeManagement.Api.Utils;
namespace ResumeManagement.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // =====================================
            // 1. REPOSITORY
            // =====================================

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddHttpContextAccessor();
            services.AddScoped<UrlService>();


            services.AddScoped<IApplicantService, ApplicantService>();
            services.Configure<FileSettings>(configuration.GetSection("FileSettings"));

            services.AddScoped<IFileStorageService, FileStorageService>();

            // =====================================
            // 2. SWAGGER
            // =====================================

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Placement Management System API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []

                });
            });
            // =====================================
            // 3. OUTPUT CACHE
            // =====================================

            services.AddOutputCache();

            // =====================================
            // 4. HEALTH CHECKS
            // =====================================

            services.AddHealthChecks();

            return services;
        }
    }
}
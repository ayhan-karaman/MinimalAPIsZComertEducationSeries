using System.ComponentModel.DataAnnotations;
using Entities.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Configuration;
public static class ConfigurationExtensions
{
    public static void ValidationIdInRange(this int id)
    {
        if (!(id > 0 && id <= 1000))
            throw new ArgumentOutOfRangeException("1-1000");
    }

    public static void Validate<T>(this T item)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(item);
        var isValid = Validator.TryValidateObject(item, context, validationResults, true);
        if (!isValid)
        {
            var errors = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
            throw new ValidationException(errors);
        }
    }

    public static void UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var contextFeatures = context.Features.Get<IExceptionHandlerFeature>();

                if (contextFeatures is not null)
                {
                    context.Response.StatusCode = contextFeatures.Error switch
                    {
                        NotFoundException => StatusCodes.Status404NotFound,
                        ValidationException => StatusCodes.Status422UnprocessableEntity,
                        ArgumentOutOfRangeException => StatusCodes.Status400BadRequest,
                        _ => StatusCodes.Status500InternalServerError
                    };

                    await context.Response.WriteAsync(
                            new ErrorDetails()
                            {
                                Message = contextFeatures.Error.Message,
                                StatusCode = context.Response.StatusCode
                            }.ToString()
                    );
                }
            });
        });

    }

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("all", builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });

            options.AddPolicy("special", builder =>
            {
                builder.WithOrigins("https://localhost:300")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });

        return services;
    }


    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "Book API",
                Version = "V1",
                Description = "Virtual campus minimal api training program",
                License = new(),
                TermsOfService = new("https://www.samsun.edu.tr"),
                Contact = new()
                {
                    Email = "example@example.com.tr",
                    Name = "Ayhan Karaman",
                    Url = new("https://www.youtube.com/@virtual.campus")
                }
            });
        });

        return services;

    }

}
using Serilog;
using AMS.Services.ServiceRegistration;
using AMS.Controller.Extensions;
using FluentValidation.AspNetCore;
using System.Reflection;
using JwtAuthenticationManager;
using JwtAuthenticationManager.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices(builder.Configuration);

builder.Services.AddCustomJwtAuthentication();
builder.Services.AddSingleton<JwtTokenHandler>();

var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.CustomSink()
                .Enrich.FromLogContext()
                .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddAutomaticLogs();

builder.Services.AddControllers()
    .AddFluentValidation(v =>
    {
        v.ImplicitlyValidateChildProperties = true;
        v.ImplicitlyValidateRootCollectionElements = true;
        v.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }); ;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Attendence Management System", Version = "v1" });
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer"
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
                });


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendence Management System V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles();

app.UseMiddleware(typeof(ExceptionHandlingMiddleware));

app.MapControllers();

app.Run();

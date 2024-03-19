using Serilog;
using AMS.Services.ServiceRegistration;
using AMS.Controller.Extensions;
using FluentValidation.AspNetCore;
using System.Reflection;
using JwtAuthenticationManager;
using JwtAuthenticationManager.Extensions;

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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseStaticFiles();

app.UseMiddleware(typeof(ExceptionHandlingMiddleware));

app.MapControllers();

app.Run();

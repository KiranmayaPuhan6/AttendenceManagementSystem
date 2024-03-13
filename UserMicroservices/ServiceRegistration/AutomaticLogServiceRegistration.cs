using Microsoft.AspNetCore.Mvc;

namespace UserMicroservices.ServiceRegistration
{
    public static class AutomaticLogServiceRegistration
    {
        public static IServiceCollection AddAutomaticLogs(this IServiceCollection services)
        {
            services.AddMvc()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        if (!context.ModelState.IsValid)
                        {
                            LogAutomaticBadRequest(context);
                        }

                        return new BadRequestObjectResult(context.ModelState);
                    };
                });
            return services;    
        }
        private static void LogAutomaticBadRequest(ActionContext context)
        {
            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(context.ActionDescriptor.DisplayName);

            // Get error messages
            var errorMessages = string.Join($"{System.Environment.NewLine}->", context.ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage));

            var request = context.HttpContext.Request;

            // Use whatever logging information you want
            logger.LogError("Automatic Bad Request occurred." +
                            $"{System.Environment.NewLine}Error(s): " +
                            $"{System.Environment.NewLine}{errorMessages}" +
                            $"{System.Environment.NewLine}|Method: {request.Method}| " +
                            $"{System.Environment.NewLine}|URL-Path: {request.Path}{request.QueryString}|");
        }
    }
}

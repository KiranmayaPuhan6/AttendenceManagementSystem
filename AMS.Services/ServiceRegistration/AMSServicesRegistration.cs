using AMS.Entities.Data.Context;
using AMS.Entities.Infrastructure.Repository;
using AMS.Entities.Infrastructure.Repository.IRepository;
using AMS.Services.Services;
using AMS.Services.Services.IServices;
using AMS.Services.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AMS.Services.ServiceRegistration
{
    public static class AMSServicesRegistration
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
               // options.UseLazyLoadingProxies();
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IResponseService, ResponseService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAttendenceService, AttendenceService>();
            services.Configure<TwilioOptions>(configuration.GetSection("Twilio"));
            services.AddScoped<ISmsService, SmsService>();
            services.Configure<EmailOptions>(configuration.GetSection("Email"));
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ILeaveService, LeaveService>();
            services.AddScoped<IManagerService,ManagerService>();
            services.AddScoped<IHolidayService, HolidayService>();

            return services;
        }
    }
}

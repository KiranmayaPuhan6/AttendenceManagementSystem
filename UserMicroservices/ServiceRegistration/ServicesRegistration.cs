using UserMicroservices.Repository.IRepository;
using UserMicroservices.Repository;
using UserMicroservices.Services.IServices;
using UserMicroservices.Services;
using Microsoft.EntityFrameworkCore;
using UserMicroservices.Data;

namespace UserMicroservices.ServiceRegistration
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddServices(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<UserDbContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IResponseService, ResponseService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}

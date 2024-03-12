using UserMicroservices.Models.Domain.Entities;
using UserMicroservices.Services.IServices;

namespace UserMicroservices.Services
{
    public class UserService : IUserService
    {
        private readonly ICacheService _cacheService;
        public UserService(ICacheService cacheService)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
        private IEnumerable<User> GetData(string key)
        {
            var cacheData = _cacheService.GetData<IEnumerable<User>>(key);
            if (cacheData != null)
            {
                return cacheData;
            }
            return null;
        }

        private bool SetData(string key, IEnumerable<User> data, DateTimeOffset expirationTime)
        {
            var success = _cacheService.SetData<IEnumerable<User>>(key, data, expirationTime);
            return success;
        }
    }
}

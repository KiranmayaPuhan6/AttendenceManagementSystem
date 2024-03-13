
using AMS.Services.Services.IServices;
using AMS.Services.Utility.ResponseModel;

namespace AMS.Services.Services
{
    public class ResponseService : IResponseService
    {
        public async Task<Response<T>> ResponseDtoFormatterAsync<T>(bool isSuccess, int statusCode, string message, T result) where T : class
        {
            var responseDto = new Response<T>()
            {
                IsSuccess = isSuccess,
                StatusCode = statusCode,
                Message = message,
                Result = result
            };
            return responseDto;
        }

        public async Task<ResponseList<T>> ResponseDtoFormatterAsync<T>(bool isSuccess, int statusCode, string message, IEnumerable<T> result) where T : class
        {
            var responseListDto = new ResponseList<T>()
            {
                IsSuccess = isSuccess,
                StatusCode = statusCode,
                Message = message,
                Result = result
            };

            return responseListDto;
        }
    }
}

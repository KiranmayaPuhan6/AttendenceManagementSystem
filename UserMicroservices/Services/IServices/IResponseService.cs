using UserMicroservices.Utility.ResponseModel;

namespace UserMicroservices.Services.IServices
{
    public interface IResponseService
    {
        Task<Response<T>> ResponseDtoFormatterAsync<T>(bool isSuccess, int statusCode, string message, T result) where T : class;
        Task<ResponseList<T>> ResponseDtoFormatterAsync<T>(bool isSuccess, int statusCode, string message, IEnumerable<T> result) where T : class;
    }
}

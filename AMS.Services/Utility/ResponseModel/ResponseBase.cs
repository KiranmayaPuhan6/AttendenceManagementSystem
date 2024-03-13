namespace AMS.Services.Utility.ResponseModel
{
    public abstract class ResponseBase
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }

    }
}

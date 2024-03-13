namespace AMS.Services.Utility.ResponseModel
{
    public class ResponseList<T> : ResponseBase where T : class
    {
        public IEnumerable<T> Result { get; set; }
    }
}

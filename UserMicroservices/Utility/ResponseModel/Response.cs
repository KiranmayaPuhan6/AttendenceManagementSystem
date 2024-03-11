namespace UserMicroservices.Utility.ResponseModel
{
    public class Response<T> : ResponseBase where T : class
    {
        public T Result { get; set; }

    }
}

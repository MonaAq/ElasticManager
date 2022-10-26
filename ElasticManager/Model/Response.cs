namespace ElasticManager.Model
{
    public class Response<T>
    {
        public Response()
        {
        }
        public Response(List<T> data)
        {
            Succeeded = true;
            ErrorMessage = string.Empty;
            Error = null;
            Data = data;
        }
        public List<T> Data { get; set; }
        public bool Succeeded { get; set; }
        public string? Error { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

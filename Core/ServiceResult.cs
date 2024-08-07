namespace Core
{
    public class ServiceResult<T>
    {
        public T? Result = default!;

        public List<string> Errors { get; set; } = [];

        public bool IsSuccess => Result != null && Errors.Count == 0;

        public ResultType ResultType { get; set; }


        public ServiceResult(T? result, ResultType resultType = ResultType.Success)
        {
            Result = result;
            ResultType = resultType;
        }
    }
}

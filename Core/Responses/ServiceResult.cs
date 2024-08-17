namespace Core.Responses
{
    public class ServiceResult<T>
    {
        public T? Result = default!;

        public List<string> Errors { get; set; }

        public bool IsSuccess => ResultType == ResultType.Success;

        public ResultType ResultType { get; set; }

        public ServiceResult(T? result, ResultType resultType = ResultType.Success, List<string>? errors = null)
        {
            Result = result;
            ResultType = resultType;
            Errors = errors ?? [];
        }

        public static ServiceResult<T> DefaultServerFailure()
        {
            return new ServiceResult<T>(default, ResultType.ServerError, ["A server error occurred"]);
        }
    }
}

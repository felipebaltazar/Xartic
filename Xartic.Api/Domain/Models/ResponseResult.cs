namespace Xartic.Api.Domain.Models
{
    public abstract class ResponseResult
    {
        public string Response { get; }

        protected ResponseResult(string response)
        {
            Response = response;
        }

        public static ResponseResult Invalid(string response) =>
            new InvalidResult(response);

        public static ResponseResult AllMatch(string response) =>
            new AllMatchResult(response);

        public static ResponseResult IsClosest(string response) =>
            new IsClosestResult(response);
    }
}

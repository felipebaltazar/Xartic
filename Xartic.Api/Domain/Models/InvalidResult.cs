namespace Xartic.Api.Domain.Models
{
    public sealed class InvalidResult : ResponseResult
    {
        public InvalidResult(string response) : base(response)
        {
        }
    }
}

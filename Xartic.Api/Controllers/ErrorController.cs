using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Xartic.Api.Controllers
{
    /// <summary>
    /// Error controller
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        /// <summary>
        /// Not found redirect
        /// </summary>
        /// <returns></returns>
        [Route("404")]
        [AllowAnonymous]
        public IActionResult Error404() => Redirect("/docs/index.html");
    }
}

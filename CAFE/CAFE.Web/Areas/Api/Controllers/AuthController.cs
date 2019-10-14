using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CAFE.Web.Areas.Api.Controllers
{
    /// <summary>
    /// API endpoint for make user login
    /// </summary>
    [Authorize]
    public class AuthController : ApiController
    {
        /// <summary>
        /// Returns login result
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]        
        public HttpResponseMessage Login()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Cats.Controllers;

[ApiController]
[Route("[controller]")]
public class CatResponseCodesController : Controller
{
    [HttpGet]
    [Route("ProcessUrl")]
    [Obsolete("Obsolete")]
    public string? ProcessUrl(string url)
    {
        var result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (result)
        {
            var request = (HttpWebRequest)WebRequest
                .Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            var statusCode = Convert.ToString((int)response.StatusCode);
            var catsRequest = (HttpWebRequest)WebRequest
                .Create($"https://http.cat/{statusCode}.jpg");
            var catsResponse = (HttpWebResponse)catsRequest.GetResponse();
            return Convert.ToString(catsResponse.GetResponseStream());
        }
        return "Fake link!";
    }
}
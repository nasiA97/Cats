using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Cats.Controllers;

[ApiController]
[Route("[controller]")]
public class CatResponseCodesController : Controller
{
    [HttpGet]
    [Route("ProcessUrl")]
    [Obsolete("Obsolete")]
    public IActionResult ProcessUrl(string url)
    {
        var result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (!result)
        {
            return BadRequest();
        }

        var request = (HttpWebRequest)WebRequest
            .Create(url);
        var response = (HttpWebResponse)request.GetResponse();
        var statusCode = Convert.ToString((int)response.StatusCode);
        
        var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var relativePath = @$"..\..\..\..\img\{statusCode}.png";
        var fullPath = Path.Combine(appDir, relativePath);

        WebClient Client = new WebClient();
        Client.DownloadFile($"https://http.cat/{statusCode}.jpg", fullPath);
        
        return PhysicalFile(fullPath, "image/jpg");
    }
}
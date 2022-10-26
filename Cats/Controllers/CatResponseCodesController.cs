using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Cats.Controllers;

[ApiController]
[Route("[controller]")]
public class CatResponseCodesController : Controller
{
    private IMemoryCache _memoryCache;

    public CatResponseCodesController(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    
    [HttpGet]
    [Route("testCacheGet")]
    public string testCacheGet(string httpCode)
    {
        if (_memoryCache.TryGetValue(httpCode, out string value))
        {
            return value;
        }
        testCacheSet(httpCode, $"httpCode: {httpCode}");
        return testCacheGet(httpCode);
    }

    [HttpGet]
    [Route("testCacheSet")]
    public void testCacheSet(string httpCode, string description)
    {
        _memoryCache.Set(httpCode, description, TimeSpan.FromSeconds(5));
    }

    [HttpGet]
    [Route("CacheGet")]
    public Task<FileStreamResult> CacheGet(string httpCode)
    {
        if (_memoryCache.TryGetValue(httpCode, out Task<FileStreamResult> value))
        {
            return value;
        }
        CacheSet(httpCode, DownloadImage($"https://http.cat/{httpCode}.jpg"));
        return CacheGet(httpCode);
    }

    [HttpGet]
    [Route("CacheSet")]
    public void CacheSet(string httpCode, Task<FileStreamResult> img)
    {
        _memoryCache.Set(httpCode, img, TimeSpan.FromSeconds(30));
    }
    
    [HttpGet]
    [Route("DownloadImage")]
    public async Task<FileStreamResult> DownloadImage(string url)
    {
        var data = await new HttpClient().GetAsync(url);
        var memoryStream = new MemoryStream();
        await data.Content.CopyToAsync(memoryStream);
        var contentType = "image/jpeg";
        memoryStream.Seek(0, SeekOrigin.Begin);
        return File(memoryStream, contentType);
    }

    [HttpGet]
    [Route("ProcessUrl")]
    [Obsolete("Obsolete")]
    public Task<FileStreamResult> ProcessUrl(string url)
    {
        var result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (!result)
        {
            return CacheGet("404");
        }

        var request = (HttpWebRequest)WebRequest
            .Create(url);
        var response = (HttpWebResponse)request.GetResponse();
        var statusCode = Convert.ToString((int)response.StatusCode);
        
        return CacheGet(statusCode);
        
        // var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        // var relativePath = @$"..\..\..\..\img\{statusCode}.png";
        // var fullPath = Path.Combine(appDir, relativePath);
        //
        // WebClient Client = new WebClient();
        // Client.DownloadFile($"https://http.cat/{statusCode}.jpg", fullPath);
        //
        // return PhysicalFile(fullPath, "image/jpg");
    }
}
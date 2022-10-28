using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Cats.Controllers;

[ApiController]
[Route("[controller]")]
public class CatResponseCodesController : Controller
{
    private IMemoryCache _memoryCache;
    public static int _statusCode = 100;
    static readonly HttpClient Client = new HttpClient();

    public CatResponseCodesController(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    private async Task<byte[]> CacheGet(string url)
    {
        if (_memoryCache.TryGetValue(url, out byte[] value))
        {
            return value;
        }

        CacheSet(url, await DownloadImage(url));
        return await CacheGet(url);
    }

    private async Task CacheSet(string url, byte[] img)
    {
        await Task.Run(() => _memoryCache.Set(url, img, TimeSpan.FromSeconds(5)));
    }

    private async Task<byte[]> DownloadImage(string url)
    {
        var data = await new HttpClient().GetAsync(url);

        byte[] image = await data.Content.ReadAsByteArrayAsync();

        return image;
    }

    [HttpGet]
    [Route("ProcessUrl")]
    public async Task<FileContentResult> ProcessUrl(string url)
    {
        var result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        var contentType = "image/jpeg";

        if (!result)
        {
            return File(
                await CacheGet(
                    "https://sun9-west.userapi.com/sun9-50/s/v1/ig2/WTB8jwFgUXDS2PPvsUfwkvz62QAjaYmpCx9rRjRg4szJI5V_w78MPC1AI4Z9q7YOCO4IgdrevptNjJy8GaAUo-DT.jpg?size=951x736&quality=96&type=album"),
                contentType);
        }

        try
        {
            HttpResponseMessage response = await Client.GetAsync(url);
            _statusCode = Convert.ToInt32(response.StatusCode);
        }
        catch
        {
            return File(
                await CacheGet(
                    "https://images-ext-1.discordapp.net/external/z0wgZbVvvfs8UDNLSyM_0mMsruP_VvMM0FJlmdvraCk/https/i11.fotocdn.net/s109/afa79a8bb26c8bc5/public_pin_m/2424279258.jpg%2522%29%29"),
                contentType);
        }

        return File(await CacheGet($"https://http.cat/{_statusCode}.jpg"), contentType);
    }
}
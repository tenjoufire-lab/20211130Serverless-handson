using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

public class Item
{
    public string id { get; set; }
    public string url { get; set; }
}

public static HttpResponseMessage Run(HttpRequestMessage req, Item item, ILogger log)
{
    if (item == null)
    {
        var result = $"Not found for {req.RequestUri.AbsolutePath}";
        log.LogInformation(result);
        var res = new HttpResponseMessage(HttpStatusCode.NotFound);
        res.Content = new StringContent(result);
        return res;
    }
    else
    {
        var result = $"Redirect {req.RequestUri.AbsolutePath} to {item.url}";
        log.LogInformation(result);
        var res = new HttpResponseMessage(HttpStatusCode.Found);
        res.Headers.Location = new Uri(item.url);
        res.Content = new StringContent(result);
        return res;
    }
}

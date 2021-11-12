#r "Newtonsoft.Json"

using System.Net;
using System.Net.Http;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

private static HttpClient client = new HttpClient();

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    var queryString = HttpUtility.ParseQueryString(string.Empty);
    string priceUSDstr = req.Query["priceUSD"];
    float priceUSD = Convert.ToSingle(priceUSDstr);

    var uri = "https://openexchangerates.org/api/latest.json?app_id=87ba891bc3664490bdc2963f1817cf46";

    HttpResponseMessage response;
    response = await client.GetAsync(uri);

    string contentString = await response.Content.ReadAsStringAsync();
    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(contentString);


    float priceJPYperD = Convert.ToSingle(jsonObj.rates.JPY);
    float priceJPY = priceUSD * priceJPYperD;

    return new OkObjectResult(priceJPY.ToString());

}

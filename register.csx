#r "Microsoft.Azure.DocumentDB.Core"
#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

const string DBNAME = "Shortener";
const string COLNAME = "Items";

public class Item
{
    public string id { get; set; }
    public string url { get; set; }
}

public static async Task<IActionResult> Run(
    HttpRequest req,
    DocumentClient client,
    ILogger log
)
{
    var item = new Item() {
        id = req.Query["id"],
        url = req.Query["url"],
    };

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    item.id = item.id ?? data?.id;
    item.url = item.url ?? data?.url;

    if (string.IsNullOrEmpty(item.id)) {
        return new BadRequestResult();
    }

    try {
        if (string.IsNullOrEmpty(item.url)) {
            var uri = UriFactory.CreateDocumentUri(DBNAME, COLNAME, item.id);
            var opts = new RequestOptions() { PartitionKey = new PartitionKey(item.id)　};
            await client.DeleteDocumentAsync(uri, opts);
            log.LogInformation($"Deleted {item.id}");
            return new OkObjectResult("");
        }
        else {
            var uri = UriFactory.CreateCollectionUri(DBNAME, COLNAME);
            await client.UpsertDocumentAsync(uri, item);
            log.LogInformation($"Upserted {item.id}={item.url}");
            return new OkObjectResult("");
        }
    }
    catch (DocumentClientException ex) {
        log.LogError($"Failed {item.id}");
        log.LogError(ex.Message);
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
}

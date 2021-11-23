#r "Microsoft.Azure.DocumentDB.Core"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Primitives;
using NPOI.SS.UserModel;

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
    IWorkbook workbook = null;
    ISheet sheet = null;
    try {
        var stream = new MemoryStream(); 
        await req.Body.CopyToAsync(stream);
        workbook = WorkbookFactory.Create(stream);
        sheet = workbook.GetSheetAt(0);
    }
    catch (Exception ex) {
        log.LogError(ex.Message);
    }
    if (sheet == null) {
        return new OkObjectResult("Failed to open workbook");
    }
    var result = "";
    for (int i = 0; i <= sheet.LastRowNum; i++)
    {
        var row = sheet.GetRow(i);
        if (row.LastCellNum < 2) {
            continue;
        }
        var comment = row.GetCell(0)?.StringCellValue;
        var item = new Item() {
            id = row.GetCell(1)?.StringCellValue,
            url = row.GetCell(2)?.StringCellValue,
        };
        if (!string.IsNullOrEmpty(comment) || string.IsNullOrEmpty(item.id)) {
            continue;
        }
        try {
            if (string.IsNullOrEmpty(item.url)) {
                var uri = UriFactory.CreateDocumentUri(DBNAME, COLNAME, item.id);
                var opts = new RequestOptions() { PartitionKey = new PartitionKey(item.id)　};
                await client.DeleteDocumentAsync(uri, opts);
                var line = $"Deleted {item.id}\n";
                result += line;
                log.LogInformation(line);
            }
            else {
                var uri = UriFactory.CreateCollectionUri(DBNAME, COLNAME);
                await client.UpsertDocumentAsync(uri, item);
                var line = $"Upserted {item.id}={item.url}\n";
                result += line;
                log.LogInformation(line);
            }
        }
        catch (DocumentClientException ex) {
            var line = $"Failed {item.id}\n";
            result += line;
            log.LogInformation(line);
            log.LogError(ex.Message);
        }
    }
    return new OkObjectResult(result);
}

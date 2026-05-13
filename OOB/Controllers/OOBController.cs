using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OOB.Data;
using OOB.Data.Entities;
using OOB.Models;
using System.Buffers.Text;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace OOB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OOBController(OOBDbContext dbContext, ILogger<OOBController> logger, Serilog.ILogger _logger, IConfiguration configuration) : ControllerBase
{

    [HttpPost]
    [Consumes("application/xml", "text/xml", "application/json")]
    public async Task<IActionResult> Post()
    {
        try
        {

            _logger.Information("Request Recieved");
            RequestBody body = await GetTypeAndBody();
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            if(headers.Count > 0)
            {
                _logger.Information("Received XML request headers: {RequestHeaders}", System.Text.Json.JsonSerializer.Serialize(headers));
            }


            await Cleanup();


           

            TransactionResponse transactionResponse;

            if (body.type.Contains("xml"))
            {
                _logger.Information("Received XML request body: {RequestBody}", body.body);
                transactionResponse = await ProcessXmlRequest(body.content);
            }
            else if (body.type.Contains("json"))
            {

                _logger.Information("Received JSON request body: {RequestBody}", body.body);
                transactionResponse = await ProcessJsonRequest(body.content);
            }
            else
            {
                _logger.Warning("Unsupported content type received: {ContentType}", body.type);
                //logger.LogWarning("Unsupported content type received: {ContentType}", contentType);
                return BadRequest("Unsupported content type. Use application/xml or application/json.");
            }


            //Validate Source 
            if (!await IsRequestFromTrustedSource(body,transactionResponse.TR_ApplicationID, headers))
            {
                _logger.Warning("Request from untrusted source rejected");
                return BadRequest("Untrusted source");
            }

            dbContext.TransactionResponses.Add(transactionResponse);
            await dbContext.SaveChangesAsync();

            if(transactionResponse.PS_Id == 0 && transactionResponse.TR_Command =="Void")
            {
                await SyncTrans(transactionResponse.TR_OriginalRequestID);
            }


            _logger.Information("Transaction {TransactionId} processed successfully for RequestID {RequestId}",
               transactionResponse.TR_Id, transactionResponse.TR_RequestID);

            return Ok(new { transactionResponse.TR_Id, Message = "Transaction processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing transaction: {ErrorMessage}", ex.Message);
            if (ex.InnerException != null)
                return StatusCode(500, $"An error occurred while processing the transaction. Reason:{ex.InnerException.Message}");
            else
                return StatusCode(500, $"An error occurred while processing the transaction. Reason:{ex.Message}");

        }
    }



    


    private async Task<bool> IsRequestFromTrustedSource(RequestBody body, Guid appid, Dictionary<string, string> headers)
    {



        if (headers.ContainsKey("Token")|| headers.ContainsKey("token"))
        {
            string key = headers.ContainsKey("key") ? headers["key"]: headers["Key"];
            string token = headers.ContainsKey("token")? headers["token"] : headers["Token"];

            //Get Secret from Database
            ApplicationConfig conf = dbContext.ApplicationConfigs
            .FirstOrDefault(config => config.AC_ApplicationID == appid
                && config.AC_Key == key
                && config.AC_KeyName == "APIKey.Secret");


            if (conf != null)
            {
                string[] parts = token.Split(':');

                string time = parts[0];
                string secret = conf.AC_Value;
                string seckey = parts[1];
                string url =  $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";


                string toHash = time + new Uri(url).AbsolutePath + body.body;
                StringBuilder hash = new StringBuilder();
                using (HMACSHA256 sha = new HMACSHA256(Encoding.ASCII.GetBytes(secret)))
                {
                    foreach (byte b in sha.ComputeHash(Encoding.ASCII.GetBytes(toHash)))
                        hash.Append(b.ToString("x2"));
                }



                if (hash.ToString() != seckey)
                {
                    return false;
                }
            }
        }
        //No Token Specified no need for validation


        return true;
    }

    private async Task<RequestBody> GetTypeAndBody()
    {
        var contentType = Request.ContentType?.ToLowerInvariant() ?? string.Empty;

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        var b64 = String.Empty;

        try
        {
            b64 = body.Substring(body.IndexOf(':') + 2, body.LastIndexOf('}') - 1 - (body.IndexOf(':') + 2));
            if (!IsBase64String(b64))
            {
                b64 = body.Substring(body.IndexOf(':') + 3, body.LastIndexOf('}') - 1 - (body.IndexOf(':') + 3));
            }

        }
        catch
        { }

        bool isEncoded = IsBase64String(b64);
        string decoded_body;
        if (isEncoded)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(b64);
            decoded_body = Encoding.UTF8.GetString(base64EncodedBytes);

            if (decoded_body.StartsWith("{"))
            {
                _logger.Information("Decoded request body is valid JSON");
                contentType = "json";

            }
            else if (decoded_body.StartsWith("<"))
            {
                _logger.Information("Decoded request body is valid XML");
                contentType = "xml";
            }
            else
            {
                _logger.Warning("Decoded request body is not valid JSON or XML");
            }
        }
        else
        {
            decoded_body = body;
        }

        return new RequestBody() { content = decoded_body, type = contentType ,body= b64 };
    }

    private async Task Cleanup()
    {
        // run cleanup (delete old requests)
        _logger.Information("Cleaning up Requests");

        var daysToKeep = configuration.GetValue<int>("Config:DaysToKeep", 30);
        await dbContext.Database.ExecuteSqlRawAsync("EXEC dbo.spCleanup @p0", new object[] { daysToKeep });
    }
    private async Task SyncTrans(Guid? requestid)
    {
        // run cleanup (delete old requests)
        _logger.Information("marking Original Request as Void");

        await dbContext.Database.ExecuteSqlRawAsync(@"update TransactionResponse set PS_ID=-2 where TR_ID=(select TR_ID from TransactionResponse where TR_RequestID=@p0)", new object[] { requestid });
    }

    [HttpPost]
    [Route("Test")]
    [Consumes("application/xml", "text/xml", "application/json")]
    public async Task<IActionResult> Test()
    {
        try
        {

            _logger.Information("Request Recieved");
            RequestBody body = await GetTypeAndBody();


            await Cleanup();




            TransactionResponse transactionResponse;

            if (body.type.Contains("xml"))
            {
                _logger.Information("Received XML request body: {RequestBody}", body);
                transactionResponse = await ProcessXmlRequest(body.content);
            }
            else if (body.type.Contains("json"))
            {

                _logger.Information("Received JSON request body: {RequestBody}", body);
                transactionResponse = await ProcessJsonRequest(body.content);
            }
            else
            {
                _logger.Warning("Unsupported content type received: {ContentType}", body.type);
                //logger.LogWarning("Unsupported content type received: {ContentType}", contentType);
                return BadRequest("Unsupported content type. Use application/xml or application/json.");
            }

            dbContext.TransactionResponses.Add(transactionResponse);
            //await dbContext.SaveChangesAsync();


            _logger.Information("Transaction {TransactionId} processed successfully for RequestID {RequestId}",
               transactionResponse.TR_Id, transactionResponse.TR_RequestID);

            return Ok(JsonConvert.SerializeObject(transactionResponse));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing transaction: {ErrorMessage}", ex.Message);
            if (ex.InnerException != null)
                return StatusCode(500, $"An error occurred while processing the transaction. Reason:{ex.InnerException.Message}");
            else
                return StatusCode(500, $"An error occurred while processing the transaction. Reason:{ex.Message}");

        }



    }

    List<string> MainKeys = new List<string>(); //{ "ApplicationID", "RequestID", "Mode", "TransactionIndex", "Acquirer", "Status" };

    private async Task<TransactionResponse> ProcessXmlRequest(string xmlContent)
    {
        var serializer = new XmlSerializer(typeof(VXmlRequest));
        //using var reader = new StreamReader(Request.Body);
        //var xmlContent = await reader.ReadToEndAsync();

        //logger.LogInformation("Received XML request body: {RequestBody}", xmlContent);

        using var stringReader = new StringReader(xmlContent);
        var vxml = (VXmlRequest?)serializer.Deserialize(stringReader)
            ?? throw new InvalidOperationException("Failed to deserialize XML request");

        Dictionary<string, string> xmlres = ResponseParser.ParseXmlResponse(xmlContent);



        xmlres.TryGetValue("ApplicationID", out var appid);
        xmlres.TryGetValue("RequestID", out var requestid);
        xmlres.TryGetValue("Mode", out var mode);
        xmlres.TryGetValue("TransactionIndex", out var transactionIndex);
        xmlres.TryGetValue("Acquirer", out var Aquirer);
        xmlres.TryGetValue("Result.Status", out var status);
        xmlres.TryGetValue("AcquirerReference", out var cycle);
        xmlres.TryGetValue("Command", out var command);
        xmlres.TryGetValue("OriginalRequestID", out var origionalrequest);

        //check request
        if (requestid == origionalrequest)
        {
            requestid = Guid.NewGuid().ToString();
        }

        if (cycle != null)
        {
            if (cycle.Contains(':'))
            {
                cycle = cycle.Substring(0, cycle.IndexOf(':'));
            }
            else
            {
                cycle = null;
            }
        }

        //if (status == "0" && command == "Void")

        //{

        //    status = "-2";

        //}


        TransactionResponse response = CreateTransactionResponse(
             appid, requestid, mode, transactionIndex, Aquirer, Convert.ToInt32(status), cycle,command,origionalrequest);


        foreach (KeyValuePair<string, string> val in xmlres)
        {
            if (!MainKeys.Contains(val.Key))
            {
                AddNameValueIfNotNull(response, val.Key, val.Value);
            }
        }


        return response;
    }

    private async Task<TransactionResponse> ProcessJsonRequest(string jsonContent)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
        var vjson = System.Text.Json.JsonSerializer.Deserialize<VJsonRequest>(jsonContent, options)
            ?? throw new InvalidOperationException("Failed to deserialize JSON request");

        Dictionary<string, string> jsonres = ResponseParser.ParseJsonResponse(jsonContent);




        jsonres.TryGetValue("ApplicationID", out var appid);
        jsonres.TryGetValue("RequestID", out var requestid);
        jsonres.TryGetValue("Mode", out var mode);
        jsonres.TryGetValue("TransactionIndex", out var transactionIndex);
        jsonres.TryGetValue("Acquirer", out var Aquirer);
        jsonres.TryGetValue("Result.Status", out var status);
        jsonres.TryGetValue("AcquirerReference", out var cycle);
        jsonres.TryGetValue("Command", out var command);
        jsonres.TryGetValue("OriginalRequestID", out var origionalrequest);

        //check request
        if(requestid == origionalrequest)
        {
            requestid = Guid.NewGuid().ToString();
        }


        if (cycle != null)
        {
            if (cycle.Contains(':'))
            {
                cycle = cycle.Substring(0, cycle.IndexOf(':'));
            }
            else
            {
                cycle = null;
            }
        }

        //if (status == "0" && command == "Void")

        //{

        //    status = "-2";

        //}

        TransactionResponse response = CreateTransactionResponse(
             appid, requestid, mode, transactionIndex, Aquirer, Convert.ToInt32(status), cycle,command,origionalrequest);


        foreach (KeyValuePair<string, string> val in jsonres)
        {
            if (!MainKeys.Contains(val.Key))
            {
                AddNameValueIfNotNull(response, val.Key, val.Value);
            }
        }


        return response;
    }




    private static TransactionResponse CreateTransactionResponse(
        string applicationId, string requestId, string mode, string? transactionIndex, string? acquirer, int status, string? cycle, string? command, string? originalRequestId)
    {
        return new TransactionResponse
        {
            TR_ApplicationID = ParseGuid(applicationId),
            TR_RequestID = ParseGuid(requestId),
            TR_Mode = mode.Length > 4 ? mode[..4] : mode,
            TR_TransactionIndex = string.IsNullOrEmpty(transactionIndex) ? null : ParseGuid(transactionIndex),
            TR_Acquirer = acquirer?.Length > 64 ? acquirer[..64] : acquirer,
            TR_ProcessingTime = DateTime.Now,//DateTime.UtcNow,
            PS_Id = status >= 0 ? 0 : -1,
            TR_AcquirerCycle = cycle,
            TR_Command = command,
            TR_OriginalRequestID = string.IsNullOrEmpty(originalRequestId) ? null : ParseGuid(originalRequestId)
        };
    }



    private static void AddNameValueIfNotNull(TransactionResponse response, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            response.NameValues.Add(new TransactionResponseNameValue
            {
                TRNV_Name = name,
                TRNV_Value = value.Length > 2048 ? value[..2048] : value
            });
        }
    }

    private static Guid ParseGuid(string? value)
    {
        // If missing or empty, generate a new GUID so records remain unique.
        if (string.IsNullOrWhiteSpace(value))
        {
            return Guid.NewGuid();
        }

        var cleanValue = value.Trim('{', '}');
        if (string.IsNullOrWhiteSpace(cleanValue))
        {
            return Guid.NewGuid();
        }

        try
        {
            return Guid.Parse(cleanValue);
        }
        catch
        {
            // If parsing fails, return a new GUID instead of throwing.
            return Guid.NewGuid();
        }
    }

    private static bool IsBase64String(string base64)
    {
        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
    }

    private class RequestBody
    {
        public string type { get; set; }
        public string content { get; set; }

        public string body { get; set; }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Depo.Components;
using Google.Apis.Drive.v2;
using Newtonsoft.Json.Linq;

namespace Depo.Controllers
{
    [Route("System/[controller]")]
    [ApiController]
    public class ReaderController : ControllerBase
    {
          public DriveService drive { get; set; }
        public ReaderController(DriveApiService _svc)
        {
            drive = _svc.service;
        }
        [HttpGet, DisableRequestSizeLimit]
        public async Task<ActionResult> ReaderBridge([FromQuery(Name = "data")] string fileid)
        {

            if (!string.IsNullOrEmpty(fileid))
            {
                Console.WriteLine($"{DateTime.Now} PDF İsteği: {Response.HttpContext.Connection.RemoteIpAddress}");
                var decoded = Base64.Base64Decode(fileid);
                var file = drive.Files.Get(decoded);
                file.SupportsAllDrives = true;
                file.SupportsTeamDrives = true;
                var fileinf = await file.ExecuteAsync();
                var cli = new HttpClient(drive.HttpClient.MessageHandler);
                
                var req = new HttpRequestMessage(HttpMethod.Get, $"https://www.googleapis.com/drive/v3/files/{decoded}?alt=media&acknowledgeAbuse=true");
            
                var resp = await cli.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                var contenttype = resp.Content.Headers.ContentType.MediaType;
                if (contenttype == "application/json")
                {
                    var message = JObject.Parse(resp.Content.ReadAsStringAsync().Result).SelectToken("error.message");

                    if (message.ToString() == "The download quota for this file has been exceeded.")
                    {
                        Console.WriteLine($"Can't return PDF Reason: Quota {Response.HttpContext.Connection.RemoteIpAddress}");
                        return Content("Error with getting PDF!, Reason: Quota");

                    }
                    else
                    {
                        Console.WriteLine($"Can't return PDF Reason: Drive Json Response {Response.HttpContext.Connection.RemoteIpAddress}");
                        return Content("Error with getting PDF!, Reason: Drive Json Response");
                    }
                }
                else
                {
                    string resplength = resp.Content.Headers.ContentLength.ToString();
                    if (!string.IsNullOrEmpty(resplength))
                    {
                        Response.Headers.Add("Content-Length", resplength);
                    }
                    return File(await resp.Content.ReadAsStreamAsync(), "application/pdf", fileinf.OriginalFilename, true);

                      
                   
                }


            }
            else
            {
                Console.WriteLine(
                    $"Can't return PDF Reason: Data Blank {Response.HttpContext.Connection.RemoteIpAddress}");
                return Content("Error with getting PDF!, Reason: data blank");
            }
            // }

        }
    }
}

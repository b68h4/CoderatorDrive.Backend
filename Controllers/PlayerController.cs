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
    public class PlayerController : ControllerBase
    {
        public DriveService drive { get; set; }
        public PlayerController(DriveApiService _svc)
        {
            drive = _svc.service;
        }
        [HttpGet, DisableRequestSizeLimit]
        public async Task<ActionResult> PlayerBridge([FromQuery(Name = "data")] string fileid)
        {

            if (!string.IsNullOrEmpty(fileid))
            {
                Console.WriteLine($"{DateTime.Now} Video İsteği: {Response.HttpContext.Connection.RemoteIpAddress}");
                var decoded = Base64.Base64Decode(fileid);
                var file = drive.Files.Get(decoded);
                file.SupportsAllDrives = true;
                file.SupportsTeamDrives = true;
                var fileinf = await file.ExecuteAsync();
                var cli = new HttpClient(drive.HttpClient.MessageHandler);
              
                var req = new HttpRequestMessage(HttpMethod.Get, $"https://www.googleapis.com/drive/v3/files/{decoded}?alt=media&acknowledgeAbuse=true");
                var range = Request.Headers["Range"];
                if (!string.IsNullOrEmpty(range))
                {
                    req.Headers.Add("Range", range.ToString());
                }
                var resp = await cli.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                var contenttype = resp.Content.Headers.ContentType.MediaType;
                if (contenttype == "application/json")
                {
                    var message = JObject.Parse(resp.Content.ReadAsStringAsync().Result).SelectToken("error.message");

                    if (message.ToString() == "The download quota for this file has been exceeded.")
                    {
                        Console.WriteLine($"Can't return media Reason: Quota {Response.HttpContext.Connection.RemoteIpAddress}");
                        return Content("Error with getting media!, Reason: Quota");

                    }
                    else
                    {
                        Console.WriteLine($"Can't return media Reason: Drive Json Response {Response.HttpContext.Connection.RemoteIpAddress}");
                        return Content("Error with getting media!, Reason: Drive Json Response");
                    }
                }
                else
                {
                    string resplength = resp.Content.Headers.ContentLength.ToString();
                    if (!string.IsNullOrEmpty(range))
                    {
                        string resprange = resp.Content.Headers.GetValues("Content-Range").FirstOrDefault();

                        Response.StatusCode = 206;
                        Response.Headers.Add("Content-Range", resprange);
                        if (!string.IsNullOrEmpty(resplength))
                        {
                            Response.Headers.Add("Content-Length", resplength);
                        }

                        return new FileStreamResult(await resp.Content.ReadAsStreamAsync(), fileinf.MimeType) { EnableRangeProcessing = true };
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(resplength))
                        {
                            Response.Headers.Add("Content-Length", resplength);
                        }
                        return File(await resp.Content.ReadAsStreamAsync(), fileinf.MimeType, fileinf.OriginalFilename, true);
                    }

                }


            }
            else
            {
                Console.WriteLine(
                    $"Can't return media Reason: Data Blank {Response.HttpContext.Connection.RemoteIpAddress}");
                return Content("Error with getting media!, Reason: data blank");
            }
            // }

        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Depo.Components;
using Depo.Models;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;

namespace Depo.Controllers
{
    [Route("System")]
    [ApiController]
    public class MainController : ControllerBase
    {
        public DriveService svc { get; set; }
        public DownloadStorage stor { get; set; }
        public Cache cache { get; set; }
        public MainController(DriveApiService _svc, DownloadStorage _stor,Cache _cache)
        {
            svc = _svc.service;
            stor = _stor;
            cache = _cache;
        }
        [HttpGet("List")]
        public async Task<string> List([FromQuery(Name = "id")] string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                id = Base64.Base64Decode(id);
                List<DriveResult> result = null;
                if (await cache.GetCache(id) != null)
                {
                    result = await cache.GetCache(id);
                }
                else
                {
                    var req = svc.Files.List();
                    // req.IncludeItemsFromAllDrives = true;
                    req.SupportsAllDrives = true;
                    req.IncludeTeamDriveItems = true;
                    req.OrderBy = "folder,title";
                    req.Corpora = "drive";
                    req.DriveId = "DRIVEFOLDERID";
                    req.MaxResults = 30000;
                    //gettd.Q = $"'ANOTHERTESTFOLDERID' in parents";
                
                    req.Q = $"'{id}' in parents and trashed=false";
                    var reqResult = await req.ExecuteAsync();
                    var filtResult = new List<DriveResult>();

                    foreach (var item in reqResult.Items)
                    {

                   
                        filtResult.Add(new DriveResult()
                        {
                            Id = Base64.Base64Encode(item.Id),
                            ModTime = item.ModifiedDate.ToString(),
                            Name = item.Title,
                            Size = FormatLength(item.FileSize),
                            MimeType = item.MimeType
                        });
                    }

                    await cache.CreateCache(id, filtResult);
                    result = filtResult;
                }
             
                return JsonConvert.SerializeObject(new
                {
                    Items = result
                });
            }
            else
            {
                return "";
            }

           

        }

        [HttpGet("Query")]
        public async Task<string> Query([FromQuery(Name = "id")] string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                id = Base64.Base64Decode(id);
                var req = svc.Files.Get(id);
                req.SupportsTeamDrives = true;
                req.SupportsAllDrives = true;
                req.AcknowledgeAbuse = true;
                var result = await req.ExecuteAsync();
                
                return JsonConvert.SerializeObject(new DriveResult()
                {
                    Id = Base64.Base64Encode(result.Id),
                    ModTime = result.ModifiedDate.ToString(),
                    Name = result.Title,
                    Size = result.FileSize.ToString(),
                    MimeType = result.MimeType
                });
            }
            else
            {
                return "";
            }
      
        }
        [HttpGet("CreateToken")]
        public async Task<string> CreateToken([FromQuery(Name = "id")] string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return await stor.CreateToken(Base64.Base64Decode(id));
            }
            else
            {
                return "";
            }

        }
        [HttpGet("Roots")]
        public string QueryRoots()
        {
            return System.IO.File.ReadAllText("roots.json");

        }
        public string FormatLength(long? len)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };

            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }
    }
}

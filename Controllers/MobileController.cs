using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Depo.Models;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Telegram.Bot;

namespace Depo.Controllers
{
    [Route("System/[controller]")]
    [ApiController]
    public class MobileController : ControllerBase
    {
        public TelegramBotClient reportcli = new TelegramBotClient("BOTTOKEN");
        [HttpGet("Report")]
        public async Task<string> Report([FromQuery(Name = "value")] string report)
        {
            await reportcli.SendTextMessageAsync(CHATID, $"Yeni geri bildirim!\n{report}\n\nIP: {HttpContext.Connection.RemoteIpAddress}",
                disableNotification: true);
            return "success";
        }

        [HttpGet("DevQuest")]
        public async Task<string> DevQuest()
        {
            return await System.IO.File.ReadAllTextAsync("questions.json");
        }
    }
}

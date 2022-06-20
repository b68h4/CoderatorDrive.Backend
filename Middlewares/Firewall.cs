using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Depo.Components;
namespace Depo.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class Firewall
    {
        private readonly RequestDelegate _next;

        public Firewall(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext con)
        {
            string[] hostsprod = { "alloweddomain.com" };
         
            var hosts = hostsprod;
            var origin = con.Request.Headers["Origin"].ToString();
            var referer = con.Request.Headers["Referer"].ToString();
            var ipadr = con.Connection.RemoteIpAddress.ToString();

            if (hosts.Contains(con.Request.Host.Value) && hosts.Any((a) => origin.Contains(a)) || hosts.Any((b) => referer.Contains(b)))
            {

                await _next(con);
                Logger.WriteLine("FWLogger", con.Response.StatusCode.ToString(), ipadr, $"CF-Ray: {con.Request.Headers["CF-Ray"]} / CF-Connecting: {con.Request.Headers["CF-Connecting-IP"]} / X-Forwarded: {con.Request.Headers["X-Forwarded-For"]} / User-Agent: {con.Request.Headers["User-Agent"]} / Range: {con.Request.Headers["Range"]} / Referer: {referer}");
            }
            else
            {
                con.Response.StatusCode = 403;
                await con.Response.WriteAsync("Request Denied.");

                Logger.WriteLine("Firewall", con.Response.StatusCode.ToString(), ipadr, $"Request Denied.");

            }

        }
    }

}

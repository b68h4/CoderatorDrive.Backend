using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            string[] hostsprod = {"depo.coderator.net","aom.coderator.net"};
            string[] hostsdev = {"localhost:3933","localhost","depo.coderator.net","aom.coderator.net","10.16.1.5","10.16.1.5:3933","192.168.1.22","192.168.1.22:3933"};
            var hosts = hostsdev;
            var origin = con.Request.Headers["Origin"].ToString();
            var referer = con.Request.Headers["Referer"].ToString();

            if (hosts.Contains(con.Request.Host.Value) && hosts.Any((a) => origin.Contains(a)) || hosts.Any((b) => referer.Contains(b)))
            {
                
                await _next(con);
            }
            else
            {
                Console.WriteLine($"{DateTime.Now.ToString()} Request Denied {con.Connection.RemoteIpAddress.ToString()}");
                await con.Response.WriteAsync("Request Denied.");
            }
           
        }
    }
}

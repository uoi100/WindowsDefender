using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;

namespace WindowsDefender_WebApp
{
    public class User
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public string MatchId { get; set; }
        public bool   Ready { get; set; }
        public string IpAddress { get; set; }

        public User(HubCallerContext context)
        {
            IpAddress = GetIpAddress(context);
        }

        private string GetIpAddress(HubCallerContext context)
        {
            string ipAddress;
            object tempObject;

            context.Request.Environment.TryGetValue("server.RemoteIpAddress", out tempObject);

            if (tempObject != null)
            {
                ipAddress = (string)tempObject;
            }
            else
            {
                ipAddress = "";
            }

            return ipAddress;
        }
    }
}
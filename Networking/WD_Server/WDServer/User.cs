using System;
using System.Net;
using WDServer;

namespace WindowsDefender_WebApp
{
    public class User
    {
        private object  _lock = new object();
        public IPEndPoint  EndPoint { get; set; }
        public string    Username { get; set; }
        public string    MatchId { get; set; }
        private int     _timeout = 0;

        public void IncrementTimeout()
        {
            lock (_lock)
                _timeout++;
        }

        public bool IsConnectionExpired()
        {
            lock (_lock)
            {
                if (_timeout > Server.CLIENT_TIMEOUT)
                {
                    return true;
                }
            }
            return false;
        }

        public void ResetTimeout()
        {
            lock (_lock)
                _timeout = 0;
        }
    }
}
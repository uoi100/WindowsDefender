using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace WindowsDefender_WebApp
{
    public class Match
    {
        public string Id { get; set; }
        public string HostId { get; set; } = null;

        public ArrayList Users = new ArrayList();
        private object _usersLock = new object();

        /// <summary>
        /// Creates a unique GUID for the match
        /// </summary>
        public Match()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Adds a user to the match.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool AddUser(User user)
        {
            lock (_usersLock)
            {
                if (Users.Count >= 4)
                    return false;

                Users.Add(user);

                if (HostId == null)
                    HostId = user.ConnectionId;

                return true;
            }
        }

        /// <summary>
        /// Remove user from match and re-assign the match host if needed.
        /// </summary>
        /// <param name="user"></param>
        public void RemoveUser(User user)
        {
            lock (_usersLock)
            {
                Users.Remove(user);

                // If this is the host, re-assign another user as the match host
                if (HostId == user.ConnectionId && Users.Count > 0)
                    HostId = ((User)Users[0]).ConnectionId;
            }
        }
    }
}
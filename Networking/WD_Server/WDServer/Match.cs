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
        public string ID { get; set; }

        public ArrayList Users = new ArrayList();
        private object _usersLock = new object();

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
            }
        }
    }
}
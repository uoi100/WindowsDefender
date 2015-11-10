using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections;
using System.Web.Script.Serialization;

namespace WindowsDefender_WebApp
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, User> _users = new ConcurrentDictionary<string, User>();
        private static ConcurrentDictionary<string, Match> _matches = new ConcurrentDictionary<string, Match>();

        string msg_icon = "<span class=\"glyphicon glyphicon-info-sign server-message-icon\"></span> ";
        string rdy_icon = "<span class=\"glyphicon glyphicon-ok lobby-ready-icon\"></span> ";
        string not_rdy_icon = "<span class=\"glyphicon glyphicon-remove lobby-notready-icon\"></span> ";

        /// <summary>
        /// New user has connected.
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            CreateUser();
            FindMatch();
            return base.OnConnected();
        }

        /// <summary>
        /// Creates a new user in the _users dictionary.
        /// </summary>
        private void CreateUser()
        {
            // Create
            User user = new User(this.Context)
            {
                ConnectionId = Context.ConnectionId,
                Name = Context.User.Identity.Name
            };
            // Add to collection
            _users.TryAdd(Context.ConnectionId, user);
        }

        /// <summary>
        /// Finds a match for the user.
        /// </summary>
        private void FindMatch()
        {
            User user = null;
            _users.TryGetValue(Context.ConnectionId, out user);
            if (user == null) return;

            Match match = null;
            bool matchFound = false;
            foreach (KeyValuePair<string, Match> existingMatch in _matches)
            {
                match = existingMatch.Value;

                // If a match has been found
                if (match.AddUser(user))
                {
                    user.MatchId = match.Id;
                    matchFound = true;
                    break;
                }
            }

            // If no match was found, create a new one
            if (!matchFound)
            {
                match = CreateMatch();
                match.AddUser(user);
                user.MatchId = match.Id;
            }

            // Notify users
            SendToMatchExcept(user.MatchId, "<b>" + msg_icon + user.Name + " has joined the game.</b>");
            SendToUser(user.ConnectionId, "<b>" + msg_icon + "You have joined a game.</b>");

            // Update match user list
            UpdateMatchUserLists(match);

            // Update match ready count
            UpdateReadyCount(match);
        }

        /// <summary>
        /// Creates an new empty match.
        /// </summary>
        /// <returns></returns>
        private Match CreateMatch()
        {
            // Create an empty match
            Match match = new Match();
            _matches.TryAdd(match.Id, match);

            return match;
        }

        /// <summary>
        /// Sends a message to everyone in a match,
        /// including the user who sent it.
        /// </summary>
        /// <param name="message"></param>
        public void SendToMatch(string message)
        {
            // Get user
            User user = null;
            _users.TryGetValue(Context.ConnectionId, out user);
            if (user == null)
                return;

            // Send to everyone in the match
            SendToMatchExcept(user.MatchId, message);
            SendToUser(user.ConnectionId, message);
        }
        
        /// <summary>
        /// Sends to everyone in a match except the current user.
        /// </summary>
        /// <param name="message"></param>
        private void SendToMatchExcept(string matchId, string message)
        {
            // Get user
            User user = null;
            _users.TryGetValue(Context.ConnectionId, out user);
            if (user == null)
                return;
            
            // Get match
            Match match = null;
            _matches.TryGetValue(matchId, out match);
            if (match == null)
                return;

            // Send to each user
            foreach (User matchUser in match.Users)
            {
                if (matchUser.ConnectionId != Context.ConnectionId)
                    SendToUser(matchUser.ConnectionId, message);
            }
        }

        /// <summary>
        /// Sends a message to a single user.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="message"></param>
        public void SendToUser(string connectionId, string message)
        {
            Clients.Client(connectionId).send(message);
        }

        /// <summary>
        /// Sends a chat message to everyone in the user's match with format: 'Name: message'
        /// </summary>
        /// <param name="message"></param>
        public void SendChatMessage(string message)
        {
            User user = null;
            _users.TryGetValue(Context.ConnectionId, out user);
            if (user == null)
                return;
            SendToMatch(user.Name + ": " + message);
        }

        /// <summary>
        /// Sends a chat message to everyone in the user's match with format: 'Name: message'
        /// </summary>
        /// <param name="message"></param>
        public void CancelLaunch()
        {
            User user = null;
            _users.TryGetValue(Context.ConnectionId, out user);
            if (user == null)
                return;

            // Get match
            Match match = null;
            _matches.TryGetValue(user.MatchId, out match);

            // Send cancel launch command to all users
            foreach (User u in match.Users)
                Clients.Client(u.ConnectionId).cancelLaunch();
        }

        /// <summary>
        /// Sends the current userlist to everyone in a match.
        /// </summary>
        /// <param name="match"></param>
        public void UpdateMatchUserLists(Match match)
        {
            // Convert userlist to JSON
            string[] userlist = new string[match.Users.Count];
            for (int i = 0; i < match.Users.Count; i++)
                userlist[i] = ((User)match.Users[i]).Name;
            JavaScriptSerializer js = new JavaScriptSerializer();
            string json = js.Serialize(userlist);

            // Send JSON to each user in the match
            foreach (User user in match.Users)
                Clients.Client(user.ConnectionId).updateUserList(json);
        }

        /// <summary>
        /// Sends the current readylist to everyone in a match.
        /// </summary>
        /// <param name="match"></param>
        public void UpdateReadyCount(Match match)
        {
            int readyCount = 0;
            foreach (User user in match.Users)
                if (user.Ready)
                    readyCount++;
            foreach (User user in match.Users)
                Clients.Client(user.ConnectionId).updateReadyCount(readyCount);
        }

        /// <summary>
        /// When the user presses the Ready button.
        /// </summary>
        public void Ready(bool isReady)
        {
            // Get user
            User user = null;
            _users.TryGetValue(Context.ConnectionId, out user);
            if (user == null)
                return;

            // Set if user is ready or not
            user.Ready = isReady;

            // Send message
            if (isReady)
                SendToMatch("<b>" + rdy_icon + user.Name + " is ready.</b>");
            else
                SendToMatch("<b>" + not_rdy_icon + user.Name + " is no longer ready.</b>");

            // Get match
            Match match = null;
            _matches.TryGetValue(user.MatchId, out match);

            // Send ready count to each user (0-4). Just so the user sees '0/4 players are ready'.
            UpdateReadyCount(match);

            // Launch match if everyone is ready
            if (match.Users.Count == 4) {
                if (isReady)
                {
                    bool allReady = true;
                    foreach (User u in match.Users)
                    {
                        if (!u.Ready)
                            allReady = false;
                    }

                    if (allReady)
                    {
                        SendToMatch("<b>" + rdy_icon + "The match is about to begin...</b>");

                        // Send launch game command with host ip address
                        User host = null;
                        _users.TryGetValue(match.HostId, out host);

                        // Send launch command to all users
                        foreach (User u in match.Users)
                        {
                            if (host.IpAddress == "::1")
                                host.IpAddress = "127.0.0.1";
                            Clients.Client(u.ConnectionId).launchGame(host.IpAddress);
                        }
                    }
                }
                else
                {
                    // If user left during launch countdown
                    foreach (User u in match.Users)
                        Clients.Client(user.ConnectionId).cancelLaunch();

                    SendToMatch("<b>" + rdy_icon + "Launch cancelled. Waiting for more players...</b>");
                }
            }
        }



        /// <summary>
        /// User has disconnected.
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            User user = null;
            Match match = null;

            try
            {
                // Get user
                _users.TryGetValue(Context.ConnectionId, out user);

                // Make sure user exists
                if (user == null)
                    throw new ArgumentNullException("user", "User does not exist in _users collection.");

                // Make sure user is part of a match
                if (user.MatchId == null)
                    throw new ArgumentNullException("user.MatchId", "User does not have a match id.");

                // Get match
                _matches.TryGetValue(user.MatchId, out match);
                if (match == null)
                    throw new ArgumentNullException("user.MatchId", "Match does not exist in _matches collection.");

                // Notify users
                SendToMatchExcept(user.MatchId, "<b>" + msg_icon + user.Name + " has left the game.</b>");

                // Remove user from match
                match.RemoveUser(user);

                // Update match's user list
                UpdateMatchUserLists(match);

                // Update match ready count
                UpdateReadyCount(match);

                // If match is now empty, close the match entirely
                if (match.Users.Count == 0)
                {
                    Match dummy = null;
                    _matches.TryRemove(match.Id, out dummy);
                }
            }
            catch (ArgumentNullException e) {
                Debug.WriteLine(e.ParamName + ": " + e.Message);
            }

            // Remove user
            User userdummy = null;
            if (user != null)
                _users.TryRemove(user.ConnectionId, out userdummy);

            return base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// User has reconnected.
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {
            // TODO
            return base.OnReconnected();
        }
    }
}

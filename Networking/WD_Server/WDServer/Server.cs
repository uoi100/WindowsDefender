using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using WindowsDefender_WebApp;

namespace WDServer
{
    class Server
    { 
        // Set this to false for Release builds so we aren't wasting cycles by printing to the console
        public static bool DEBUG = true;

        private int _port = 25001;
        public static int CLIENT_TIMEOUT = 15; // Seconds

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 25001);
        UdpClient newsock;

        private ConcurrentDictionary<string, User> _users = new ConcurrentDictionary<string, User>(); // Keys are IP address' of users
        private ConcurrentDictionary<string, Match> _matches = new ConcurrentDictionary<string, Match>();

        // Keeps track of user timeouts (heartbeats)
        private Thread _checkForTimeouts;

        /// <summary>
        /// Entry point.
        /// </summary>
        public Server()
        {
            // Initialize server
            Init();

            // Create thread that continuously checks for timed out users
            _checkForTimeouts = new Thread(new ThreadStart(TimeoutCheckerThread));
            _checkForTimeouts.Start();

            // Start listening
            Listen();
        }

        /// <summary>
        /// Initializes the server socket and tells the user if everything worked.
        /// </summary>
        private void Init()
        {
            try
            {
                Console.WriteLine("  _____                     ____      ___           _       ");
                Console.WriteLine(" |_   _|___ _ _ _ ___ ___  |    \\ ___|  _|___ _____| |___ ___");
                Console.WriteLine("   | | | . | | | | -_|  _| |  |  | -_|  _| -_|   | . | -_|  _|");
                Console.WriteLine("   |_| |___|_____|___|_|   |____/|___|_| |___|_|_|___|___|_|");

                newsock = new UdpClient(ipep);
              
                Console.WriteLine("\n Server started using port " + _port + "\n\n");
            }
            catch (SocketException se)
            {
                Console.WriteLine("Error starting server: " + se.ToString());
                Console.WriteLine("Press enter to quit.");
                Console.In.Read();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Listens for packets and responds accordingly by passing
        /// the incoming data to other functions.
        /// </summary>
        private void Listen()
        {
            while (true)
            {
                try
                {
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                    // Block and wait for incoming data
                    byte[] received = new byte[512];
                    received = newsock.Receive(ref sender);

                    if (DEBUG)
                        Console.WriteLine("Data recieved");

                    // Data received, remove trailing nulls

                    int i = received.Length - 1;
                    while (received[i] == 0)
                        --i;
                    byte[] data = new byte[i + 1];
                    Array.Copy(received, data, i + 1);

                    // Get ip address of sender
                    string ipAddress = sender.Address.ToString();

                    // Send back test data
                    if (DEBUG)
                    {
                        Instruction echo = new Instruction()
                        {
                            Command = Instruction.Type.CMD,
                            Arg1 = "DEBUG: SIR, I HAVE I RECEIVED YOUR DATA. - Server",
                            Arg2 = "TEST"
                        };
                        byte[] echobytes = Serializer.Serialize(echo);
                        newsock.Send(echobytes, echobytes.Length, sender);
                    }

                    // Deserialize received instruction
                    Instruction instruction = Serializer.DeSerialize(data);

                    // Debug
                    if (DEBUG)
                    {
                        Console.WriteLine("DEBUG: Received command: " + instruction.Command + " from ip: "
                            + ipAddress + "." + " with arguments: " + instruction.Arg1 + "/" + instruction.Arg2
                            + "/" + instruction.Arg3 + "/" + instruction.Arg4);
                    }

                    // Reset the user's timeout counter
                    ResetUserTimeout(ipAddress);

                    // Parse instruction
                    switch (instruction.Command)
                    {
                        case Instruction.Type.JOIN:
                            OnJoin(sender, ipAddress, instruction);
                            break;

                        case Instruction.Type.LEAVE:
                            OnDisconnect(ipAddress);
                            break;

                        case Instruction.Type.CMD:
                            OnCommand(ipAddress, instruction, data);
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// When the JOIN command is received by a user.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="ipAddress"></param>
        /// <param name="instruction"></param>
        private void OnJoin(IPEndPoint remoteEndPoint, string ipAddress, Instruction instruction)
        {
            User user = new User()
            {
                EndPoint = remoteEndPoint
            };

            // Set username
            user.Username = instruction.Arg1;

            // Set match id
            user.MatchId = instruction.Arg2;

            // Add user
            _users.TryAdd(ipAddress, user);

            // Find the user's match and add him/her to it
            AddToMatch(user);

            Instruction i = new Instruction() { Command = Instruction.Type.JOINED };
            byte[] data = Serializer.Serialize(i);
            newsock.Send(data, data.Length, user.EndPoint);
        }

        /// <summary>
        /// When a command instruction is received it is simply
        /// echoed to everyone in the same match as the user.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="instruction"></param>
        private void OnCommand(string ipAddress, Instruction instruction, byte[] data)
        {
            User user;

            if (!_users.TryGetValue(ipAddress, out user))
            {
                return;
            }

            SendToMatch(user, data);
        }

        /// <summary>
        /// Adds a user to his/her Match.
        /// If it doesn't exist yet then it will be created.
        /// </summary>
        /// <param name="user"></param>
        private void AddToMatch(User user)
        {
            Match match;
            if (_matches.TryGetValue(user.MatchId, out match))
            {
                match.AddUser(user);
                if (DEBUG)
                    Console.WriteLine("DEBUG: Added " + user.Username + " to match");
            }
            else
            {
                match = new Match()
                {
                    ID = user.MatchId
                };
                match.AddUser(user);
                _matches.TryAdd(user.MatchId, match);
                if (DEBUG)
                    Console.WriteLine("DEBUG: Created new match and added " + user.Username + " to it.");
            }
        }

        /// <summary>
        /// Sends to all users in a match.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="instruction"></param>
        private void SendToMatch(User user, byte[] data)
        {
            // Find match
            Match match;
            if (!_matches.TryGetValue(user.MatchId, out match))
                return;

            // Send to every user in the match
            foreach (KeyValuePair<string, User> u in match.Users)
                newsock.Send(data, data.Length, u.Value.EndPoint);
        }

        /// <summary>
        /// Sends to all users in a match except the current user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="instruction"></param>
        private void SendToMatchExcept(User user, byte[] data)
        {
            // TODO
            // Find match
            // Send instruction to everyone in match except current user
        }

        /// <summary>
        /// If we haven't received messages from users for awhile,
        /// we consider them disconnected and they can be removed
        /// from any match they were a part of.
        /// </summary>
        private void TimeoutCheckerThread()
        {
            while (true)
            {
                foreach (KeyValuePair<string, User> user in _users)
                {
                    user.Value.IncrementTimeout();
                    if (user.Value.IsConnectionExpired())
                    {
                        // Disconnect user
                        OnDisconnect(user.Key);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Resets a user's timeout (heartbeat counter).
        /// </summary>
        /// <param name="ipAddress"></param>
        private void ResetUserTimeout(string ipAddress)
        {
            User user;
            if (_users.TryGetValue(ipAddress, out user))
                user.ResetTimeout();
        }

        /// <summary>
        /// Disconnects a user based on their ip address.
        /// </summary>
        /// <param name="ipaddress"></param>
        private void OnDisconnect(string ipAddress)
        {
            // Get user
            User user;
            if (!_users.TryGetValue(ipAddress, out user))
                return;

            // Remove user from match
            Match match;
            if (_matches.TryGetValue(user.MatchId, out match))
            {
                match.RemoveUser(user);
                // If user is last person in his/her match, remove the match completely
                if (match.Users.Count <= 0)
                {
                    Match dummyMatch;
                    _matches.TryRemove(match.ID, out dummyMatch);
                    if (DEBUG)
                        Console.WriteLine("DEBUG: Removed match because no more users are in it.");
                }
            }

            // Tell user they have been disconnected
            Instruction inactive = new Instruction()
            {
                Command = Instruction.Type.LEAVE
            };
            byte[] data = Serializer.Serialize(inactive);
            newsock.Send(data, data.Length, user.EndPoint);

            // Remove user
            User dummy;
            _users.TryRemove(user.EndPoint.Address.ToString(), out dummy);

            if (DEBUG && dummy != null)
                Console.WriteLine("DEBUG: " + user.Username + " disconnected.");
        }
    }
}

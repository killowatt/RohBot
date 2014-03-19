﻿using System.Linq;

namespace SteamMobile.Packets
{
    public class Authenticate : Packet
    {
        public override string Type
        {
            get { return "auth"; }
        }

        public string Method; // login/register/guest
        public string Username;
        public string Password;
        public string Tokens;

        public override void Handle(Connection connection)
        {
            var guest = false;

            switch (Method)
            {
                case "login":
                    if (Program.DelayManager.AddAndCheck(connection, 10))
                        break;

                    Program.Logger.InfoFormat("Login '{1}' from {0}", connection.Address, Username);
                    connection.Login(Username, Password, (Tokens ?? "").Split(',').ToList());
                    break;

                case "register":
                    if (Program.DelayManager.AddAndCheck(connection, 10))
                        break;

                    Program.Logger.InfoFormat("Register '{1}' from {0}", connection.Address, Username);
                    connection.Register(Username, Password);
                    break;

                case "guest":
                    if (Program.DelayManager.AddAndCheck(connection, 10))
                        break;

                    if (connection.Session != null)
                    {
                        connection.Session.Remove(connection);
                        connection.Session = null;
                    }

                    guest = true;
                    break;
            }

            if (connection.Session == null)
            {
                connection.Send(new AuthenticateResponse
                {
                    Name = null,
                    Tokens = "",
                    Success = guest
                });

                if (guest)
                {
                    var room = Program.RoomManager.Get(Program.Settings.DefaultRoom);
                    connection.SendJoinRoom(room);
                }
            }
        }
    }
}

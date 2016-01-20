using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace PokeBattle
{
    class Player
    {
        TcpClient client;
        NetworkStream stream;
        public Player()
        {
            Random rand = new Random();
            this.PokeTeam = new Pokemon[6];
            for (int i = 0; i < 6; i++)
                this.PokeTeam[i] = PokeBox.GetRandomPokemonByLevel(rand.Next(60, 81));
        }

        public Pokemon[] PokeTeam { get; private set; }
        
        public void Connect()
        {
            var server = new TcpListener(IPAddress.Loopback, 9073);
            server.Start();
            client = server.AcceptTcpClient();
            stream = client.GetStream();
        }

        public void Close()
        {
            client.Close();
            stream.Dispose();
        }

        public void WriteString(string message)
        {
            stream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
        }

        public void WritePokeTeam()
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            WriteString(serializer.Serialize(PokeTeam));
        }
    }
}

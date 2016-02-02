﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace PokeBattle
{
    class Player
    {
        TcpClient client;
        NetworkStream stream;
        JavaScriptSerializer serializer;

        public Player()
        {
            Random rand = new Random();
            this.PokeTeam = new Pokemon[6];
            for (int i = 0; i < 6; i++)
                this.PokeTeam[i] = PokeBox.GetRandomPokemonByLevel(rand.Next(60, 81));
            serializer = new JavaScriptSerializer();
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

        public void WriteLine(string message)
        {
            stream.Write(Encoding.ASCII.GetBytes(message + '\n'), 0, message.Length + 1);
            
        }

        public void WritePokeTeam()
        {
            WriteLine(serializer.Serialize(PokeTeam));
        }

        public void WritePokemon(Pokemon p)
        {
            WriteLine(serializer.Serialize(p));
        }
    }
}

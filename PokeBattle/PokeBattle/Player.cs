using System;
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
        int selectedPokemonIdx = 0;

        public Player()
        {
            Random rand = new Random();
            this.PokeTeam = new Pokemon[6];
            for (int i = 0; i < 6; i++)
                this.PokeTeam[i] = PokeBox.GetRandomPokemonByLevel(rand.Next(60, 81));
            serializer = new JavaScriptSerializer();
        }

        public Pokemon[] PokeTeam { get; private set; }

        public int SelectedPokemonIdx
        {
            get { return selectedPokemonIdx; }
            set
            {
                if (value >= 0 && value <= 6)
                    selectedPokemonIdx = value;
                else
                    throw new IndexOutOfRangeException();
            }
        }

        public Pokemon SelectedPokemon { get { return PokeTeam[selectedPokemonIdx]; } }

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

        private void WriteLine(string message, MessageType type)
        {
            if (stream != null) // temp for DEBUG
                stream.Write(Encoding.ASCII.GetBytes((char)type + message + '\n'), 0, message.Length + 2);
        }

        private void WriteMessageTypeOnly(MessageType type)
        {
            stream.WriteByte((byte)type);
        }

        public void WriteBeginTurn()
        {
            WriteMessageTypeOnly(MessageType.BeginTurn);
        }

        public void WriteText(string text)
        {
            WriteLine(text, MessageType.Text);
        }

        public void WritePokeTeam()
        {
            WriteLine(serializer.Serialize(PokeTeam), MessageType.PokeTeam);
        }

        public void WriteOpponent(Pokemon p)
        {
            WriteLine(serializer.Serialize(p), MessageType.ChangeOpponent);
        }

        public void WriteInBattle()
        {
            WriteLine(serializer.Serialize(SelectedPokemon.InBattle), MessageType.InBattleOpponent);
        }

        public void WriteInBattleOpponent(InBattleClass inBattle)
        {
            WriteLine(serializer.Serialize(inBattle), MessageType.InBattleOpponent);
        }

        public byte[] ReadMove()
        {
            byte[] buff = new byte[2];
            stream.Read(buff, 0, 2);
            return buff;
        }
    }

    enum MessageType : byte { Text, ChangeOpponent, PokeTeam, InBattleUser, InBattleOpponent, BeginTurn, UserFainted }
}

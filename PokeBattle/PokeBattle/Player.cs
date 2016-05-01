﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization;

namespace PokeBattle
{
    class Player
    {
        TcpClient _client;
        NetworkStream _stream;
        JavaScriptSerializer _serializer;

        public Player()
        {
            Random rand = new Random();
            PokeTeam = new Pokemon[6];
            for (int i = 0; i < 6; i++)
                PokeTeam[i] = PokeBox.GetRandomPokemonByLevel(rand.Next(60, 81));
            _serializer = new JavaScriptSerializer();
        }

        public Pokemon[] PokeTeam { get; }

        public bool Lost => PokeTeam.All(p => p.Fainted);

        int _selectedPokemonIdx = 0;
        public int SelectedPokemonIdx
        {
            get { return _selectedPokemonIdx; }
            set
            {
                if (value >= 0 && value < 6)
                    _selectedPokemonIdx = value;
                else
                    throw new IndexOutOfRangeException();
            }
        }

        public Pokemon SelectedPokemon => PokeTeam[_selectedPokemonIdx];

        public void Connect()
        {
            var server = new TcpListener(IPAddress.Loopback, 9073);
            server.Start();
            _client = server.AcceptTcpClient();
            _stream = _client.GetStream();
            server.Stop();
        }

        public void Close()
        {
            _client.Close();
            _stream.Dispose();
        }

        private void WriteBytes(byte[] array)
        {
            _stream?.Write(array, 0, array.Length);
        }

        private void WriteLine(string message, MessageType type)
        {
            WriteMessageType(type);
            WriteBytes(Encoding.ASCII.GetBytes(message + '\n'));
        }

        private void WriteMessageType(MessageType type)
        {
            _stream?.WriteByte((byte)type);
#if DEBUG
            Console.WriteLine(type.ToString());
#endif
        }

        public void SendBeginTurn() =>
            WriteMessageType(MessageType.BeginTurn);

        public void SendText(string text) =>
            WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(text)), MessageType.Text);

        public void SendPokeTeam() =>
            WriteLine(_serializer.Serialize(PokeTeam), MessageType.PokeTeam);

        public void SendOpponent(Pokemon p) =>
            WriteLine(_serializer.Serialize(p), MessageType.ChangeOpponent);

        public void SendInBattle() =>
            WriteLine(_serializer.Serialize(SelectedPokemon.InBattle), MessageType.InBattleUser);

        public void SendInBattleOpponent(InBattleClass inBattle) =>
            WriteLine(_serializer.Serialize(inBattle), MessageType.InBattleOpponent);

        public void SendUserFainted() =>
            WriteMessageType(MessageType.UserFainted);

        public void SendOpponentFainted() =>
            WriteMessageType(MessageType.OpponentFainted);

        // Each turn the player can either use a move (0), change (1) or don't change (2 - only when opponent's fainted) the active pokemon
        private ReadReturn ReadGeneric() => 
            new ReadReturn { Type = (ReadType)_stream.ReadByte(), Value = (byte)_stream.ReadByte() };

        public ReadReturn Read()
        {
            var r = ReadGeneric();
            if (r.Type == ReadType.NoSwitch)
                throw new Exception();
            return r;
        }

        public ReadReturn ReadSwitch()
        {
            var r = Read();
            if (r.Type == ReadType.Move)
                throw new Exception();
            return r;
        }
    }

    enum MessageType : byte { Text, ChangeOpponent, PokeTeam, InBattleUser, InBattleOpponent, BeginTurn, UserFainted, OpponentFainted }

    enum ReadType : byte { Move, Switch, NoSwitch }

    struct ReadReturn
    {
        public ReadType Type { get; set; }
        public byte Value { get; set; }
    }
}

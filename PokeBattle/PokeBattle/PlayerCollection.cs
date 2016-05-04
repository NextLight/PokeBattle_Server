using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PokeBattle
{
    class PlayerCollection : IEnumerable<Player>
    {
        Player[] _p;

        public PlayerCollection(params Player[] players)
        {
            _p = players;
            foreach (Player p in _p)
            {
                p.ClientConnected += P_ClientConnected;
                p.ClientDisconnected += P_ClientDisconnected;
            }
        }

        public Player this[int i] => _p[i];

        public int Length => _p.Length;

        public bool AreAllConnected => _p.All(p => p.IsConnected);


        public event EventHandler Connected;


        public async Task Connect()
        {
            foreach (Player p in _p)
                if (!p.IsConnected)
                    await p.Connect();
        }

        public void Close() => Execute();

        public void SendBeginTurn() => Execute();

        public void SendText(string text) => Execute(new[] { text });

        public void SendPokeTeam() => Execute();

        public void SendOpponent(Player opp) => Execute(new[] { opp.SelectedPokemon }, opp);

        public void SendInBattle() => Execute();

        public void SendInBattleOpponent(Player opp) => Execute(new[] { opp.SelectedPokemon.InBattle }, opp);

        public async Task PlayerFainted(Player p)
        {
            p.SendUserFainted();
            var s = await p.ReadSwitchAsync();
            if (s.Type != ReadType.Switch)
                throw new Exception();
            p.SelectedPokemonIdx = s.Value;
            Execute(null, p, "SendOpponentFainted");
            SendOpponent(p);
            foreach (var r in await ReadSwitch(p))
            {
                r.Key.SelectedPokemonIdx = r.Value;
                SendOpponent(r.Key);
            }
        }

        public IEnumerable<Task<ReadReturn>> Read() => _p.Select(p => p.ReadAsync());

        public async Task<IEnumerable<KeyValuePair<Player, byte>>> ReadSwitch(Player f) => // this is horrible
            (await Task.WhenAll(Filter(f).Select(p => new KeyValuePair<Player, Task<ReadReturn>>(p, p.ReadSwitchAsync()))
                .Select(async kv => new KeyValuePair<Player, ReadReturn>(kv.Key, await kv.Value)))
                ).Where(kv => kv.Value.Type == ReadType.Switch).Select(kv => new KeyValuePair<Player, byte>(kv.Key, kv.Value.Value));

        public void CancelRead() => Execute();

        private void P_ClientConnected(object sender, EventArgs e)
        {
#if DEBUG
            Console.WriteLine($"Player {_p.ToList().IndexOf((Player)sender)} connected.");
#endif
            if (AreAllConnected)
            {
                foreach (Player p in _p.Where(p => !p.IsInitialized))
                {
                    p.SendPokeTeam();
                    p.SendActivePokemon();
                    SendOpponent(Filter(p).First());
                    SendBeginTurn();
                }
                Connected?.Invoke(this, null);
            }
        }

        private void P_ClientDisconnected(object sender, EventArgs e)
        {
#if DEBUG
            Console.WriteLine($"Player {_p.ToList().IndexOf((Player)sender)} disconnected.");
#endif
            ((Player)sender).Connect();
        }

        private void Execute(object[] args = null, Player f = null, [CallerMemberName]string name = "")
        {
            MethodInfo method = typeof(Player).GetMethod(name);
            foreach (Player p in Filter(f))
                method.Invoke(p, args);
        }


        public IEnumerable<Player> Filter(Player p) =>
            p == null ? _p : _p.Where(pl => pl != p);

        public IEnumerator<Player> GetEnumerator() => ((IEnumerable<Player>)_p).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
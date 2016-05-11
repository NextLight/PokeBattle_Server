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
        }

        public Player this[int i] => _p[i];

        public int Length => _p.Length;

        public void Connect() => Execute();

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
                r.Key.SelectedPokemonIdx = r.Value;
        }

        public async Task<IEnumerable<ReadReturn>> Read() => await Task.WhenAll(_p.Select(p => p.ReadAsync()));

        public async Task<IEnumerable<KeyValuePair<Player, byte>>> ReadSwitch(Player f) => // this is horrible
            (await Task.WhenAll(Filter(f).Select(p => new KeyValuePair<Player, Task<ReadReturn>>(p, p.ReadSwitchAsync()))
                .Select(async kv => new KeyValuePair<Player, ReadReturn>(kv.Key, await kv.Value)))
                ).Where(kv => kv.Value.Type == ReadType.Switch).Select(kv => new KeyValuePair<Player, byte>(kv.Key, kv.Value.Value));



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

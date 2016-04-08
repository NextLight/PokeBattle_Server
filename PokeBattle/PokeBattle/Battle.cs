using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeBattle
{
    class Battle
    {
        Pokemon[] pokemons;
        Random rand;
        
        public Battle(Pokemon p1, Pokemon p2)
        {
            this.pokemons = new Pokemon[] { p1, p2 };
            rand = new Random();
        }

        public void ChangePokemon(int idx, Pokemon p)
        {
            pokemons[idx] = p;
        }

        List<Tuple<int, int>> _storedMoves = new List<Tuple<int, int>>();

        public void StoreMove(int p, int m)
        {
            _storedMoves.Add(new Tuple<int, int>(p, m));
        }

        public IEnumerable<Tuple<int, int>> ExecuteMoves()
        {
            foreach (var t in _storedMoves.OrderByDescending(x => pokemons[x.Item1].InBattle.Stats.Speed))
            {
                ExecuteMove(t.Item1, t.Item2);
                yield return t;
            }
            _storedMoves.Clear();
        }

        private void ExecuteMove(int p, int m)
        {
            Pokemon user = pokemons[p], opponent = pokemons[p ^ 1]; // eh xor, beware // TODO: change to something saner
            InBattleClass uBattle = user.InBattle, oBattle = opponent.InBattle;
            Move move = user.Moves[m];
            int hits = rand.Next(move.MinHits ?? 1, move.MaxHits ?? 1 + 1);
            for (int i = 0; i < hits; i++)
            {
                int stage = uBattle.Stats.AccuracyStage - oBattle.Stats.EvasionStage;
                stage = stage > 6 ? 6 : stage < -6 ? -6 : stage; // stage must be in range [-6,6]
                float mult = Math.Max(3, 3 + stage) / (float)Math.Max(3, 3 - stage);
                if ((move.Accuracy * mult ?? 100) >= rand.Next(101))
                {
                    if (move.IsOhko)
                    {
                        oBattle.Stats.Hp = 0;
                    }
                    else if (move.DamageClass != DamageClass.None && move.Power != null) 
                    {
                        int damage = CalculateDamage(user.Level, move.Power.Value,
                            move.DamageClass == DamageClass.Physical ? uBattle.Stats.Attack  : uBattle.Stats.SpecialAttack, 
                            move.DamageClass == DamageClass.Physical ? oBattle.Stats.Defense : oBattle.Stats.SpecialDefense
                        );
                        if (user.Types.Item1 == move.TypeId || user.Types.Item2 == move.TypeId) // stab
                            damage = damage * 3 / 2;
                        if (rand.Next(16 / (1 + move.CriticalRate)) == 0) // critical
                            damage = damage * 3 / 2;
                        damage = (int)(damage * PokeBox.TypeEfficacy(move.TypeId, user.Types));
                        oBattle.Stats.Hp -= damage;
                    }
                    // change stats
                    if (move.StatsChanges.Chance >= rand.Next(101))
                    {
                        var targets = new List<InBattleClass>();
                        switch (move.Target)
                        {
                            case StatsTarget.User:
                                targets.Add(uBattle);
                                break;
                            case StatsTarget.Opponent:
                                targets.Add(oBattle);
                                break;
                            case StatsTarget.Both:
                                targets.Add(uBattle);
                                targets.Add(oBattle);
                                break;
                        }
                        foreach (StatChange sc in move.StatsChanges.Changes)
                            foreach (InBattleClass t in targets)
                                t.Stats.ChangeStat(sc.Id, sc.Change);
                    }
                    uBattle.Stats.Hp += move.HpChanges;
                }
            }            
        }

        private int CalculateDamage(int level, int power, int offensive, int defensive)
        {
            return (2 * level / 5 + 2) * offensive * power / defensive / 50 + 2;
        }
    }

}
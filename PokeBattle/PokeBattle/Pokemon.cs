using System;
using System.Linq;

namespace PokeBattle
{
    class Pokemon
    {
        public int Id { get; }
        public string Name { get; }
        public int Level { get; }
        public Tuple<int, int?> Types { get; }
        public Move[] Moves { get; }
        public int Nature { get; }
        public InBattleClass InBattle;

        public Pokemon(int id, string name, int level, Tuple<int, int?> types, int[] baseStats, Move[] moves)
        {
            Id = id;
            Name = name;
            Level = level;
            Types = types;
            Moves = moves;
            Random rand = new Random();
            // Generate EVs. Max 252 / 4 = 63 for each stat but 510 / 4 = 127 in total.
            // TODO: change ev generation to be more random
            int[] Efforts = new int[6];
            for (int i = 0; i < Efforts.Length; i++)
                Efforts[i] = rand.Next((127 - Efforts.Count()) % 65);
            InBattle = new InBattleClass(
                new StatsClass
                (
                    HpStatCalc(baseStats[0], rand.Next(32), Efforts[0]),      // hp
                    GenericStatCalc(baseStats[1], rand.Next(32), Efforts[1]), // attack
                    GenericStatCalc(baseStats[2], rand.Next(32), Efforts[2]), // defense
                    GenericStatCalc(baseStats[3], rand.Next(32), Efforts[3]), // special attack
                    GenericStatCalc(baseStats[4], rand.Next(32), Efforts[4]), // special defense
                    GenericStatCalc(baseStats[5], rand.Next(32), Efforts[5])  // speed
                ));
            // TODO: nature should change stats. Guess I'll make a method somewhere to look up the natures table
            Nature = rand.Next(1, 26);
        }

        int GenericStatCalc(int b, int i, int e) => (2 * b + i + e) * Level / 100 + 5;

        int HpStatCalc(int b, int i, int e) => (2 * b + i + e) * Level / 100 + Level + 10;

        public override string ToString() =>  
            Name + " : lvl. " + Level +
            "\nHp: " + InBattle.Stats.Hp +
            "\nAttack: " + InBattle.Stats.Attack +
            "\nDefense: " + InBattle.Stats.Defense +
            "\nSpecialAttack: " + InBattle.Stats.SpecialAttack +
            "\nSpecialDefense: " + InBattle.Stats.SpecialDefense +
            "\nSpeed: " + InBattle.Stats.Speed;
    }

    public class InBattleClass
    {
        public InBattleClass(StatsClass stats)
        {
            Stats = stats;
        }

        public StatsClass Stats { get; }
        // TODO: status modifiers
    }

    public enum Statistics { None, Hp, Attack, Defense, SpecialAttack, SpecialDefense, Speed, Accuracy, Evasion }

    public class StatsClass
    {
        int[] _stats;
        int[] _stages;

        public StatsClass(int hp, int a, int d, int sa, int sd, int s)
        {
            _stats = new int[7];
            _stages = new int[9];
            _stats[1] = Hp = hp;
            _stats[2] = a;
            _stats[3] = d;
            _stats[4] = sa;
            _stats[5] = sd;
            _stats[6] = s;
        }
        
        public int this[int idx]
        {
            get
            {
                return _stats[idx];
            }
            set
            {
                _stats[idx] = value;
            }
        }
        public int this[Statistics s]
        {
            get
            {
                return this[(int)s];
            }
            set
            {
                this[(int)s] = value;
            }
        }

        public void ChangeStat(int statId, int n)
        {
            if (Math.Abs(_stages[statId] += n) > 6)
                _stages[statId] = _stages[statId] > 0 ? 6 : -6;
        }

        private int Calculate(Statistics s)
        {
            int idx = (int)s;
            return _stats[idx] * Math.Max(2, 2 + _stages[idx]) / Math.Max(2, 2 - _stages[idx]);
        }

        public int Hp { get; set; }

        public int MaxHp => Calculate(Statistics.Hp);
        public int Attack => Calculate(Statistics.Attack);
        public int Defense => Calculate(Statistics.Defense);
        public int SpecialAttack => Calculate(Statistics.SpecialAttack);
        public int SpecialDefense => Calculate(Statistics.SpecialDefense);
        public int Speed => Calculate(Statistics.Speed);
        public int AccuracyStage => _stages[7];
        public int EvasionStage => _stages[8];
    }
}
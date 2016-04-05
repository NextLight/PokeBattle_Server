using System;
using System.Linq;

namespace PokeBattle
{
    class Pokemon
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Level { get; private set; }
        public Tuple<int, int?> Types { get; private set; }
        public Move[] Moves { get; private set; }
        public int Hp { get; private set; }
        public int Nature { get; private set; }
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
                {
                    Hp             =      HpStatCalc(baseStats[0], rand.Next(32), Efforts[0]),
                    Attack         = GenericStatCalc(baseStats[1], rand.Next(32), Efforts[1]),
                    Defense        = GenericStatCalc(baseStats[2], rand.Next(32), Efforts[2]),
                    SpecialAttack  = GenericStatCalc(baseStats[3], rand.Next(32), Efforts[3]),
                    SpecialDefense = GenericStatCalc(baseStats[4], rand.Next(32), Efforts[4]),
                    Speed          = GenericStatCalc(baseStats[5], rand.Next(32), Efforts[5])
                });
            // TODO: nature should change stats. Guess I'll make a method somewhere to look up the natures table
            Nature = rand.Next(1, 26);
        }

        int GenericStatCalc(int b, int i, int e)
        {
            return (2 * b + i + e) * Level / 100 + 5;
        }

        int HpStatCalc(int b, int i, int e)
        {
            return (2 * b + i + e) * Level / 100 + Level + 10;
        }

        public override string ToString()
        {
            return Name + " : lvl. " + Level +
                "\nHp: " + InBattle.Stats.Hp +
                "\nAttack: " + InBattle.Stats.Attack +
                "\nDefense: " + InBattle.Stats.Defense +
                "\nSpecialAttack: " + InBattle.Stats.SpecialAttack +
                "\nSpecialDefense: " + InBattle.Stats.SpecialDefense +
                "\nSpeed: " + InBattle.Stats.Speed;
        }
    }

    public class InBattleClass
    {
        public InBattleClass(StatsClass stats)
        {
            Stats = stats;
        }

        public StatsClass Stats { get; private set; }
        // TODO: status modifiers
    }

    public enum Statistics { None, Hp, Attack, Defense, SpecialAttack, SpecialDefense, Speed, Accuracy, Evasion }

    public class StatsClass
    {
        int[] _stats = new int[9];


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
                return _stats[(int)s];
            }
            set
            {
                _stats[(int)s] = value;
            }
        }

        public int Hp { get { return this[Statistics.Hp]; } set { this[Statistics.Hp] = value; } }
        public int Attack { get { return this[Statistics.Attack]; } set { this[Statistics.Attack] = value; } }
        public int Defense { get { return this[Statistics.Defense]; } set { this[Statistics.Defense] = value; } }
        public int SpecialAttack { get { return this[Statistics.SpecialAttack]; } set { this[Statistics.SpecialAttack] = value; } }
        public int SpecialDefense { get { return this[Statistics.SpecialDefense]; } set { this[Statistics.SpecialDefense] = value; } }
        public int Speed { get { return this[Statistics.Speed]; } set { this[Statistics.Speed] = value; } }
        public int Accuracy { get { return this[Statistics.Accuracy]; } set { this[Statistics.Accuracy] = value; } }
        public int Evasion { get { return this[Statistics.Evasion]; } set { this[Statistics.Evasion] = value; } }
    }
}
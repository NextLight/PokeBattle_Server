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
        public int Attack { get; private set; }
        public int Defense { get; private set; }
        public int SpecialAttack { get; private set; }
        public int SpecialDefense { get; private set; }
        public int Speed { get; private set; }
        public int Nature { get; private set; }
        public class InBattleClass
        {
            public int Hp { get; set; }
            // TODO: status modifiers
        }
        public InBattleClass InBattle = new InBattleClass();

        public Pokemon(int id, string name, int level, Tuple<int, int?> types, int[] baseStats, Move[] moves)
        {
            this.Id = id;
            this.Name = name;
            this.Level = level;
            this.Types = types;
            this.Moves = moves;
            Random rand = new Random();
            // Generate EVs. Max 252 / 4 = 63 for each stat but 510 / 4 = 127 in total.
            // TODO: change ev generation to be more random
            int[] Efforts = new int[6];
            for (int i = 0; i < Efforts.Length; i++)
                Efforts[i] = rand.Next((127 - Efforts.Count()) % 65);
            this.Hp             =      HpStatCalc(baseStats[0], rand.Next(32), Efforts[0]);
            this.Attack         = GenericStatCalc(baseStats[1], rand.Next(32), Efforts[1]);
            this.Defense        = GenericStatCalc(baseStats[2], rand.Next(32), Efforts[2]);
            this.SpecialAttack  = GenericStatCalc(baseStats[3], rand.Next(32), Efforts[3]);
            this.SpecialDefense = GenericStatCalc(baseStats[4], rand.Next(32), Efforts[4]);
            this.Speed          = GenericStatCalc(baseStats[5], rand.Next(32), Efforts[5]);
            // TODO: nature should change stats. Guess I'll make a method somewhere to look up the natures table
            this.Nature = rand.Next(1, 26);
        }

        int GenericStatCalc(int b, int i, int e)
        {
            return (2 * b + i + e) * this.Level / 100 + 5;
        }

        int HpStatCalc(int b, int i, int e)
        {
            return (2 * b + i + e) * this.Level / 100 + this.Level + 10;
        }

        public override string ToString()
        {
            return Name + " : lvl. " + Level +
                "\nHp: " + Hp +
                "\nAttack: " + Attack +
                "\nDefense: " + Defense +
                "\nSpecialAttack: " + SpecialAttack +
                "\nSpecialDefense: " + SpecialDefense +
                "\nSpeed: " + Speed;
        }
    }
}
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
        // TODO: use in-battle stats
        public Battle(Pokemon p1, Pokemon p2)
        {
            this.pokemons = new Pokemon[] { p1, p2 };
            rand = new Random();
        }

        public void ChangePokemon(int idx, Pokemon p)
        {
            pokemons[idx] = p;
        }

        public void ExecuteMoves(int? m1, int? m2)
        {
            if (pokemons[0].Speed > pokemons[1].Speed)
            {
                ExecuteMove(0, m1);
                ExecuteMove(1, m2);
            }
            else {
                ExecuteMove(1, m2);
                ExecuteMove(0, m1);
            }
        }

        private void ExecuteMove(int p, int? m)
        {
            if (m == null) // not sure that's the right way tho
                return;
            Move move = pokemons[p].Moves[m.Value];
            if ((move.Accuracy ?? 100) >= rand.Next(101))
            {
                if (move.DamageClass != DamageClass.None && move.Power != null) 
                {
                    bool critical = rand.Next(16) == 0;
                    int damage = CalculateDamage(pokemons[p].Level, move.Power.Value,
                        move.DamageClass == DamageClass.Physical ? pokemons[p].Attack : pokemons[p].SpecialAttack, 
                        move.DamageClass == DamageClass.Physical ? pokemons[p ^ 1].Defense : pokemons[p ^ 1].SpecialDefense);
                    if (pokemons[p].Types.Item1 == move.TypeId || pokemons[p].Types.Item2 == move.TypeId)
                        damage = damage * 3 / 2;
                    if (critical)
                        damage = damage * 3 / 2;
                    damage = (int)(damage * PokeBox.TypeEfficacy(move.TypeId, pokemons[p ^ 1].Types));
                    pokemons[p ^ 1].InBattle.Hp -= damage; // eh xor, beware // TODO: change to something saner
                }
            }
        }

        private int CalculateDamage(int level, int power, int offensive, int defensive)
        {
            return (2 * level / 5 + 2) * offensive * power / defensive / 50 + 2;
        }
    }
}

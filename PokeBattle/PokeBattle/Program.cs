using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeBattle
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                var pokeBox = new PokeBox();
                Pokemon pk = pokeBox.GetRandomPokemonByLevel(100);
                Console.WriteLine(pk.ToString() + '\n');
            } while (Console.ReadKey().Key != ConsoleKey.Escape);
        }
    }
}

using System;
using System.Linq;

namespace PokeBattle
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for 2 players.");
            Player[] players = new Player[2] { new Player(), new Player() };
            players[0].Connect();
#if !DEBUG
            players[1].Connect();
#endif
            players[0].WritePokeTeam();
#if !DEBUG
            players[1].WritePokeTeam();
#endif
            players[0].WriteOpponent(players[1].SelectedPokemon);
#if !DEBUG
            players[1].WriteOpponent(players[0].SelectedPokemon);
#endif
            Battle battle = new Battle(players[0].SelectedPokemon, players[1].SelectedPokemon);

            bool exit = false;
            while (!exit)
            {
                players[0].WriteBeginTurn();
#if !DEBUG
                players[1].WriteBeginTurn();
#endif

                byte[][] m = new byte[2][];
                m[0] = players[0].ReadMove();
#if !DEBUG
                m[1] = players[1].ReadMove();
#else
                m[1] = new byte[] { 0, 1 };
#endif
                for (int i = 0; i < m.Length; i++)
                {
                    if (m[i][0] == 1)
                    {
                        players[i].SelectedPokemonIdx = m[i][1];
                        battle.ChangePokemon(i, players[i].SelectedPokemon);
                    }
                    else
                    {
                        battle.StoreMove(i, m[i][1]);
                    }
                }
                foreach (var t in battle.ExecuteMoves())
                    foreach (Player p in players)
                        p.WriteText($"{players[t.Item1].SelectedPokemon.Name} used {players[t.Item1].SelectedPokemon.Moves[t.Item2].Name}\n");


                players[0].WriteInBattle();
#if !DEBUG
                players[1].WriteInBattle();
#endif
                players[0].WriteInBattleOpponent(players[1].SelectedPokemon.InBattle);
#if !DEBUG
                players[1].WriteInBattleOpponent(players[0].SelectedPokemon.InBattle);
#endif
                exit = players.Any(p => p.Lost);
                if (!exit)
                {
                    foreach (Player p in players)
                        if (p.SelectedPokemon.Fainted)
                        {
                            p.WriteUserFainted();
                            byte[] mu;
#if DEBUG
                            if (p == players[0])
#endif
                                mu = p.ReadMove();
#if DEBUG
                            else
                                mu = new byte[] { 1, (byte)((p.SelectedPokemonIdx + 1) % 6) };
#endif
                            if (mu[0] == 1)
                            {
                                p.SelectedPokemonIdx = mu[1];
                                foreach (Player pl in players.Where(pl => pl != p))
                                {
#if DEBUG
                                    if (pl == players[0])
#endif
                                    {
                                        pl.WriteOpponentFainted();
                                        pl.WriteOpponent(p.SelectedPokemon);
                                        byte[] mp = pl.ReadMove();
                                        if (mp[0] == 1)
                                            pl.SelectedPokemonIdx = mp[1];
                                        else if (mp[0] != 2)
                                            throw new Exception();
                                    }
                                }
                            }
                            else
                                throw new Exception();
                        }
                    for (int i = 0; i < players.Length; i++)
                        battle.ChangePokemon(i, players[i].SelectedPokemon);
                }
            }

            players[0].Close();
#if !DEBUG
            players[1].Close();
#endif
            Console.WriteLine("Game over. Player " + (players[0].Lost ? 2 : 1) + " won.");
            Console.ReadKey();
        }
    }
}

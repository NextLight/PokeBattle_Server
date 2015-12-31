# PokeBattle_Server (WIP)
PokeBattle is a school project and it aims to be a pokemon battle simulator.

The server interfaces 2 clients connected via TCP and handles all the battle logic, the clients are only responsible for input and graphics handling.

The pokemon stats, moves and whatnot mechanics are from the 6th generation, but every pokemon from the international pokedex is playable.

This uses the [veekun database](https://github.com/veekun/pokedex) and the [Cave of Dragonflies eference](http://www.dragonflycave.com/mechanics.aspx) for battle/stats mechanics so huge thanks to eevee for making them available.

# Building
The only building dependence is [System.Data.SQLite](https://system.data.sqlite.org) but NuGet should automatically take care of it.

# Running
You will need to have the pokedex database (`pokedex.sqlite`) in the same directory as the exe. To generate it [get the veekun pokedex data](https://github.com/veekun/pokedex/wiki/Getting-Data). Instead of `pokexed load`, to load the data faster and only get the needed tables, you can run:

`pokedex load pokemon pokemon_moves moves move_names pokemon_stats stats pokemon_types types pokemon_species pokemon_evolution`

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace PokeBattle
{
    static class PokeBox
    {
        static Db db;
        static int versionGroupId = 16, languageId = 9;
        static public int NOfPokemon { get; private set; }

        static PokeBox()
        {
            db = new Db("pokedex.sqlite");
            NOfPokemon = db.ReadValue<int>("SELECT MAX(species_id) FROM pokemon");
        }

        static public Pokemon GetPokemonByIdAndLevel(int id, int level)
        {
            string name = db.ReadValue<string>("SELECT identifier FROM pokemon WHERE id = " + id);
            var types = db.ReadColumn<int>("SELECT type_id FROM pokemon_types WHERE pokemon_id = " + id).ToArray();
            var baseStats = db.ReadColumn<int>($"SELECT base_stat FROM pokemon_stats WHERE pokemon_id = {id} ORDER BY stat_id").ToArray();
            string sql = $@"SELECT pk_mv.move_id, name, power, accuracy, pp, type_id, damage_class_id
                FROM pokemon_moves pk_mv
                JOIN moves mv ON mv.id = pk_mv.move_id
                JOIN move_names mv_n ON mv_n.move_id = pk_mv.move_id
                WHERE pokemon_id = {id} AND version_group_id = {versionGroupId} AND local_language_id = {languageId} AND level BETWEEN 1 AND {level}";
            Move[] moves = db.ReadDataTable(sql).AsEnumerable().OrderBy(dr => Guid.NewGuid()).Take(4)
                .Select(dr => new Move(dr.GetValue<int>("move_id"), dr.GetValue<string>("name"), dr.GetValue<int?>("power"), dr.GetValue<int?>("accuracy"), dr.GetValue<int?>("pp"),
                    dr.GetValue<int>("type_id"), (DamageClass)dr.GetValue<int>("damage_class_id")))
                .ToArray();
            return new Pokemon(name, level, new Tuple<int, int?>(types[0], types.Length > 1 ? types[1] : (int?)null), baseStats.ToArray(), moves);
        }

        static public Pokemon GetRandomPokemonByLevel(int level)
        {
            // Try to always select the best pokemon id in evolution chain
            // TODO: use MAX(minimum_level) from pokemon_evolution instead of MAX(evolves_from_species_id)
            int evChain = db.ReadValue<int>($"SELECT evolution_chain_id FROM pokemon_species ORDER BY RANDOM() limit 0, 1");
            int? maxFromSpecieId = db.ReadValue<int?>("SELECT MAX(evolves_from_species_id) FROM pokemon_species WHERE evolution_chain_id = " + evChain);
            int id;
            if (maxFromSpecieId != null)
                id = db.ReadValue<int>($"SELECT id FROM pokemon_species WHERE evolves_from_species_id = {maxFromSpecieId} ORDER BY RANDOM() limit 0, 1");
            else // chain doesn't have evolutions
                id = db.ReadValue<int>($"SELECT id FROM pokemon_species WHERE evolution_chain_id = {evChain}");
            return GetPokemonByIdAndLevel(id, level);
        }
    }
}

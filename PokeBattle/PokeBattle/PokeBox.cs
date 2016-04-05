using System;
using System.Linq;
using System.Data;
using System.Text.RegularExpressions;

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
            var baseStats = db.ReadColumn<int>("SELECT base_stat FROM pokemon_stats WHERE pokemon_id = " + id + " ORDER BY stat_id").ToArray();
            string sqlMoves =
                @"SELECT pk_mv.move_id, name, power, accuracy, pp, type_id, damage_class_id,
                    meta_ailment_id, min_hits, max_hits, min_turns, max_turns, (drain + healing) hp_changes,
                    crit_rate, ailment_chance, flinch_chance, stat_chance, effect_chance, short_effect
                FROM pokemon_moves pk_mv
                JOIN moves mv ON mv.id = pk_mv.move_id
                JOIN move_meta mv_m ON mv_m.move_id = mv.id
                LEFT JOIN move_names mv_n ON mv_n.move_id = mv.id
                LEFT JOIN move_effect_prose mv_e ON mv_e.move_effect_id = mv.effect_id
                WHERE pokemon_id = " + id +
                " AND version_group_id = " + versionGroupId +
                " AND mv_n.local_language_id = " + languageId +
                " AND mv_e.local_language_id = " + languageId +
                " AND level BETWEEN 1 AND " + level;
            Move[] moves = db.ReadDataTable(sqlMoves).AsEnumerable().OrderBy(dr => Guid.NewGuid()).Take(4)
                .Select(dr => new Move
                {
                    Id = dr.GetValue<int>("move_id"),
                    Name = dr.GetValue<string>("name"),
                    Power = dr.GetValue<int?>("power"),
                    Accuracy = dr.GetValue<int?>("accuracy"),
                    Pp = dr.GetValue<int?>("pp"),
                    TypeId = dr.GetValue<int>("type_id"),
                    DamageClass = (DamageClass)dr.GetValue<int>("damage_class_id"),
                    Status = (Statuses)dr.GetValue<int>("meta_ailment_id"),
                    StatusChance = dr.GetValue<int>("ailment_chance"),
                    MinHits = dr.GetValue<int?>("min_hits"),
                    MaxHits = dr.GetValue<int?>("max_hits"),
                    MinTurns = dr.GetValue<int?>("min_turns"),
                    MaxTurns = dr.GetValue<int?>("max_turns"),
                    HpChanges = dr.GetValue<int>("hp_changes"),
                    CriticalRate = dr.GetValue<int>("crit_rate"),
                    FlinchChance = dr.GetValue<int>("flinch_chance"),
                    StatsChanges = new StatsChanges(dr.GetValue<int>("stat_chance"),
                        db.ReadDataTable("SELECT stat_id, change FROM move_meta_stat_change WHERE move_id = " + dr.GetValue<int>("move_id"))
                            .AsEnumerable().Select(r => new StatChange(r.GetValue<int>("stat_id"), r.GetValue<int>("change"))).ToArray()),
                    EffectText = Regex.Replace(
                        dr.GetValue<string>("short_effect").Replace("$effect_chance%", dr.GetValue<int?>("effect_chance").ToString()),
                        @"\[(.*?)\]{.*?}",
                        "$1")
                })
                .ToArray();
            return new Pokemon(id, name, level, new Tuple<int, int?>(types[0], types.Length > 1 ? types[1] : (int?)null), baseStats.ToArray(), moves);
        }

        static public Pokemon GetRandomPokemonByLevel(int level)
        {
            // Try to always select the best pokemon id in evolution chain
            // TODO: use MAX(minimum_level) from pokemon_evolution instead of MAX(evolves_from_species_id)
            int evChain = db.ReadValue<int>("SELECT evolution_chain_id FROM pokemon_species ORDER BY RANDOM() limit 0, 1");
            int? maxFromSpecieId = db.ReadValue<int?>("SELECT MAX(evolves_from_species_id) FROM pokemon_species WHERE evolution_chain_id = " + evChain);
            int id;
            if (maxFromSpecieId != null)
                id = db.ReadValue<int>("SELECT id FROM pokemon_species WHERE evolves_from_species_id = " + maxFromSpecieId + " ORDER BY RANDOM() limit 0, 1");
            else // chain doesn't have evolutions
                id = db.ReadValue<int>("SELECT id FROM pokemon_species WHERE evolution_chain_id = " + evChain);
            return GetPokemonByIdAndLevel(id, level);
        }

        static private int TypeEfficacy(int m1, int m2)
        {
            return db.ReadValue<int>("SELECT damage_factor FROM type_efficacy WHERE damage_type_id = " + m1 + " AND target_type_id = " + m2);
        }

        static public double TypeEfficacy(int moveType, Tuple<int, int?> pokemonTypes)
        {
            double eff1, eff2 = 1;
            eff1 = TypeEfficacy(moveType, pokemonTypes.Item1) / 100.0;
            if (pokemonTypes.Item2 != null)
                eff2 = TypeEfficacy(moveType, pokemonTypes.Item2.Value) / 100.0;
            return eff1 * eff2;
        }
    }
}

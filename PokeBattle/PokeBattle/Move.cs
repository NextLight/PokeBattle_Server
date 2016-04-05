using System;
using System.Collections.Generic;
using System.Linq;

namespace PokeBattle
{
    enum DamageClass { None = 1, Physical, Special };
    enum Statuses { Unknown, None, Paralysis, Sleep, Freeze, Burn, Poison, Confusion, Infatuation, Trap, Nightmare, Torment, Disable, Yawn, HealBlock, NoTypeImmunity, LeechSeed, Embargo, PerishSong, Ingrain }
    enum StatsTarget { None, User, Opponent, Both }

    class Move
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Power { get; set; }
        public int? Accuracy { get; set; }
        public int? Pp { get; set; }
        public bool IsOhko { get; set; }
        public int TypeId { get; set; }
        public DamageClass DamageClass { get; set; }
        public Statuses Status { get; set; }
        public int StatusChance { get; set; }
        public int? MinHits { get; set; }
        public int? MaxHits { get; set; }
        public int? MinTurns { get; set; }
        public int? MaxTurns { get; set; }
        public int HpChanges { get; set; }
        public int CriticalRate { get; set; }
        public int FlinchChance { get; set; }
        public StatsTarget Target { get; set; }
        public StatsChanges StatsChanges { get; set; }
        public string EffectText { get; set; }
    }

    class StatsChanges
    {
        public int Chance { get; private set; }
        public StatChange[] Changes { get; private set; }

        public StatsChanges(int chance, StatChange[] stats)
        {
            Chance = chance;
            Changes = stats;
        }
    }

    class StatChange
    {
        public int Id { get; private set; }
        public int Change { get; private set; }

        public StatChange(int id, int change)
        {
            Id = id;
            Change = change;
        }
    }
}

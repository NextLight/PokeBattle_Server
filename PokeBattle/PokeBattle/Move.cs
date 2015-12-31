using System;
using System.Collections.Generic;
using System.Linq;

namespace PokeBattle
{
    enum DamageClass { None = 1, Physical, Special };
    class Move
    {
        int id;
        public string Name { get; private set; }
        public int? Power { get; private set; }
        public int? Accuracy { get; private set; }
        public int? Pp { get; private set; }
        public int TypeId { get; private set; }
        public DamageClass DamageClass { get; private set; }

        public Move(int id, string name, int? power, int? accuracy, int? pp, int typeId, DamageClass damageClass)
        {
            this.id = id;
            this.Name = name;
            this.Power = power;
            this.Accuracy = accuracy;
            this.Pp = pp;
            this.TypeId = typeId;
            this.DamageClass = damageClass;
        }
    }
}

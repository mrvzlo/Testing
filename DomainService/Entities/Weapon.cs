﻿using System.Collections.Generic;

namespace Pnprpg.DomainService.Entities
{
    public class Weapon : BaseEntity<int>
    {
        public int Level { get; set; }
        public int SkillId { get; set; }
        public int Weight { get; set; }
        public string Name { get; set; }

        public virtual SkillInfo Skill { get; set; }
        public virtual ICollection<WeaponBonus> Bonuses { get; set; }
    }
}
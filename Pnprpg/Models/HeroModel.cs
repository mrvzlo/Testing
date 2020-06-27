﻿using System;
using System.Collections.Generic;
using System.Linq;
using Boot.Enums;
using Boot.Helpers;
using Boot.Models.JsonModels;

namespace Boot.Models
{
    public class HeroModel
    {
        public int[] Stats;
        public int[] MinStats;
        public int[] Traits;
        public int Level, Race;
        public ChaosLevel Chaos;
        public Dictionary<int, int> Skills;
        public int UsedSkillPoints;

        public string Name;

        public int FreeStatPoints() =>
            Chaos == ChaosLevel.Random ? 0 : Constants.MaxStatSum - Stats.Sum();
        public int FreeSkillPoints() =>
            Constants.BaseSkillPoints + Constants.SkillPointsPerLvl * Level - UsedSkillPoints;

        public int BaseArmor() => (Stats[(int)StatType.E] + 5) / 10;
        public int BaseDmg() => (Stats[(int)StatType.S] + 5) / 10;
        public int MaxHp() => Stats[(int)StatType.E] + Level * 2 - 2;
        public int MaxEp() => Math.Max(Stats[(int)StatType.I] + Level - 4, 0);
        public int MaxCarry() => Stats[(int)StatType.S] * Stats[(int)StatType.E] / 10;

        #region SaveLoad

        public HeroModel(string data)
        {
            ResetSkills();
            ResetTraits();
            if (string.IsNullOrEmpty(data)) return;
            var count = EnumExtensions.GetEnumCount(typeof(StatType));
            var list = data.Split(StringHelper.Separator);
            Name = list.First();
            var intData = list.Skip(1).Select(int.Parse).ToList();
            Stats = new int[count];
            MinStats = new int[count];
            var x = 0;
            for (var i = 0; i < Stats.Length; i++)
                Stats[i] = intData[x++];
            Level = intData[x++];
            Race = intData[x++];
            Chaos = (ChaosLevel)intData[x++];
            SetMinStats();
            var skillsCount = intData[x++];
            for (var i = 0; i < skillsCount; i++)
                Skills.Add(intData[x++], intData[x++]);
            for (var i = 0; i < Constants.TraitCount; i++)
                Traits[i] = intData[x++];
        }

        public override string ToString()
        {
            var list = new List<string> { Name };
            list.AddRange(Stats.Select(x => x.ToString()));
            list.AddRange(new[] { Level, Race, (int)Chaos }.Select(x => x.ToString()));
            list.Add(Skills.Count.ToString());
            if (Skills.Any())
            {
                var skillsInfo = Skills.Where(x => x.Value > 0)
                    .Select(x => $"{x.Key}{StringHelper.Separator}{x.Value}");
                list.AddRange(skillsInfo);
            }
            list.AddRange(Traits.Select(x => x.ToString()));
            return string.Join($"{StringHelper.Separator}", list);
        }

        public HeroModel(ChaosLevel chaos)
        {
            Name = "";
            ResetSkills();
            ResetTraits();
            Level = 1;
            Chaos = chaos;
            var count = EnumExtensions.GetEnumCount(typeof(StatType));
            Stats = new int[count];
            MinStats = new int[count];
            var rand = new Random(DateTime.Now.Millisecond);
            for (var i = 0; i < Stats.Length; i++)
                Stats[i] = Constants.MinStat;
            SetMinStats();
            switch (chaos)
            {
                case ChaosLevel.Normal:
                    for (var i = 0; i < Stats.Length; i++)
                        Stats[i] = 8;
                    return;
                case ChaosLevel.High:
                    return;
                case ChaosLevel.Extreme:
                    for (var i = 0; i < Stats.Length; i++)
                        Stats[i] = Constants.MaxStat / 2;
                    for (var i = 0; i < Constants.MaxStat / 2; i++)
                    {
                        IncStat(rand.Next(count), -1);
                        IncStat(rand.Next(count), 1);
                    }
                    return;
                case ChaosLevel.Random:
                    for (var i = 0; i < Stats.Length; i++)
                        Stats[i] = rand.Next(Constants.MaxStat) + 1;
                    return;
            }
        }
        
        public void LoadRace(Race race)
        {
            race.effects?.ForEach(x => ApplyStatEffect(x, false, true));
        }

        #endregion

        public bool ChangeRace(Race oldRace, Race newRace)
        {
            if (oldRace.id == newRace.id)
                return false;
            Race = newRace.id;
            oldRace.effects?.ForEach(x => ApplyStatEffect(x, true));
            newRace.effects?.ForEach(x => ApplyStatEffect(x));

            return !(Stats.Any(x => x < 1 || x > Constants.MaxStat));
        }

        public void ResetTraits()
        {
            Traits = new int[Constants.TraitCount];
            for (var i = 0; i < Constants.TraitCount; i++)
                Traits[i] = -1;
        }

        public bool AddTrait(int id)
        {
            for (var i = 0; i < Traits.Length; i++)
            {
                if (Traits[i] >= 0) continue;
                Traits[i] = id;
                return true;
            }

            return false;
        }

        public bool IncStat(int attr, int val)
        {
            if (Stats[attr] + val < MinStats[attr]
                || Stats[attr] + val > Constants.MaxStat
                || Stats.Sum() + val > Constants.MaxStatSum)
                return false;
            Stats[attr] += val;
            return true;
        }

        #region Skills 

        public void ResetSkills()
        {
            UsedSkillPoints = 0;
            Skills = new Dictionary<int, int>();
        }

        public bool AddSkill(SkillInfo skillInfo)
        {
            if (!CanIncSkill(skillInfo)) return false;
            Skills[skillInfo.Id]++;
            UsedSkillPoints += skillInfo.Difficulty + 1;
            return true;
        }

        public bool CanIncSkill(SkillInfo skillInfo)
        {
            var skillPoints = GetOrCreateSkillPoints(skillInfo.Id);
            return skillInfo.Difficulty + 1 <= FreeSkillPoints() && Level > skillPoints;
        }

        private int GetOrCreateSkillPoints(int skillId)
        {
            if (Skills.ContainsKey(skillId))
                return Skills[skillId];
            Skills.Add(skillId, 0);
            return 0;
        }

        #endregion

        private void ApplyStatEffect(Effect effect, bool revert = false, bool baseOnly= false)
        {
            if (effect.type != EffectType.Weaken && effect.type != EffectType.Boost)
                return;
            var stat = (int)effect.stat;
            var value = effect.value;
            if (revert ^ effect.type == EffectType.Weaken)
                value *= -1;

            if (baseOnly)
                MinStats[stat] += value;
            else
                Stats[stat] += value;
        }

        private void SetMinStats()
        {
            var baseMin = GetMinStat();
            for (var i = 0; i < Stats.Length; i++)
                MinStats[i] = baseMin;
        }

        private int GetMinStat()
        {
            switch (Chaos)
            {
                case ChaosLevel.Normal:
                    return 8;
                case ChaosLevel.High:
                    return Constants.MinStat;
                case ChaosLevel.Extreme:
                    return Constants.MaxStat;
                case ChaosLevel.Random:
                    return Constants.MaxStat;
                default:
                    return Constants.MinStat;
            }
        }
    }
}
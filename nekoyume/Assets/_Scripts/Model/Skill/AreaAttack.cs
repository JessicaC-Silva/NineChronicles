using System;
using System.Collections.Generic;
using Nekoyume.TableData;

namespace Nekoyume.Model.Skill
{
    [Serializable]
    public class AreaAttack : AttackSkill
    {
        public AreaAttack(SkillSheet.Row skillRow, int power, int chance) : base(skillRow, power, chance)
        {
        }

        public override Model.BattleStatus.Skill Use(
            CharacterBase caster, 
            int simulatorWaveTurn, 
            IEnumerable<Buff.Buff> buffs)
        {
            return new Model.BattleStatus.AreaAttack(
                (CharacterBase) caster.Clone(), 
                ProcessDamage(caster, simulatorWaveTurn), 
                ProcessBuff(caster, simulatorWaveTurn, buffs)
            );
        }
    }
}
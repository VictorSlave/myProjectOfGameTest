using UnityEngine;
using System.Collections.Generic;

namespace CardGame
{
    public class BattleManager : MonoBehaviour
    {
        public const int CARD_SLOT_MAX_LENGTH = 10;
        
        private BattleState battleState => GameManager.Instance.battleState;
        
        public void PlayCard(Card card, int slotPosition)
        {
            // 检查费用
            if (battleState.player.currentMana < card.cost)
                return;
            
            // 消耗费用
            battleState.player.currentMana -= card.cost;
            
            // 放入横槽
            if (slotPosition < 0 || slotPosition > battleState.cardSlot.Count)
                slotPosition = battleState.cardSlot.Count;
            
            battleState.cardSlot.Insert(slotPosition, card);
            
            // 检查横槽长度
            if (battleState.cardSlot.Count > CARD_SLOT_MAX_LENGTH)
            {
                battleState.cardSlot.RemoveAt(CARD_SLOT_MAX_LENGTH);
            }
            
            // 从手牌中移除
            battleState.player.hand.Remove(card);
            
            // 检查技能触发
            CheckSkillTriggers();
        }
        
        public void UseSkill(Skill skill, SkillMode mode)
        {
            // 检查技能是否可以触发
            if (!CanTriggerSkill(skill))
                return;
            
            if (mode == SkillMode.Cast)
            {
                // 施放模式
                TriggerSkill(skill);
                // 消耗对应序列（删除）
                ConsumeSkillSequence(skill);
                // 回复1点费用
                battleState.player.currentMana = Mathf.Min(battleState.player.currentMana + 1, battleState.player.maxMana);
            }
            else if (mode == SkillMode.Charge)
            {
                // 蓄力模式
                battleState.chargedSkill = skill;
                // 回复1点费用
                battleState.player.currentMana = Mathf.Min(battleState.player.currentMana + 1, battleState.player.maxMana);
            }
        }
        
        public void UseChargedSkill()
        {
            if (battleState.chargedSkill == null)
                return;
            
            // 触发蓄力技能
            TriggerSkill(battleState.chargedSkill);
            // 消耗对应序列
            ConsumeSkillSequence(battleState.chargedSkill);
            // 清空蓄力技能
            battleState.chargedSkill = null;
        }
        
        private bool CanTriggerSkill(Skill skill)
        {
            // 检查横槽中是否有对应序列
            for (int i = 0; i <= battleState.cardSlot.Count - skill.effects.Count; i++)
            {
                bool match = true;
                for (int j = 0; j < skill.effects.Count; j++)
                {
                    Card card = battleState.cardSlot[i + j];
                    SkillEffectType expectedEffect = skill.effects[j];
                    
                    // 检查卡牌类型是否匹配技能效果类型
                    if (!CardTypeMatchesEffect(card.type, expectedEffect))
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return true;
            }
            return false;
        }
        
        private bool CardTypeMatchesEffect(CardType cardType, SkillEffectType effectType)
        {
            return (cardType == CardType.Attack && effectType == SkillEffectType.Attack) ||
                   (cardType == CardType.Defense && effectType == SkillEffectType.Defense) ||
                   (cardType == CardType.Heal && effectType == SkillEffectType.Heal);
        }
        
        private void TriggerSkill(Skill skill)
        {
            // 触发技能效果
            foreach (SkillEffectType effect in skill.effects)
            {
                switch (effect)
                {
                    case SkillEffectType.Attack:
                        // 对怪物造成伤害
                        battleState.monster.currentHealth -= skill.value;
                        break;
                    case SkillEffectType.Defense:
                        // 为玩家增加护盾
                        battleState.player.shield += skill.value;
                        break;
                    case SkillEffectType.Heal:
                        // 恢复玩家生命值
                        battleState.player.currentHealth = Mathf.Min(battleState.player.currentHealth + skill.value, battleState.player.maxHealth);
                        break;
                }
            }
            
            // 检查战斗是否结束
            CheckBattleEnd();
        }
        
        private void ConsumeSkillSequence(Skill skill)
        {
            // 找到并删除第一个匹配的序列
            for (int i = 0; i <= battleState.cardSlot.Count - skill.effects.Count; i++)
            {
                bool match = true;
                for (int j = 0; j < skill.effects.Count; j++)
                {
                    Card card = battleState.cardSlot[i + j];
                    SkillEffectType expectedEffect = skill.effects[j];
                    
                    if (!CardTypeMatchesEffect(card.type, expectedEffect))
                    {
                        match = false;
                        break;
                    }
                }
                
                if (match)
                {
                    // 删除匹配的序列
                    battleState.cardSlot.RemoveRange(i, skill.effects.Count);
                    break;
                }
            }
        }
        
        private void CheckSkillTriggers()
        {
            // 这里可以添加技能触发的视觉提示
        }
        
        public void EndPlayerTurn()
        {
            // 结束玩家回合
            battleState.isPlayerTurn = false;
            
            // 怪物回合
            StartCoroutine(MonsterTurn());
        }
        
        private System.Collections.IEnumerator MonsterTurn()
        {
            yield return new WaitForSeconds(1f);
            
            // 怪物AI逻辑
            if (battleState.monster.currentHealth < 30)
            {
                // 低血量时优先防御
                battleState.monster.defense += 2;
            }
            else
            {
                // 正常攻击
                int damage = Mathf.Max(1, battleState.monster.attack - battleState.player.shield);
                battleState.player.currentHealth -= damage;
                battleState.player.shield = Mathf.Max(0, battleState.player.shield - battleState.monster.attack);
            }
            
            // 检查战斗是否结束
            if (CheckBattleEnd())
                yield break;
            
            // 进入下一回合
            StartNextTurn();
        }
        
        private void StartNextTurn()
        {
            // 增加回合数
            battleState.turnCount++;
            
            // 重置费用
            battleState.player.currentMana = battleState.player.maxMana;
            
            // 抽牌
            GameManager.Instance.DrawCards(2);
            
            // 清空蓄力技能
            battleState.chargedSkill = null;
            
            // 玩家回合
            battleState.isPlayerTurn = true;
        }
        
        private bool CheckBattleEnd()
        {
            if (battleState.player.currentHealth <= 0)
            {
                // 玩家失败
                GameManager.Instance.EndBattle(false);
                return true;
            }
            
            if (battleState.monster.currentHealth <= 0)
            {
                // 玩家胜利
                GameManager.Instance.EndBattle(true);
                return true;
            }
            
            return false;
        }
    }
}

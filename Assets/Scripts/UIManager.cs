using UnityEngine;
using System.Collections.Generic;

namespace CardGame
{
    public class UIManager : MonoBehaviour
    {
        private BattleState battleState => GameManager.Instance.battleState;
        private BattleManager battleManager;
        private bool showMap = true;
        
        private void Start()
        {
            battleManager = GetComponent<BattleManager>();
        }
        
        private void OnGUI()
        {
            if (showMap)
            {
                DrawMapUI();
            }
            else
            {
                DrawBattleUI();
            }
        }
        
        private void DrawMapUI()
        {
            GUI.Box(new Rect(10, 10, 300, 400), "地图");
            
            int y = 40;
            foreach (MapNode node in GameManager.Instance.mapNodes)
            {
                bool isUnlocked = node.isUnlocked;
                GUI.enabled = isUnlocked;
                
                if (GUI.Button(new Rect(20, y, 280, 40), $"{node.name} ({node.type})") && isUnlocked)
                {
                    // 移动到节点
                    GameManager.Instance.MoveToNode(node.id);
                    // 显示战斗界面
                    showMap = false;
                }
                
                y += 50;
            }
            
            GUI.enabled = true;
        }
        
        private void DrawBattleUI()
        {
            // 玩家状态
            GUI.Box(new Rect(10, 10, 300, 150), "玩家状态");
            GUI.Label(new Rect(20, 40, 280, 20), $"生命值: {battleState.player.currentHealth}/{battleState.player.maxHealth}");
            GUI.Label(new Rect(20, 60, 280, 20), $"护盾: {battleState.player.shield}");
            GUI.Label(new Rect(20, 80, 280, 20), $"费用: {battleState.player.currentMana}/{battleState.player.maxMana}");
            
            // 怪物状态
            if (battleState.monster != null)
            {
                GUI.Box(new Rect(320, 10, 300, 100), "怪物状态");
                GUI.Label(new Rect(330, 40, 280, 20), $"名称: {battleState.monster.name}");
                GUI.Label(new Rect(330, 60, 280, 20), $"生命值: {battleState.monster.currentHealth}/{battleState.monster.maxHealth}");
            }
            
            // 手牌
            GUI.Box(new Rect(10, 170, 610, 150), "手牌");
            int handX = 20;
            foreach (Card card in battleState.player.hand)
            {
                if (GUI.Button(new Rect(handX, 200, 100, 120), $"{card.name}\n{GetCardTypeText(card.type)}: {card.value}\n费用: {card.cost}"))
                {
                    // 直接放入最后位置
                    battleManager.PlayCard(card, battleState.cardSlot.Count);
                }
                handX += 110;
            }
            
            // 横槽
            GUI.Box(new Rect(10, 330, 610, 120), "横槽");
            int slotX = 20;
            for (int i = 0; i < BattleManager.CARD_SLOT_MAX_LENGTH; i++)
            {
                if (i < battleState.cardSlot.Count)
                {
                    Card card = battleState.cardSlot[i];
                    GUI.Button(new Rect(slotX, 360, 50, 80), $"{GetCardTypeText(card.type)}");
                }
                else
                {
                    GUI.Box(new Rect(slotX, 360, 50, 80), "");
                }
                slotX += 60;
            }
            
            // 技能
            GUI.Box(new Rect(10, 460, 300, 150), "技能");
            int skillY = 490;
            foreach (Skill skill in battleState.skills)
            {
                if (GUI.Button(new Rect(20, skillY, 280, 30), $"{skill.name}: {string.Join("→", skill.effects)}"))
                {
                    // 默认使用施放模式
                    battleManager.UseSkill(skill, SkillMode.Cast);
                }
                skillY += 40;
            }
            
            // 蓄力技能
            if (battleState.chargedSkill != null)
            {
                GUI.Box(new Rect(320, 460, 300, 80), "蓄力技能");
                if (GUI.Button(new Rect(330, 490, 280, 40), $"使用蓄力技能: {battleState.chargedSkill.name}"))
                {
                    battleManager.UseChargedSkill();
                }
            }
            
            // 结束回合按钮
            if (GUI.Button(new Rect(320, 550, 300, 50), "结束回合"))
            {
                battleManager.EndPlayerTurn();
            }
            
            // 返回地图按钮
            if (GUI.Button(new Rect(10, 610, 610, 50), "返回地图"))
            {
                showMap = true;
            }
        }
        
        private string GetCardTypeText(CardType type)
        {
            switch (type)
            {
                case CardType.Attack:
                    return "攻击";
                case CardType.Defense:
                    return "防御";
                case CardType.Heal:
                    return "治疗";
                default:
                    return "未知";
            }
        }
    }
}

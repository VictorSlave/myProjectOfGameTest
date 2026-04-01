using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CardGame
{
    public class UIManager : MonoBehaviour
    {
        public GameObject battleUI;
        public GameObject mapUI;
        
        // 战斗UI元素
        public Text playerHealthText;
        public Text playerShieldText;
        public Text playerManaText;
        public Text monsterHealthText;
        public Text monsterNameText;
        public Transform handPanel;
        public Transform cardSlotPanel;
        public Transform skillPanel;
        public Transform chargedSkillPanel;
        public Button endTurnButton;
        
        // 地图UI元素
        public Transform mapNodePanel;
        
        private BattleState battleState => GameManager.Instance.battleState;
        private BattleManager battleManager;
        
        private void Start()
        {
            battleManager = GetComponent<BattleManager>();
            endTurnButton.onClick.AddListener(() => battleManager.EndPlayerTurn());
            
            // 初始显示地图界面
            ShowMapUI();
        }
        
        private void Update()
        {
            if (battleUI.activeSelf)
            {
                UpdateBattleUI();
            }
            else if (mapUI.activeSelf)
            {
                UpdateMapUI();
            }
        }
        
        public void ShowBattleUI()
        {
            battleUI.SetActive(true);
            mapUI.SetActive(false);
            UpdateBattleUI();
        }
        
        public void ShowMapUI()
        {
            battleUI.SetActive(false);
            mapUI.SetActive(true);
            UpdateMapUI();
        }
        
        private void UpdateBattleUI()
        {
            // 更新玩家状态
            playerHealthText.text = $"生命值: {battleState.player.currentHealth}/{battleState.player.maxHealth}";
            playerShieldText.text = $"护盾: {battleState.player.shield}";
            playerManaText.text = $"费用: {battleState.player.currentMana}/{battleState.player.maxMana}";
            
            // 更新怪物状态
            if (battleState.monster != null)
            {
                monsterNameText.text = battleState.monster.name;
                monsterHealthText.text = $"生命值: {battleState.monster.currentHealth}/{battleState.monster.maxHealth}";
            }
            
            // 更新手牌
            UpdateHandCards();
            
            // 更新横槽
            UpdateCardSlot();
            
            // 更新技能
            UpdateSkills();
            
            // 更新蓄力技能
            UpdateChargedSkill();
        }
        
        private void UpdateHandCards()
        {
            // 清空手牌面板
            foreach (Transform child in handPanel)
            {
                Destroy(child.gameObject);
            }
            
            // 创建手牌
            foreach (Card card in battleState.player.hand)
            {
                GameObject cardObj = CreateCardUI(card);
                cardObj.transform.SetParent(handPanel);
                cardObj.transform.localScale = Vector3.one;
                
                // 添加点击事件
                Button cardButton = cardObj.GetComponent<Button>();
                cardButton.onClick.AddListener(() => OnCardClick(card));
            }
        }
        
        private void UpdateCardSlot()
        {
            // 清空横槽面板
            foreach (Transform child in cardSlotPanel)
            {
                Destroy(child.gameObject);
            }
            
            // 创建横槽卡牌
            for (int i = 0; i < BattleManager.CARD_SLOT_MAX_LENGTH; i++)
            {
                if (i < battleState.cardSlot.Count)
                {
                    Card card = battleState.cardSlot[i];
                    GameObject cardObj = CreateCardUI(card);
                    cardObj.transform.SetParent(cardSlotPanel);
                    cardObj.transform.localScale = Vector3.one * 0.8f;
                }
                else
                {
                    // 创建空槽位
                    GameObject emptySlot = new GameObject("EmptySlot");
                    emptySlot.transform.SetParent(cardSlotPanel);
                    emptySlot.transform.localScale = Vector3.one * 0.8f;
                    
                    Image slotImage = emptySlot.AddComponent<Image>();
                    slotImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                    
                    RectTransform rect = emptySlot.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(80, 120);
                }
            }
        }
        
        private void UpdateSkills()
        {
            // 清空技能面板
            foreach (Transform child in skillPanel)
            {
                Destroy(child.gameObject);
            }
            
            // 创建技能按钮
            foreach (Skill skill in battleState.skills)
            {
                GameObject skillObj = new GameObject(skill.name);
                skillObj.transform.SetParent(skillPanel);
                skillObj.transform.localScale = Vector3.one;
                
                Button skillButton = skillObj.AddComponent<Button>();
                Text skillText = skillObj.AddComponent<Text>();
                skillText.text = $"{skill.name}\n{string.Join("→", skill.effects)}";
                skillText.alignment = TextAnchor.MiddleCenter;
                
                // 添加点击事件
                skillButton.onClick.AddListener(() => OnSkillClick(skill));
            }
        }
        
        private void UpdateChargedSkill()
        {
            // 清空蓄力技能面板
            foreach (Transform child in chargedSkillPanel)
            {
                Destroy(child.gameObject);
            }
            
            // 创建蓄力技能按钮
            if (battleState.chargedSkill != null)
            {
                GameObject skillObj = new GameObject("ChargedSkill");
                skillObj.transform.SetParent(chargedSkillPanel);
                skillObj.transform.localScale = Vector3.one;
                
                Button skillButton = skillObj.AddComponent<Button>();
                Text skillText = skillObj.AddComponent<Text>();
                skillText.text = $"蓄力: {battleState.chargedSkill.name}";
                skillText.alignment = TextAnchor.MiddleCenter;
                
                // 添加点击事件
                skillButton.onClick.AddListener(() => battleManager.UseChargedSkill());
            }
        }
        
        private void UpdateMapUI()
        {
            // 清空地图节点面板
            foreach (Transform child in mapNodePanel)
            {
                Destroy(child.gameObject);
            }
            
            // 创建地图节点
            foreach (MapNode node in GameManager.Instance.mapNodes)
            {
                GameObject nodeObj = new GameObject(node.name);
                nodeObj.transform.SetParent(mapNodePanel);
                nodeObj.transform.localScale = Vector3.one;
                
                Button nodeButton = nodeObj.AddComponent<Button>();
                Text nodeText = nodeObj.AddComponent<Text>();
                nodeText.text = $"{node.name}\n{node.type}";
                nodeText.alignment = TextAnchor.MiddleCenter;
                
                // 设置按钮状态
                nodeButton.interactable = node.isUnlocked;
                
                // 添加点击事件
                int nodeId = node.id;
                nodeButton.onClick.AddListener(() => OnMapNodeClick(nodeId));
            }
        }
        
        private GameObject CreateCardUI(Card card)
        {
            GameObject cardObj = new GameObject(card.name);
            
            Button button = cardObj.AddComponent<Button>();
            Text text = cardObj.AddComponent<Text>();
            
            // 设置卡牌文本
            string cardTypeText = "";
            switch (card.type)
            {
                case CardType.Attack:
                    cardTypeText = "攻击";
                    text.color = Color.red;
                    break;
                case CardType.Defense:
                    cardTypeText = "防御";
                    text.color = Color.blue;
                    break;
                case CardType.Heal:
                    cardTypeText = "治疗";
                    text.color = Color.green;
                    break;
            }
            
            text.text = $"{card.name}\n{cardTypeText}: {card.value}\n费用: {card.cost}";
            text.alignment = TextAnchor.MiddleCenter;
            
            // 设置卡牌大小
            RectTransform rect = cardObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 150);
            
            return cardObj;
        }
        
        private void OnCardClick(Card card)
        {
            // 弹出选择位置的界面
            // 这里简化处理，直接放入最后位置
            battleManager.PlayCard(card, battleState.cardSlot.Count);
        }
        
        private void OnSkillClick(Skill skill)
        {
            // 弹出选择技能模式的界面
            // 这里简化处理，默认使用施放模式
            battleManager.UseSkill(skill, SkillMode.Cast);
        }
        
        private void OnMapNodeClick(int nodeId)
        {
            // 移动到节点
            GameManager.Instance.MoveToNode(nodeId);
            // 显示战斗界面
            ShowBattleUI();
        }
    }
}

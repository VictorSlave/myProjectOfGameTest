using UnityEngine;
using System.Collections.Generic;

namespace CardGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        public BattleState battleState;
        public List<MapNode> mapNodes;
        public int currentNodeId = 0;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeGame()
        {
            // 初始化玩家状态
            battleState = new BattleState();
            battleState.player = new PlayerState();
            
            // 初始化卡组
            InitializeDeck();
            
            // 初始化技能
            InitializeSkills();
            
            // 初始化地图
            InitializeMap();
            
            // 抽初始手牌
            DrawCards(5);
        }
        
        private void InitializeDeck()
        {
            // 创建基础卡组
            battleState.player.deck = new List<Card> {
                new Card { name = "攻击", type = CardType.Attack, value = 5 },
                new Card { name = "攻击", type = CardType.Attack, value = 5 },
                new Card { name = "攻击", type = CardType.Attack, value = 5 },
                new Card { name = "防御", type = CardType.Defense, value = 3 },
                new Card { name = "防御", type = CardType.Defense, value = 3 },
                new Card { name = "治疗", type = CardType.Heal, value = 4 },
                new Card { name = "治疗", type = CardType.Heal, value = 4 },
                new Card { name = "攻击", type = CardType.Attack, value = 3 },
                new Card { name = "防御", type = CardType.Defense, value = 5 },
                new Card { name = "攻击", type = CardType.Attack, value = 4 }
            };
            
            // 洗牌
            ShuffleDeck();
        }
        
        private void ShuffleDeck()
        {
            System.Random rng = new System.Random();
            int n = battleState.player.deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card temp = battleState.player.deck[k];
                battleState.player.deck[k] = battleState.player.deck[n];
                battleState.player.deck[n] = temp;
            }
        }
        
        private void InitializeSkills()
        {
            battleState.skills = new List<Skill> {
                new Skill {
                    name = "连击",
                    effects = new List<SkillEffectType> { SkillEffectType.Attack, SkillEffectType.Attack },
                    value = 8
                },
                new Skill {
                    name = "守护",
                    effects = new List<SkillEffectType> { SkillEffectType.Defense, SkillEffectType.Defense },
                    value = 6
                },
                new Skill {
                    name = "治疗术",
                    effects = new List<SkillEffectType> { SkillEffectType.Heal, SkillEffectType.Heal },
                    value = 10
                },
                new Skill {
                    name = "攻防一体",
                    effects = new List<SkillEffectType> { SkillEffectType.Attack, SkillEffectType.Defense },
                    value = 5
                }
            };
        }
        
        private void InitializeMap()
        {
            mapNodes = new List<MapNode> {
                new MapNode { id = 0, type = NodeType.Start, name = "起点", nextNodeIds = new List<int> { 1, 2 }, isUnlocked = true },
                new MapNode { id = 1, type = NodeType.Monster, name = "怪物1", nextNodeIds = new List<int> { 3, 4 } },
                new MapNode { id = 2, type = NodeType.Shop, name = "商店", nextNodeIds = new List<int> { 4, 5 } },
                new MapNode { id = 3, type = NodeType.Event, name = "事件", nextNodeIds = new List<int> { 6 } },
                new MapNode { id = 4, type = NodeType.Monster, name = "怪物2", nextNodeIds = new List<int> { 6 } },
                new MapNode { id = 5, type = NodeType.Monster, name = "怪物3", nextNodeIds = new List<int> { 6 } },
                new MapNode { id = 6, type = NodeType.Boss, name = "最终BOSS", nextNodeIds = new List<int>() }
            };
        }
        
        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (battleState.player.deck.Count == 0)
                {
                    // 牌库为空，从弃牌堆重新洗牌
                    battleState.player.deck.AddRange(battleState.player.discardPile);
                    battleState.player.discardPile.Clear();
                    ShuffleDeck();
                }
                
                if (battleState.player.deck.Count > 0)
                {
                    Card card = battleState.player.deck[0];
                    battleState.player.deck.RemoveAt(0);
                    battleState.player.hand.Add(card);
                }
            }
        }
        
        public void StartBattle(Monster monster)
        {
            battleState.monster = monster;
            battleState.cardSlot.Clear();
            battleState.chargedSkill = null;
            battleState.isPlayerTurn = true;
            battleState.turnCount = 1;
            
            // 重置费用
            battleState.player.currentMana = battleState.player.maxMana;
        }
        
        public void EndBattle(bool isVictory)
        {
            // 处理战斗结束逻辑
            if (isVictory)
            {
                // 解锁下一个节点
                MapNode currentNode = mapNodes.Find(n => n.id == currentNodeId);
                foreach (int nextId in currentNode.nextNodeIds)
                {
                    MapNode nextNode = mapNodes.Find(n => n.id == nextId);
                    if (nextNode != null)
                    {
                        nextNode.isUnlocked = true;
                    }
                }
            }
        }
        
        public void MoveToNode(int nodeId)
        {
            currentNodeId = nodeId;
            MapNode node = mapNodes.Find(n => n.id == nodeId);
            
            switch (node.type)
            {
                case NodeType.Monster:
                    // 生成怪物并开始战斗
                    Monster monster = new Monster {
                        name = node.name,
                        maxHealth = 30 + (currentNodeId * 5),
                        currentHealth = 30 + (currentNodeId * 5),
                        attack = 5 + (currentNodeId * 2),
                        defense = 2 + (currentNodeId)
                    };
                    StartBattle(monster);
                    break;
                
                case NodeType.Shop:
                    // 打开商店界面
                    break;
                
                case NodeType.Event:
                    // 触发随机事件
                    break;
                
                case NodeType.Boss:
                    // 生成BOSS并开始战斗
                    Monster boss = new Monster {
                        name = "最终BOSS",
                        maxHealth = 100,
                        currentHealth = 100,
                        attack = 15,
                        defense = 5
                    };
                    StartBattle(boss);
                    break;
            }
        }
    }
}

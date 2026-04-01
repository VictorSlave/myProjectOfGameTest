using System;
using System.Collections.Generic;

namespace CardGame
{
    // 卡牌类型
    public enum CardType
    {
        Attack,  // 攻击卡
        Defense, // 防御卡
        Heal     // 治疗卡
    }

    // 节点类型
    public enum NodeType
    {
        Start,      // 起点
        Monster,    // 怪物战斗
        Shop,       // 商店
        Event,      // 随机事件
        Boss        // 最终BOSS
    }

    // 技能效果类型
    public enum SkillEffectType
    {
        Attack,  // 攻击
        Defense, // 防御
        Heal     // 治疗
    }

    // 技能使用模式
    public enum SkillMode
    {
        Cast,    // 施放模式
        Charge   // 蓄力模式
    }

    // 卡牌类
    [Serializable]
    public class Card
    {
        public string name;
        public CardType type;
        public int value; // 攻击/防御/治疗值
        public int cost = 1; // 费用
    }

    // 技能类
    [Serializable]
    public class Skill
    {
        public string name;
        public List<SkillEffectType> effects;
        public int value; // 技能效果值
    }

    // 地图节点类
    [Serializable]
    public class MapNode
    {
        public int id;
        public NodeType type;
        public string name;
        public List<int> nextNodeIds;
        public bool isUnlocked = false;
    }

    // 怪物类
    [Serializable]
    public class Monster
    {
        public string name;
        public int maxHealth;
        public int currentHealth;
        public int attack;
        public int defense;
    }

    // 玩家状态类
    [Serializable]
    public class PlayerState
    {
        public int maxHealth = 100;
        public int currentHealth = 100;
        public int shield = 0;
        public int maxMana = 3;
        public int currentMana = 3;
        public List<Card> hand = new List<Card>();
        public List<Card> deck = new List<Card>();
        public List<Card> discardPile = new List<Card>();
    }

    // 战斗状态类
    [Serializable]
    public class BattleState
    {
        public PlayerState player;
        public Monster monster;
        public List<Card> cardSlot = new List<Card>(); // 横槽
        public List<Skill> skills = new List<Skill>(); // 可用技能
        public Skill chargedSkill = null; // 蓄力技能
        public bool isPlayerTurn = true;
        public int turnCount = 1;
    }
}

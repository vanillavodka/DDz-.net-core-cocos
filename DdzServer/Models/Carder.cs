using System;
using System.Collections.Generic;
using System.Linq;

namespace DdzServer.Models
{
    public class Carder
    {
        public List<Card> CardList { get; set; } = new List<Card>();
        private static readonly Dictionary<string, int> CardValue = new Dictionary<string, int>
        {
            {"A", 12}, {"2", 13}, {"3", 1}, {"4", 2}, {"5", 3}, {"6", 4}, {"7", 5}, {"8", 6}, {"9", 7}, {"10", 8}, {"J", 9}, {"Q", 10}, {"K", 11}
        };
        private static readonly Dictionary<string, int> CardShape = new Dictionary<string, int>
        {
            {"S", 1}, {"H", 2}, {"C", 3}, {"D", 4}
        };
        private static readonly Dictionary<string, int> Kings = new Dictionary<string, int>
        {
            {"kx", 14}, // 小王
            {"Kd", 15}  // 大王
        };

        public Carder()
        {
            CreateCards();
            ShuffleCards();
        }

        private void CreateCards()
        {
            foreach (var value in CardValue.Values)
            {
                foreach (var shape in CardShape.Values)
                {
                    var card = new Card { Value = value, Shape = shape };
                    card.Index = CardList.Count;
                    CardList.Add(card);
                }
            }
            foreach (var king in Kings.Values)
            {
                var card = new Card { King = king };
                card.Index = CardList.Count;
                CardList.Add(card);
            }
        }

        private void ShuffleCards()
        {
            var rand = new Random();
            for (int i = CardList.Count - 1; i >= 0; i--)
            {
                int randomIndex = rand.Next(i + 1);
                var tmpCard = CardList[randomIndex];
                CardList[randomIndex] = CardList[i];
                CardList[i] = tmpCard;
            }
        }

        // 分三份和底牌
        public List<List<Card>> SplitThreeCards()
        {
            var threeCards = new Dictionary<int, List<Card>>();
            for (int i = 0; i < 17; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (threeCards.ContainsKey(j))
                        threeCards[j].Add(CardList.Last());
                    else
                        threeCards[j] = new List<Card> { CardList.Last() };
                    CardList.RemoveAt(CardList.Count - 1);
                }
            }
            return new List<List<Card>> { threeCards[0], threeCards[1], threeCards[2], new List<Card>(CardList) };
        }

        // 是否对子
        public static bool IsDoubleCard(List<Card> cardList)
        {
            if (cardList.Count != 2) return false;
            if (!cardList[0].Value.HasValue || cardList[0].Value != cardList[1].Value) return false;
            return true;
        }

        // 三张不带
        public static bool IsThree(List<Card> cardList)
        {
            if (cardList.Count != 3) return false;
            if (!cardList[0].Value.HasValue || !cardList[1].Value.HasValue || !cardList[2].Value.HasValue) return false;
            if (cardList[0].Value != cardList[1].Value) return false;
            if (cardList[0].Value != cardList[2].Value) return false;
            return true;
        }

        // 三带一
        public static bool IsThreeAndOne(List<Card> cardList)
        {
            if (cardList.Count != 4) return false;
            if (!cardList[1].Value.HasValue || !cardList[2].Value.HasValue) return false;
            if (cardList[0].Value == cardList[1].Value && cardList[1].Value == cardList[2].Value) return true;
            if (cardList[1].Value == cardList[2].Value && cardList[2].Value == cardList[3].Value) return true;
            return false;
        }

        // 三带二
        public static bool IsThreeAndTwo(List<Card> cardList)
        {
            if (cardList.Count != 5) return false;
            if (cardList[0].Value == cardList[1].Value && cardList[1].Value == cardList[2].Value && cardList[3].Value == cardList[4].Value) return true;
            if (cardList[2].Value == cardList[3].Value && cardList[3].Value == cardList[4].Value && cardList[0].Value == cardList[1].Value) return true;
            return false;
        }

        // 四张炸弹
        public static bool IsBoom(List<Card> cardList)
        {
            if (cardList.Count != 4) return false;
            var map = new Dictionary<int, int>();
            foreach (var card in cardList)
            {
                if (!card.Value.HasValue) return false;
                if (map.ContainsKey(card.Value.Value)) map[card.Value.Value]++;
                else map[card.Value.Value] = 1;
            }
            return map.Count == 1;
        }

        // 王炸
        public static bool IsKingBoom(List<Card> cardList)
        {
            if (cardList.Count != 2) return false;
            if (cardList[0].King.HasValue && cardList[1].King.HasValue) return true;
            return false;
        }

        // 顺子
        public static bool IsShunzi(List<Card> cardList)
        {
            if (cardList.Count < 5) return false;
            var values = cardList.Where(c => c.Value.HasValue).Select(c => c.Value.Value).OrderBy(v => v).ToList();
            for (int i = 1; i < values.Count; i++)
            {
                if (values[i] != values[i - 1] + 1) return false;
                if (values[i] > 13) return false; // 不能有2和王
            }
            return true;
        }

        // 连对
        public static bool IsLianDui(List<Card> cardList)
        {
            if (cardList.Count < 6 || cardList.Count % 2 != 0) return false;
            var values = cardList.Where(c => c.Value.HasValue).Select(c => c.Value.Value).OrderBy(v => v).ToList();
            for (int i = 0; i < values.Count; i += 2)
            {
                if (i + 1 >= values.Count || values[i] != values[i + 1]) return false;
                if (i > 0 && values[i] != values[i - 2] + 1) return false;
                if (values[i] > 13) return false; // 不能有2和王
            }
            return true;
        }

        // 飞机不带
        public static bool IsPlane(List<Card> cardList)
        {
            if (cardList.Count < 6 || cardList.Count % 3 != 0) return false;
            var map = new Dictionary<int, int>();
            foreach (var card in cardList)
            {
                if (!card.Value.HasValue) return false;
                if (map.ContainsKey(card.Value.Value)) map[card.Value.Value]++;
                else map[card.Value.Value] = 1;
            }
            var threes = map.Where(kv => kv.Value == 3).Select(kv => kv.Key).OrderBy(v => v).ToList();
            if (threes.Count < 2) return false;
            for (int i = 1; i < threes.Count; i++)
            {
                if (threes[i] != threes[i - 1] + 1) return false;
                if (threes[i] > 13) return false;
            }
            return true;
        }

        // 飞机带单
        public static bool IsPlaneWithSingle(List<Card> cardList)
        {
            if (cardList.Count < 8 || cardList.Count % 4 != 0) return false;
            var map = new Dictionary<int, int>();
            foreach (var card in cardList)
            {
                if (!card.Value.HasValue) return false;
                if (map.ContainsKey(card.Value.Value)) map[card.Value.Value]++;
                else map[card.Value.Value] = 1;
            }
            var threes = map.Where(kv => kv.Value == 3).Select(kv => kv.Key).OrderBy(v => v).ToList();
            if (threes.Count < 2) return false;
            for (int i = 1; i < threes.Count; i++)
            {
                if (threes[i] != threes[i - 1] + 1) return false;
                if (threes[i] > 13) return false;
            }
            return true;
        }

        // 飞机带对
        public static bool IsPlaneWithDouble(List<Card> cardList)
        {
            if (cardList.Count < 10 || cardList.Count % 5 != 0) return false;
            var map = new Dictionary<int, int>();
            foreach (var card in cardList)
            {
                if (!card.Value.HasValue) return false;
                if (map.ContainsKey(card.Value.Value)) map[card.Value.Value]++;
                else map[card.Value.Value] = 1;
            }
            var threes = map.Where(kv => kv.Value == 3).Select(kv => kv.Key).OrderBy(v => v).ToList();
            var pairs = map.Where(kv => kv.Value == 2).Select(kv => kv.Key).ToList();
            if (threes.Count < 2 || pairs.Count < 2) return false;
            for (int i = 1; i < threes.Count; i++)
            {
                if (threes[i] != threes[i - 1] + 1) return false;
                if (threes[i] > 13) return false;
            }
            return true;
        }

        // 单牌比较
        public static int CompareOne(Card a, Card b)
        {
            int va = a.King ?? a.Value ?? 0;
            int vb = b.King ?? b.Value ?? 0;
            return va.CompareTo(vb);
        }

        // 对子比较
        public static int CompareDouble(List<Card> a, List<Card> b)
        {
            int va = a[0].Value ?? 0;
            int vb = b[0].Value ?? 0;
            return va.CompareTo(vb);
        }

        // 三张比较
        public static int CompareThree(List<Card> a, List<Card> b)
        {
            int va = a[0].Value ?? 0;
            int vb = b[0].Value ?? 0;
            return va.CompareTo(vb);
        }

        // 炸弹比较
        public static int CompareBoom(List<Card> a, List<Card> b)
        {
            int va = a[0].Value ?? 0;
            int vb = b[0].Value ?? 0;
            return va.CompareTo(vb);
        }

        // 王炸比较
        public static int CompareKingBoom(List<Card> a, List<Card> b)
        {
            // 王炸无比较，平手
            return 0;
        }

        // 飞机比较（不带）
        public static int ComparePlane(List<Card> a, List<Card> b)
        {
            var mapA = a.GroupBy(c => c.Value).Where(g => g.Count() == 3).Select(g => g.Key ?? 0).OrderBy(x => x).ToList();
            var mapB = b.GroupBy(c => c.Value).Where(g => g.Count() == 3).Select(g => g.Key ?? 0).OrderBy(x => x).ToList();
            if (mapA.Count == 0 || mapB.Count == 0) return 0;
            return mapA[0].CompareTo(mapB[0]);
        }

        // 牌型判断等方法可继续迁移...
    }
} 
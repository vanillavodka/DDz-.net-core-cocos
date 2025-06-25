namespace DdzServer.Models
{
    public class Player
    {
        public string AccountId { get; set; }
        public string NickName { get; set; }
        public string AvatarUrl { get; set; }
        public int Gold { get; set; }
        public int SeatIndex { get; set; }
        public bool IsReady { get; set; }
        public List<Card> HandCards { get; set; } = new List<Card>();
        // 其他属性可根据后续迁移补充

        // 玩家准备
        public void Ready()
        {
            IsReady = true;
            // 可扩展：通知房间/服务
        }

        // 玩家出牌
        public bool PlayCards(List<Card> cards)
        {
            // 校验玩家手牌是否包含所出牌
            foreach (var card in cards)
            {
                if (!HandCards.Any(c => c.Value == card.Value && c.Shape == card.Shape && c.King == card.King))
                    return false;
            }
            // 移除已出的牌
            foreach (var card in cards)
            {
                var toRemove = HandCards.FirstOrDefault(c => c.Value == card.Value && c.Shape == card.Shape && c.King == card.King);
                if (toRemove != null) HandCards.Remove(toRemove);
            }
            return true;
        }

        // 状态变更
        public void ChangeStatus(bool isReady)
        {
            IsReady = isReady;
        }
    }
} 
namespace DdzServer.Models
{
    public class Card
    {
        public int? Value { get; set; } // 点数（A=12, 2=13, 3=1...）
        public int? Shape { get; set; } // 花色（1=黑桃, 2=红桃, 3=梅花, 4=方片）
        public int? King { get; set; }  // 王（14=小王, 15=大王）
        public int Index { get; set; }  // 牌在牌堆中的索引
        // 可根据后续迁移补充更多属性
    }
} 
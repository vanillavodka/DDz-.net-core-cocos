namespace DdzServer.Models
{
    public enum RoomState
    {
        ROOM_INVALID = -1,
        ROOM_WAITREADY = 1,   // 等待游戏
        ROOM_GAMESTART = 2,   // 开始游戏
        ROOM_PUSHCARD = 3,    // 发牌
        ROOM_ROBSTATE = 4,    // 抢地主
        ROOM_SHOWBOTTOMCARD = 5, // 显示底牌
        ROOM_PLAYING = 6      // 出牌阶段
    }
} 
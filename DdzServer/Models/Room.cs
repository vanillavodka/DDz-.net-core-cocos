using System;
using System.Collections.Generic;
using System.Linq;

namespace DdzServer.Models
{
    public class Room
    {
        public string RoomId { get; set; }
        public List<Player> PlayerList { get; set; } = new List<Player>();
        public Player Owner { get; set; } // 房主
        public int Bottom { get; set; }
        public int Rate { get; set; }
        public int Gold { get; set; }
        public Player HouseManager { get; set; }
        public RoomState State { get; set; } = RoomState.ROOM_INVALID;
        public Carder Carder { get; set; } // 发牌器
        public Player LostPlayer { get; set; }
        public List<Player> RobPlayers { get; set; } = new List<Player>();
        public Player RoomMaster { get; set; }
        public List<int> ThreeCards { get; set; } = new List<int>();
        public List<Player> PlayingCards { get; set; } = new List<Player>();
        public List<int> CurPushCardList { get; set; } = new List<int>();
        public List<int> LastPushCardList { get; set; } = new List<int>();
        public string LastPushCardAccountId { get; set; }

        // 构造方法
        public Room(int rate, Player owner)
        {
            this.RoomId = GetRandomStr(6);
            this.Owner = owner;
            this.HouseManager = owner;
            this.PlayerList = new List<Player>();
            this.State = RoomState.ROOM_INVALID;
            // tconfig 相关属性后续迁移 config 逻辑时补充
        }

        // 静态工具方法：获取空座位号
        public static int GetSeatIndex(List<Player> playerList)
        {
            int seatIndex = 1;
            if (playerList.Count == 0) return seatIndex;
            int index = 1;
            foreach (var player in playerList)
            {
                if (index != player.SeatIndex)
                    return index;
                index++;
            }
            return index;
        }

        // 静态工具方法：生成随机数字字符串
        public static string GetRandomStr(int count)
        {
            var rand = new Random();
            string str = "";
            for (int i = 0; i < count; i++)
            {
                str += rand.Next(0, 10).ToString();
            }
            return str;
        }

        // 加入玩家
        public void JoinPlayer(Player player)
        {
            if (player != null)
            {
                player.SeatIndex = GetSeatIndex(PlayerList);
                // 广播给房间其他用户（后续用 SignalR 实现）
                // foreach (var p in PlayerList) { ... }
                PlayerList.Add(player);
            }
        }

        // 进入房间
        public List<object> EnterRoom(Player player)
        {
            var playerData = new List<object>();
            foreach (var p in PlayerList)
            {
                var data = new
                {
                    accountid = p.AccountId,
                    nick_name = p.NickName,
                    avatarUrl = p.AvatarUrl,
                    goldcount = p.Gold,
                    seatindex = p.SeatIndex,
                    isready = p.IsReady
                };
                playerData.Add(data);
            }
            // 返回房间内所有玩家数据，供回调/推送
            return playerData;
        }

        // 玩家掉线
        public void PlayerOffLine(Player player)
        {
            for (int i = 0; i < PlayerList.Count; i++)
            {
                if (PlayerList[i].AccountId == player.AccountId)
                {
                    PlayerList.RemoveAt(i);
                    // 判断是否为房主掉线
                    if (HouseManager.AccountId == player.AccountId && PlayerList.Count >= 1)
                    {
                        ChangeHouseManager(PlayerList[0]);
                    }
                    break;
                }
            }
        }

        // 玩家准备
        public void PlayerReady(Player player)
        {
            player.IsReady = true;
            // 广播给房间所有用户（后续用 SignalR 实现）
        }

        // 重新设置房主
        public void ChangeHouseManager(Player player)
        {
            if (player != null)
            {
                HouseManager = player;
                // 广播房主变更（后续用 SignalR 实现）
            }
        }

        // 发牌
        public void DealCards(Carder carder)
        {
            this.Carder = carder;
            var cards = carder.SplitThreeCards();
            for (int i = 0; i < PlayerList.Count && i < 3; i++)
            {
                // 假设 Player 有 HandCards 属性
                PlayerList[i].HandCards = cards[i];
            }
            ThreeCards = cards[3].Select(c => c.Value ?? c.King ?? 0).ToList();
            State = RoomState.ROOM_PUSHCARD;
        }

        // 抢地主流程（简化版，详细流程可根据原JS补充）
        public void StartRobMaster()
        {
            State = RoomState.ROOM_ROBSTATE;
            // 广播可抢地主玩家（SignalR实现）
        }

        // 确定地主
        public void SetMaster(Player master)
        {
            RoomMaster = master;
            State = RoomState.ROOM_SHOWBOTTOMCARD;
            // 广播地主和底牌（SignalR实现）
        }

        // 出牌
        public void PlayCards(Player player, List<Card> cards)
        {
            // 业务校验、牌型判断、轮转等
            // 广播出牌结果（SignalR实现）
        }

        // 结算
        public void Settle()
        {
            State = RoomState.ROOM_INVALID;
            // 结算逻辑、金币结算、广播结果等
        }

        // 房间状态切换
        public void ChangeState(RoomState newState)
        {
            State = newState;
            // 广播房间状态（SignalR实现）
        }

        // 玩家进房
        public void PlayerJoin(Player player)
        {
            JoinPlayer(player);
            // 广播玩家加入（SignalR实现）
        }

        // 玩家离房
        public void PlayerLeave(Player player)
        {
            PlayerOffLine(player);
            // 广播玩家离开（SignalR实现）
        }
    }
} 
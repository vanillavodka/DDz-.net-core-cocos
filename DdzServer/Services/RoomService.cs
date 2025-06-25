using System.Collections.Generic;
using System.Linq;
using DdzServer.Models;

namespace DdzServer.Services
{
    public class RoomService
    {
        private readonly List<Room> _rooms = new List<Room>();

        // 创建房间
        public Room CreateRoom(int rate, Player owner)
        {
            var room = new Room(rate, owner);
            _rooms.Add(room);
            return room;
        }

        // 查找房间
        public Room FindRoom(string roomId)
        {
            return _rooms.FirstOrDefault(r => r.RoomId == roomId);
        }

        // 销毁房间
        public void DestroyRoom(string roomId)
        {
            var room = FindRoom(roomId);
            if (room != null)
                _rooms.Remove(room);
        }

        // 玩家加入房间
        public bool JoinRoom(string roomId, Player player)
        {
            var room = FindRoom(roomId);
            if (room == null) return false;
            room.PlayerJoin(player);
            return true;
        }

        // 玩家离开房间
        public bool LeaveRoom(string roomId, Player player)
        {
            var room = FindRoom(roomId);
            if (room == null) return false;
            room.PlayerLeave(player);
            return true;
        }

        // 游戏流程调度（如发牌、抢地主、出牌、结算等）
        public void StartGame(string roomId, Carder carder)
        {
            var room = FindRoom(roomId);
            if (room == null) return;
            room.DealCards(carder);
            room.StartRobMaster();
        }

        public void SetMaster(string roomId, Player master)
        {
            var room = FindRoom(roomId);
            if (room == null) return;
            room.SetMaster(master);
        }

        public void PlayCards(string roomId, Player player, List<Card> cards)
        {
            var room = FindRoom(roomId);
            if (room == null) return;
            room.PlayCards(player, cards);
        }

        public void Settle(string roomId)
        {
            var room = FindRoom(roomId);
            if (room == null) return;
            room.Settle();
        }
    }
} 
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using DdzServer.Models;
using DdzServer.Services;
using System.Collections.Generic;
using System.Text.Json;

namespace DdzServer.Hubs
{
    public class GameHub : Hub
    {
        private readonly RoomService _roomService;
        private readonly Carder _carder;

        public GameHub(RoomService roomService, Carder carder)
        {
            _roomService = roomService;
            _carder = carder;
        }

        // Socket.IO 兼容的通用消息处理方法
        public async Task Notify(string message)
        {
            try
            {
                var request = JsonSerializer.Deserialize<SocketRequest>(message);
                
                switch (request.Cmd)
                {
                    case "wxlogin":
                        await HandleWxLogin(request.Data, request.CallIndex);
                        break;
                    case "createroom_req":
                        await HandleCreateRoom(request.Data, request.CallIndex);
                        break;
                    case "joinroom_req":
                        await HandleJoinRoom(request.Data, request.CallIndex);
                        break;
                    case "enterroom_req":
                        await HandleEnterRoom(request.Data, request.CallIndex);
                        break;
                    case "player_ready_notify":
                        await HandlePlayerReady(request.Data);
                        break;
                    case "player_start_notify":
                        await HandlePlayerStart(request.Data, request.CallIndex);
                        break;
                    case "player_rob_notify":
                        await HandlePlayerRob(request.Data);
                        break;
                    case "chu_card_req":
                        await HandleChuCard(request.Data, request.CallIndex);
                        break;
                    case "chu_bu_card_req":
                        await HandleBuChuCard(request.Data, request.CallIndex);
                        break;
                    default:
                        await Clients.Caller.SendAsync("notify", new { 
                            type = "error", 
                            result = -1, 
                            data = "Unknown command", 
                            callBackIndex = request.CallIndex 
                        });
                        break;
                }
            }
            catch (System.Exception ex)
            {
                await Clients.Caller.SendAsync("notify", new { 
                    type = "error", 
                    result = -1, 
                    data = ex.Message, 
                    callBackIndex = 0 
                });
            }
        }

        // 微信登录
        private async Task HandleWxLogin(object data, int? callIndex)
        {
            // 模拟登录成功
            var response = new { 
                type = "wxlogin", 
                result = 0, 
                data = new { 
                    accountid = "test_user_" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
                    nick_name = "测试用户",
                    avatarUrl = "avatar_1",
                    goldcount = 1000
                }, 
                callBackIndex = callIndex 
            };
            await Clients.Caller.SendAsync("notify", response);
        }

        // 创建房间
        private async Task HandleCreateRoom(object data, int? callIndex)
        {
            var roomData = JsonSerializer.Deserialize<CreateRoomRequest>(JsonSerializer.Serialize(data));
            var player = new Player { 
                AccountId = roomData.accountid, 
                NickName = roomData.nick_name,
                AvatarUrl = roomData.avatarUrl,
                Gold = roomData.goldcount
            };
            
            var room = _roomService.CreateRoom(roomData.rate, player);
            
            var response = new { 
                type = "createroom_req", 
                result = 0, 
                data = new { 
                    roomid = room.RoomId,
                    rate = room.Rate
                }, 
                callBackIndex = callIndex 
            };
            await Clients.Caller.SendAsync("notify", response);
        }

        // 加入房间
        private async Task HandleJoinRoom(object data, int? callIndex)
        {
            var joinData = JsonSerializer.Deserialize<JoinRoomRequest>(JsonSerializer.Serialize(data));
            var player = new Player { 
                AccountId = joinData.accountid, 
                NickName = joinData.nick_name,
                AvatarUrl = joinData.avatarUrl,
                Gold = joinData.goldcount
            };
            
            var success = _roomService.JoinRoom(joinData.roomid, player);
            
            if (success)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, joinData.roomid);
                
                var response = new { 
                    type = "joinroom_req", 
                    result = 0, 
                    data = new { 
                        roomid = joinData.roomid,
                        success = true
                    }, 
                    callBackIndex = callIndex 
                };
                await Clients.Caller.SendAsync("notify", response);
                
                // 通知房间其他玩家
                await Clients.Group(joinData.roomid).SendAsync("notify", new { 
                    type = "player_joinroom_notify", 
                    result = 0, 
                    data = new { 
                        accountid = player.AccountId,
                        nick_name = player.NickName,
                        avatarUrl = player.AvatarUrl,
                        goldcount = player.Gold,
                        seatindex = player.SeatIndex,
                        isready = player.IsReady
                    }, 
                    callBackIndex = (int?)null 
                });
            }
            else
            {
                var response = new { 
                    type = "joinroom_req", 
                    result = -1, 
                    data = "Room not found", 
                    callBackIndex = callIndex 
                };
                await Clients.Caller.SendAsync("notify", response);
            }
        }

        // 进入房间
        private async Task HandleEnterRoom(object data, int? callIndex)
        {
            var enterData = JsonSerializer.Deserialize<EnterRoomRequest>(JsonSerializer.Serialize(data));
            var room = _roomService.FindRoom(enterData.roomid);
            
            if (room != null)
            {
                var playerData = room.EnterRoom(new Player { AccountId = enterData.accountid });
                
                var response = new { 
                    type = "enterroom_req", 
                    result = 0, 
                    data = new { 
                        roomid = room.RoomId,
                        players = playerData
                    }, 
                    callBackIndex = callIndex 
                };
                await Clients.Caller.SendAsync("notify", response);
            }
            else
            {
                var response = new { 
                    type = "enterroom_req", 
                    result = -1, 
                    data = "Room not found", 
                    callBackIndex = callIndex 
                };
                await Clients.Caller.SendAsync("notify", response);
            }
        }

        // 玩家准备
        private async Task HandlePlayerReady(object data)
        {
            var readyData = JsonSerializer.Deserialize<PlayerReadyRequest>(JsonSerializer.Serialize(data));
            var room = _roomService.FindRoom(readyData.roomid);
            
            if (room != null)
            {
                var player = room.PlayerList.FirstOrDefault(p => p.AccountId == readyData.accountid);
                if (player != null)
                {
                    player.Ready();
                    await Clients.Group(readyData.roomid).SendAsync("notify", new { 
                        type = "player_ready_notify", 
                        result = 0, 
                        data = new { 
                            accountid = player.AccountId,
                            isready = player.IsReady
                        }, 
                        callBackIndex = (int?)null 
                    });
                }
            }
        }

        // 开始游戏
        private async Task HandlePlayerStart(object data, int? callIndex)
        {
            var startData = JsonSerializer.Deserialize<PlayerStartRequest>(JsonSerializer.Serialize(data));
            var room = _roomService.FindRoom(startData.roomid);
            
            if (room != null && room.PlayerList.Count >= 3)
            {
                _roomService.StartGame(startData.roomid, _carder);
                
                var response = new { 
                    type = "player_start_notify", 
                    result = 0, 
                    data = new { 
                        roomid = room.RoomId,
                        success = true
                    }, 
                    callBackIndex = callIndex 
                };
                await Clients.Caller.SendAsync("notify", response);
                
                // 通知游戏开始
                await Clients.Group(startData.roomid).SendAsync("notify", new { 
                    type = "gameStart_notify", 
                    result = 0, 
                    data = new { 
                        roomid = room.RoomId
                    }, 
                    callBackIndex = (int?)null 
                });
                
                // 发牌通知
                await Clients.Group(startData.roomid).SendAsync("notify", new { 
                    type = "pushcard_notify", 
                    result = 0, 
                    data = new { 
                        roomid = room.RoomId,
                        cards = room.PlayerList.Select(p => new { 
                            accountid = p.AccountId,
                            cards = p.HandCards.Select(c => c.Value ?? c.King ?? 0).ToList()
                        }).ToList()
                    }, 
                    callBackIndex = (int?)null 
                });
            }
            else
            {
                var response = new { 
                    type = "player_start_notify", 
                    result = -1, 
                    data = "Cannot start game", 
                    callBackIndex = callIndex 
                };
                await Clients.Caller.SendAsync("notify", response);
            }
        }

        // 抢地主
        private async Task HandlePlayerRob(object data)
        {
            var robData = JsonSerializer.Deserialize<PlayerRobRequest>(JsonSerializer.Serialize(data));
            var room = _roomService.FindRoom(robData.roomid);
            
            if (room != null)
            {
                var player = room.PlayerList.FirstOrDefault(p => p.AccountId == robData.accountid);
                if (player != null)
                {
                    room.RobPlayers.Add(player);
                    
                    await Clients.Group(robData.roomid).SendAsync("notify", new { 
                        type = "canrob_state_notify", 
                        result = 0, 
                        data = new { 
                            accountid = player.AccountId,
                            state = robData.state
                        }, 
                        callBackIndex = (int?)null 
                    });
                }
            }
        }

        // 出牌
        private async Task HandleChuCard(object data, int? callIndex)
        {
            var cardData = JsonSerializer.Deserialize<ChuCardRequest>(JsonSerializer.Serialize(data));
            var room = _roomService.FindRoom(cardData.roomid);
            
            if (room != null)
            {
                var player = room.PlayerList.FirstOrDefault(p => p.AccountId == cardData.accountid);
                if (player != null)
                {
                    var cards = cardData.cards.Select(c => new Card { Value = c }).ToList();
                    var success = player.PlayCards(cards);
                    
                    if (success)
                    {
                        var response = new { 
                            type = "chu_card_req", 
                            result = 0, 
                            data = new { 
                                success = true
                            }, 
                            callBackIndex = callIndex 
                        };
                        await Clients.Caller.SendAsync("notify", response);
                        
                        // 通知其他玩家出牌
                        await Clients.Group(cardData.roomid).SendAsync("notify", new { 
                            type = "other_chucard_notify", 
                            result = 0, 
                            data = new { 
                                accountid = player.AccountId,
                                cards = cardData.cards
                            }, 
                            callBackIndex = (int?)null 
                        });
                    }
                    else
                    {
                        var response = new { 
                            type = "chu_card_req", 
                            result = -1, 
                            data = "Invalid cards", 
                            callBackIndex = callIndex 
                        };
                        await Clients.Caller.SendAsync("notify", response);
                    }
                }
            }
        }

        // 不出牌
        private async Task HandleBuChuCard(object data, int? callIndex)
        {
            var buCardData = JsonSerializer.Deserialize<BuChuCardRequest>(JsonSerializer.Serialize(data));
            var room = _roomService.FindRoom(buCardData.roomid);
            
            if (room != null)
            {
                var response = new { 
                    type = "chu_bu_card_req", 
                    result = 0, 
                    data = new { 
                        success = true
                    }, 
                    callBackIndex = callIndex 
                };
                await Clients.Caller.SendAsync("notify", response);
                
                // 通知其他玩家不出牌
                await Clients.Group(buCardData.roomid).SendAsync("notify", new { 
                    type = "other_chucard_notify", 
                    result = 0, 
                    data = new { 
                        accountid = buCardData.accountid,
                        cards = new List<int>()
                    }, 
                    callBackIndex = (int?)null 
                });
            }
        }

        // 连接建立
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("notify", new { 
                type = "connection", 
                result = 0, 
                data = "Connected successfully", 
                callBackIndex = (int?)null 
            });
            await base.OnConnectedAsync();
        }

        // 连接断开
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    // 请求模型类
    public class SocketRequest
    {
        public string Cmd { get; set; }
        public object Data { get; set; }
        public int? CallIndex { get; set; }
    }

    public class CreateRoomRequest
    {
        public string accountid { get; set; }
        public string nick_name { get; set; }
        public string avatarUrl { get; set; }
        public int goldcount { get; set; }
        public int rate { get; set; }
    }

    public class JoinRoomRequest
    {
        public string roomid { get; set; }
        public string accountid { get; set; }
        public string nick_name { get; set; }
        public string avatarUrl { get; set; }
        public int goldcount { get; set; }
    }

    public class EnterRoomRequest
    {
        public string roomid { get; set; }
        public string accountid { get; set; }
    }

    public class PlayerReadyRequest
    {
        public string roomid { get; set; }
        public string accountid { get; set; }
    }

    public class PlayerStartRequest
    {
        public string roomid { get; set; }
        public string accountid { get; set; }
    }

    public class PlayerRobRequest
    {
        public string roomid { get; set; }
        public string accountid { get; set; }
        public int state { get; set; }
    }

    public class ChuCardRequest
    {
        public string roomid { get; set; }
        public string accountid { get; set; }
        public List<int> cards { get; set; }
    }

    public class BuChuCardRequest
    {
        public string roomid { get; set; }
        public string accountid { get; set; }
    }
} 
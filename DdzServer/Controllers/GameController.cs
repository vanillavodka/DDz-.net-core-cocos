using Microsoft.AspNetCore.Mvc;
using DdzServer.Services;
using DdzServer.Models;
using System.Collections.Generic;

namespace DdzServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly RoomService _roomService;

        public GameController(RoomService roomService)
        {
            _roomService = roomService;
        }

        // 获取房间列表
        [HttpGet("rooms")]
        public IActionResult GetRooms()
        {
            // 这里应该从数据库获取房间列表
            // 暂时返回空列表
            return Ok(new { rooms = new List<object>() });
        }

        // 获取房间信息
        [HttpGet("rooms/{roomId}")]
        public IActionResult GetRoom(string roomId)
        {
            var room = _roomService.FindRoom(roomId);
            if (room == null)
                return NotFound(new { message = "Room not found" });

            return Ok(new { 
                roomid = room.RoomId,
                rate = room.Rate,
                state = room.State,
                playercount = room.PlayerList.Count
            });
        }

        // 健康检查
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = System.DateTime.UtcNow });
        }
    }
} 
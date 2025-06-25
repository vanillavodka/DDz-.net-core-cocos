using Microsoft.EntityFrameworkCore;
using DdzServer.Models;

namespace DdzServer.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<Room> Rooms { get; set; }
        // public DbSet<GameRecord> GameRecords { get; set; } // 可扩展战绩表

        // 可根据需要配置表结构和关系
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // 配置Player表
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.AccountId);
                entity.Property(e => e.AccountId).HasMaxLength(50);
                entity.Property(e => e.NickName).HasMaxLength(50);
                entity.Property(e => e.AvatarUrl).HasMaxLength(200);
            });

            // 配置Room表
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomId);
                entity.Property(e => e.RoomId).HasMaxLength(20);
                entity.Property(e => e.State).HasConversion<int>();
            });
        }
    }
} 
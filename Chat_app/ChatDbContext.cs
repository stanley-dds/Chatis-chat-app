using Chat_app.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat_app
{
    public class ChatDbContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }

        public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)  // Передаем настройки контекста в базовый класс
        {
        }        

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=chat.db");
    }

    //public class Message
    //{
    //    public int Id { get; set; }
    //    public string User { get; set; }
    //    public string Text { get; set; }
    //    public DateTime Timestamp { get; set; }
    //}
}

using Microsoft.EntityFrameworkCore;

namespace Chat_app
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)  // Передаем настройки контекста в базовый класс
        {
        }

        public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=chat.db");
    }

    public class Message
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

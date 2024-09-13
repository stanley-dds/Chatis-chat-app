
using Microsoft.EntityFrameworkCore;

namespace Chat_app
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();           

            // ??????????? ????????? ???? ?????? ChatDbContext
            builder.Services.AddDbContext<ChatDbContext>(options =>
                options.UseSqlite("Data Source=chat.db"));

            builder.Services.AddSignalR();


            var app = builder.Build();

            app.UseStaticFiles();
            // routing for SignalR
            app.MapHub<ChatHub>("/chatHub");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();




            app.Run();
        }
    }
}

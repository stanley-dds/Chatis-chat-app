
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

            // Регистрация контекста базы данных ChatDbContext
            builder.Services.AddDbContext<ChatDbContext>(options =>
                options.UseSqlite("Data Source=chat.db"));

            builder.Services.AddSignalR();



            // Добавляем сервисы аутентификации JWT

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "http://localhost",  // создатель
                    ValidAudience = "chat_app_client", // аудитория
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("777")) // ключ шифрования
                };
            });

            builder.Services.AddAuthorization();


            var app = builder.Build();

            // Включаем аутентификацию и авторизацию
            app.UseAuthentication();
            app.UseAuthorization();

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

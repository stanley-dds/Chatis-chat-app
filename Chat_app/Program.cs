using Chat_app;
using Chat_app.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(origin => true) // Allow any origin
                .AllowCredentials();
        });
});

// 2. Configure Database (SQLite)
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));

// 3. Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "http://localhost",
        ValidAudience = "chat_app_client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TheeeeeSuperLongSecretKey1234567890"))
    };

    // Support for passing token in SignalR hub requests
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Check if the request is for the SignalR hub
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

// 4. Add SignalR
builder.Services.AddSignalR();

// 5. Add Controllers
builder.Services.AddControllers();

// 6. Add Swagger for API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 7. Configure Ports from launchSettings.json
builder.WebHost.UseUrls("https://localhost:7024", "http://localhost:5024");

// 8. Build Application
var app = builder.Build();

// 9. Use Swagger in Development Environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat App API V1");
        c.RoutePrefix = "swagger"; // Swagger available at /swagger
    });
}

// 10. Use CORS
app.UseCors("AllowAll");

// 11. Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// 12. Serve Static Files
app.UseStaticFiles();

// 13. Set Default Route to index.html
app.MapFallbackToFile("index.html");

// 14. Map Controllers and SignalR Hub
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

// 15. Run Application
app.Run();

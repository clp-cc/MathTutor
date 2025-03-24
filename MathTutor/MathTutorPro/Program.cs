
using MathTutorPro.DAL;
using MathTutorPro.Models.Entities;
using MathTutorPro.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MathTutorPro
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // 添加数据库上下文
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
            // 配置 Identity
            builder.Services
                .AddIdentity<User, IdentityRole>(options =>
                {
                    options.Password.RequireNonAlphanumeric = false;  // 不需要非字母数字字符
                    options.Password.RequireUppercase = false;        // 需要大写字母
                    options.Password.RequiredLength = 6;            // 最小长度
                    options.Password.RequireDigit = true;           // 需要数字


                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // 添加认证和授权
            // JWT 认证
            builder.Services.AddAuthentication(options =>
            {
                // 设置默认认证方案为 JWT
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // 是否验证 Issuer（颁发者）
                    ValidateIssuer = true,
                    // 合法的 Issuer（需与 appsettings.json 中的配置一致）
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],

                    // 是否验证 Audience（接收者）
                    ValidateAudience = true,
                    // 合法的 Audience
                    ValidAudience = builder.Configuration["Jwt:Audience"],

                    // 是否验证密钥
                    ValidateIssuerSigningKey = true,
                    // 密钥（从配置中读取并转换为字节数组）
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
                    ),

                    // 是否验证 Token 有效期
                    ValidateLifetime = true
                };
            });
            builder.Services.AddAuthorization();

            // 添加 CORS 策略
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueApp", builder =>
                {
                    builder.WithOrigins("http://localhost:9528") // 前端地址
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials(); // 若前端需要发送凭证（如 cookies）
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddScoped<JwtHelper>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();

            app.UseHttpsRedirection();


            // 启用 CORS（注意顺序：在 Routing 之后，Authorization 之前）
            app.UseCors("AllowVueApp");

            // 必须按顺序配置！
            app.UseAuthentication(); // 先认证
            app.UseAuthorization();  // 后授权


            app.MapControllers();

            app.Run();
        }
    }
}


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

            // ������ݿ�������
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
            // ���� Identity
            builder.Services
                .AddIdentity<User, IdentityRole>(options =>
                {
                    options.Password.RequireNonAlphanumeric = false;  // ����Ҫ����ĸ�����ַ�
                    options.Password.RequireUppercase = false;        // ��Ҫ��д��ĸ
                    options.Password.RequiredLength = 6;            // ��С����
                    options.Password.RequireDigit = true;           // ��Ҫ����


                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // �����֤����Ȩ
            // JWT ��֤
            builder.Services.AddAuthentication(options =>
            {
                // ����Ĭ����֤����Ϊ JWT
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // �Ƿ���֤ Issuer���䷢�ߣ�
                    ValidateIssuer = true,
                    // �Ϸ��� Issuer������ appsettings.json �е�����һ�£�
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],

                    // �Ƿ���֤ Audience�������ߣ�
                    ValidateAudience = true,
                    // �Ϸ��� Audience
                    ValidAudience = builder.Configuration["Jwt:Audience"],

                    // �Ƿ���֤��Կ
                    ValidateIssuerSigningKey = true,
                    // ��Կ���������ж�ȡ��ת��Ϊ�ֽ����飩
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
                    ),

                    // �Ƿ���֤ Token ��Ч��
                    ValidateLifetime = true
                };
            });
            builder.Services.AddAuthorization();

            // ��� CORS ����
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueApp", builder =>
                {
                    builder.WithOrigins("http://localhost:9528") // ǰ�˵�ַ
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials(); // ��ǰ����Ҫ����ƾ֤���� cookies��
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


            // ���� CORS��ע��˳���� Routing ֮��Authorization ֮ǰ��
            app.UseCors("AllowVueApp");

            // ���밴˳�����ã�
            app.UseAuthentication(); // ����֤
            app.UseAuthorization();  // ����Ȩ


            app.MapControllers();

            app.Run();
        }
    }
}

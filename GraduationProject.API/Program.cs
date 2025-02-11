using GraduationProject.BL;
using GraduationProject.BL.Managers.Places;
using GraduationProject.DAL;
using GraduationProject.Data.Context;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using System;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace GraduationProject.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            //Add OData Service
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);
            builder.Services.AddControllers()
                            .AddOData(options => options
                            .AddRouteComponents("odata", GetEdmModel())
                            .Select()
                            .Filter()
                            .OrderBy()
                            .SetMaxTop(20)
                            .Count()
                            .Expand()
    );
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GraduationProject.API v1"));
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Add DbContext
            using (new CultureScope(culture))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("con1")));
            }

            services.AddScoped<IPlacesRepo, PlacesRepo>();
            services.AddScoped<IPlacesManager, PlacesManager>();


            // Add Identity
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Add Authentication with JWT
            services.AddAuthentication(options =>
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
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    ValidAudience = configuration["JWT:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
                };
            });

            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GraduationProject.API", Version = "v1" });
            });

            // Add Email Sender
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddControllers();
        }

        private class CultureScope : IDisposable
        {
            private readonly CultureInfo originalCulture;

            public CultureScope(CultureInfo culture)
            {
                originalCulture = CultureInfo.CurrentCulture;
                CultureInfo.CurrentCulture = culture;
            }

            public void Dispose()
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }
        //OData Entity Data Model Configuration
        static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new();
            builder.EntitySet<Place>("Places");
            return builder.GetEdmModel();
        }
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var client = new SmtpClient())
            {
                client.Host = _emailSettings.SmtpServer;
                client.Port = _emailSettings.Port;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password);
                client.EnableSsl = true;

                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_emailSettings.UserName);
                mailMessage.To.Add(email);
                mailMessage.Subject = subject;
                mailMessage.Body = htmlMessage;
                mailMessage.IsBodyHtml = true;

                return client.SendMailAsync(mailMessage);
            }
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
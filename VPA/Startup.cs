using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VPA.Models;
using Microsoft.EntityFrameworkCore;
using VPA.Filters;
using Microsoft.IdentityModel.Tokens;
using System.Text;
//using NLog.Extensions.Logging;
//using NLog.Web;
//using NLog;

namespace VPA
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)          
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("secrets/appsettings.secrets.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                builder => builder.AllowAnyOrigin()
                             .AllowAnyMethod()
                             .AllowAnyHeader()
                             .AllowCredentials());
            });
            
            //Custom Policy
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("Over21",
            //                      policy => policy.Requirements
            //                      .Add(new Validator(21)));
            //});

            // Add framework services.
            services.AddMvc();

            //in memory Caching

            services.AddSession();

            //services.AddSession();


            //Add Mongo Configuration
            services.Configure<Settings>(options =>
            {
                options.ConnectionString = Configuration.GetSection("MongoConnection:ConnectionString").Value;
                options.Database = Configuration.GetSection("MongoConnection:Database").Value;
            });
            services.AddTransient<IVPAEnquiryRequetRepository, VPAEnquiryRequestRepository>();

            //Add Postgre Configuration
            var sqlConnectionString = Configuration.GetConnectionString("VPAEnquiryRequestPostgreSql");
            services.AddDbContext<vpaPostgreSqlContext>(options =>
                options.UseNpgsql(
                    sqlConnectionString,
                    b => b.MigrationsAssembly("VPA")
                )
            );
            services.AddScoped<IVPAEnquiryRequestPostgreRepo, VPAEnquiryRequestPostgreSql>();
            services.AddScoped<VPAEnquiryResponse>();
            services.AddScoped<VPATranslateResponse>();
            services.AddScoped<VPAInformation>();
            services.AddScoped<PersonalInformation>();
            services.AddScoped<AccountInformation>();
            services.AddScoped<MerchantInformation>();
            services.AddScoped<List<AssociatedVpas>>();
            services.AddScoped<AssociatedVpas>();
            services.AddScoped<VPAEnquiryRequest>();

            
            //Add Redis Configuration

            //services.AddDistributedRedisCache(option =>
            //{
            //    option.Configuration = Configuration.GetSection("AppSettings:redisIP").Value;
            //    option.InstanceName = Configuration.GetSection("AppSettings:redisInstanceName").Value;
            //});

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetSection("AppSettings:redisIP").Value;
                option.InstanceName = Configuration.GetSection("AppSettings:redisInstanceName").Value;
            });

            //AppSettings
            services.Configure<AppSettings>(options =>
            {
                options.translateUri = Configuration.GetSection("AppSettings:translateUri").Value;
                options.accountEnquiryUri = Configuration.GetSection("AppSettings:accountEnquiryUri").Value;
                options.isTest = Convert.ToBoolean(Configuration.GetSection("AppSettings:isTest").Value);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //env.EnvironmentName = EnvironmentName.Development;
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/error");
            //}

            //app.UseStatusCodePages();
            //app.UseCors("CorsPolicy");
            
            app.UseSession();
            app.UseCors("CorsPolicy");
            //app.UseSession();
            //app.UseJwtBearerAuthentication(new JwtBearerOptions
            //{
            //    AutomaticAuthenticate = true,
            //    AutomaticChallenge = true,
            //    TokenValidationParameters = new TokenValidationParameters
            //    {
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("AppSettings:JwtKey").Value)),
            //        ValidAudience = Configuration.GetSection("AppSettings:SiteUrl").Value,
            //        ValidateIssuerSigningKey = true,
            //        ValidateLifetime = true,
            //        ValidIssuer = Configuration.GetSection("AppSettings:SiteUrl").Value
            //    }
            //});
            app.UseMvc();
            //app.Run(async (context) =>
            //{
            //    var message = $"Host: {Environment.MachineName}\n" +
            //        $"EnvironmentName: {env.EnvironmentName}\n" +
            //        $"Secret value: {Configuration["Database:ConnectionString"]}";
            //    await context.Response.WriteAsync(message);
            //});
        }
    }
}

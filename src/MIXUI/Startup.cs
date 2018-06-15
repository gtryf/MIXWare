using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MIXUI.Assembler;
using MIXUI.Entities;
using MIXUI.Helpers;
using MIXUI.TaskQueues;
using Swashbuckle.AspNetCore.Swagger;

namespace MIXUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("MIXWareDb")));
            services.AddMvc();
            services.AddAutoMapper();

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection(nameof(AppSettings));
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SameUserPolicy", policy =>
                    policy.Requirements.Add(new SameUserRequirement()));
            });

            // add identity
            var builder = services.AddIdentityCore<AppUser>(o =>
            {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            });
            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);
            builder.AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

            // configure DI
            services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, WorkspaceAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, FileAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, SubmissionAuthorizationHandler>();

            services.AddScoped<PlainTextPrettyPrinter>();
            services.AddScoped<TeXPrettyPrinter>();
            services.AddScoped<Func<string, IPrettyPrinter>>(serviceProvider => serviceType =>
            {
                switch (serviceType)
                {
                    case "plain":
                        return serviceProvider.GetService<PlainTextPrettyPrinter>();
                    case "tex":
                        return serviceProvider.GetService<TeXPrettyPrinter>();
                    default:
                        throw new KeyNotFoundException();
                }
            });

            services.AddScoped<BinaryAssembler>();
            services.AddScoped<CardAssembler>();
            services.AddScoped<Func<string, IAssembler>>(serviceProvider => serviceType =>
            {
                switch (serviceType)
                {
                    case "binary":
                        return serviceProvider.GetService<BinaryAssembler>();
                    case "card":
                        return serviceProvider.GetService<CardAssembler>();
                    default:
                        throw new KeyNotFoundException();
                }
            });

            services.AddSingleton<IHostedService, QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "MIXWare API",
                    Description = "Web API for the MIX Simulator",
                    Contact = new Contact
                    {
                        Name = "George Tryfonas",
                        Email = "george.tryfonas@gmail.com"
                    },
                    License = new License
                    {
                        Name = "MIT",
                        Url = "https://opensource.org/licenses/MIT"
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MIXWare API V1");
            });

            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}

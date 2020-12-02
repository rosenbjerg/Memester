using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFMpegCore;
using Hangfire;
using Hangfire.PostgreSql;
using HangfireBasicAuthenticationFilter;
using Memester.Application.Model;
using Memester.Core;
using Memester.Core.Options;
using Memester.Database;
using Memester.FileStorage;
using Memester.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SessionOptions = Memester.Core.Options.SessionOptions;

namespace Memester
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            AddOptions<EmailOptions>(services);
            AddOptions<SessionOptions>(services);
            AddOptions<FileStorageOptions>(services);
            
            FFMpegOptions.Configure(new FFMpegOptions
            {
                RootDirectory = ""
            });
            
            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Memester API", Version = "v1" }));
            
            HangfireContext.EnsureCreated(_configuration.GetConnectionString("HangfirePgsql"));
            ConfigureHangfire(GlobalConfiguration.Configuration);
            services.AddHangfire(ConfigureHangfire);
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Math.Min(Environment.ProcessorCount * 2 - 1, 16);
                options.Queues = JobQueues.All;
            });
            
            services.AddMvc().AddJsonOptions(options => ConfigureJsonSerializer(options.JsonSerializerOptions));
            
            services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(_configuration.GetConnectionString("Pgsql"), b => b.MigrationsAssembly(typeof(DatabaseContext).Assembly.FullName)));
            
            services.AddScoped(typeof(OperationContext), _ => new OperationContext());
            services.AddScoped(typeof(IHttpSessionService), typeof(CookieSessionService));
            services.AddScoped(typeof(ScrapingService));
            services.AddScoped(typeof(IndexingService));
            services.AddScoped(typeof(FileStorageService));
            services.AddScoped(typeof(AuthenticationService));
            
            services.AddSingleton(typeof(IEmailService), typeof(MailkitEmailService));
            services.AddSingleton(typeof(SessionService));
            services.AddSingleton(typeof(Random));

            services.AddTransient<IAsyncInitialized, FileStorageService>();
            // services.AddTransient<IAsyncInitialized, DatabaseContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseContext databaseContext, ILogger logger)
        {
            var initializers = app.ApplicationServices.GetServices<IAsyncInitialized>();
            foreach (var initializer in initializers)
            {
                initializer.Initialize().GetAwaiter().GetResult();
                logger.LogInformation("Initialization of {ServiceTypeName} completed", initializer.GetType().Name);
            }

            databaseContext.Database.EnsureCreated();
            logger.LogInformation("Database created");
            
            app.UseHangfireDashboard("/dashboard", new DashboardOptions
            {
                Authorization = new [] { new HangfireCustomBasicAuthenticationFilter{ User = "memester", Pass = "memesterdev" } }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Memester API"));
            }
            else
            {
                RecurringJob.AddOrUpdate<ScrapingService>(service => service.IndexBoard("wsg"), "*/30 * * * *");
            }
            
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.Use(FallbackMiddlewareHandler);
        }
        
        protected void ConfigureHangfire(IGlobalConfiguration hangfireConfiguration)
        {
            hangfireConfiguration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(_configuration.GetConnectionString("HangfirePgsql"));
        }
        
        protected void ConfigureJsonSerializer(JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
            options.Converters.Add(new JsonStringEnumConverter());
        }
        
        protected void AddOptions<TOptions>(IServiceCollection services)
            where TOptions : class, new()
        {
            services.AddOptions<TOptions>()
                .Bind(_configuration.GetSection(typeof(TOptions).Name))
                .ValidateDataAnnotations();
            services.AddSingleton(typeof(TOptions), provider => provider.GetRequiredService<IOptions<TOptions>>().Value);
        }
        
        private static readonly Regex HashRegex = new Regex("\\.[0-9a-f]{5}\\.", RegexOptions.Compiled); 
        private static readonly FileExtensionContentTypeProvider FileExtensionContentTypeProvider = new FileExtensionContentTypeProvider(); 
        private static async Task FallbackMiddlewareHandler(HttpContext context, Func<Task> next)
        {
            var path = context.Request.Path.ToString().TrimStart('/');
            var file = Path.Combine("public", path);
            var fileInfo = File.Exists(file)
                ? new FileInfo(file)
                : new FileInfo(Path.Combine("public", "index.html"));
            if (!context.Response.HasStarted)
            {
                if (HashRegex.IsMatch(path))
                    context.Response.Headers.Add("Cache-Control", "max-age=2592000");
                
                if(!FileExtensionContentTypeProvider.TryGetContentType(fileInfo.Name, out var contentType))
                    contentType = "application/octet-stream";
                context.Response.ContentType = contentType;
                context.Response.StatusCode = 200;
            }
            await context.Response.SendFileAsync(new PhysicalFileInfo(fileInfo));
            await context.Response.CompleteAsync();
        }
    }
}
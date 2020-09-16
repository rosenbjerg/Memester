using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FFMpegCore;
using Hangfire;
using Hangfire.PostgreSql;
using Memester.Application.Model;
using Memester.Database;
using Memester.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

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
            
            services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(_configuration.GetConnectionString("Pgsql")));
            
            services.AddScoped(typeof(ScrapingService));
            services.AddScoped(typeof(IndexingService));
            
            services.AddSingleton(typeof(Random));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseContext databaseContext)
        {
            //databaseContext.Database.EnsureDeleted();
            databaseContext.Database.EnsureCreated();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHangfireDashboard("/dashboard");
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Memester API"));
            }
            
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
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
            services.AddSingleton(typeof(TOptions), provider => provider.GetService<IOptions<TOptions>>().Value);
        }
    }
}
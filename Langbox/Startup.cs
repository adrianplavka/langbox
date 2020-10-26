using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Westwind.AspNetCore.LiveReload;

using Langbox.Data;
using Langbox.Models;
using Langbox.Services;

namespace Langbox
{
    public class Startup
    {
        private readonly IWebHostEnvironment Env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAntDesign();

            if (Env.IsDevelopment())
                services.AddLiveReload(config => {
                    config.LiveReloadEnabled = true;
                    config.ClientFileExtensions = ".css,.js,.htm,.html";
                    config.FolderToMonitor = "~/../";
                });

            if (Env.IsDevelopment())
                services.AddRazorPages().AddRazorRuntimeCompilation();
            else
                services.AddRazorPages();
            
            services.AddServerSideBlazor();

            // Configure injectable classes.
            services.AddTransient<MonacoEditor>();
            services.AddScoped<SandboxEnvironmentService>();
            services.AddScoped<ChallengeService>();

            services.AddDbContext<LangboxDbContext>(options =>
                options.UseNpgsql(CreateDatabaseConnectionString()), ServiceLifetime.Transient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Env.IsDevelopment())
            {
                app.UseLiveReload();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. 
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            // Initializes application's database in order to re-create database tables every time
            // the application started.
            using var scope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            using var context = scope.ServiceProvider.GetService<LangboxDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            SeedDatabase(context);
        }

        private static string CreateDatabaseConnectionString()
        {
            var isUri = Uri.TryCreate(
                    Environment.GetEnvironmentVariable("DATABASE_URL"),
                    UriKind.Absolute,
                    out var uri);
            if (!isUri || uri is null)
                throw new UriFormatException("Cannot deserialize DATABASE_URL environment variable.");

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Username = uri.UserInfo.Contains(":", StringComparison.Ordinal)
                    ? uri.UserInfo.Split(":")[0]
                    : uri.UserInfo,
                Password = uri.UserInfo.Contains(":", StringComparison.Ordinal)
                    ? uri.UserInfo.Split(":")[1]
                    : null,
                Database = uri.LocalPath.TrimStart('/'),
                Pooling = true,
            };

            return builder.ToString();
        }

        private static void SeedDatabase(LangboxDbContext context)
        {
            // Seed the database with sandbox environments.
            context.Add(new SandboxEnvironment 
            { 
                TemplateName = "C# (MSUnit)",
                Language = "csharp",
                MainFileName = "Main.cs", 
                TestFileName = "Test.cs",
                MainBoilerplate = "namespace Challenge\n{\n\tpublic class ChallengeClass\n\t{\n\t\t\n\t}\n}",
                TestBoilerplate = "using Microsoft.VisualStudio.TestTools.UnitTesting;\n\nusing Challenge;\n\nnamespace ChallengeTests\n{\n\t[TestClass]\n\tpublic class ChallengeTest\n\t{\n\t\t[TestMethod]\n\t\tpublic void Test()\n\t\t{\n\t\t\t\n\t\t}\n\t}\n}",
                DockerImage = "lgbox/csharp-msunit-template:0.1.0",
                DockerCommand = "python main.py",
                DockerMainPath = "/app/Lgbox-CSharp-MSUnit/Main.cs",
                DockerTestPath = "/app/Lgbox-CSharp-MSUnit-Tests/Test.cs"
            });

            // Seed the database with challenges.
            context.Add(new Challenge
            {
                Title = "One & Two",
                Instructions =
@"Your task is to fulfill class method implementations, according to the return types.

Example:

- Method `One` should return `1`.

- Method `Two` should return `""2""`.",
                MainContent =
@"namespace Example
{
    public class ExampleClass
    {
        public static int One()
        {
            return 0;
        }

        public static string Two()
        {
            return """";
        }
    }
}",
                TestContent =
@"using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExampleTests
{
    [TestClass]
    public class ExampleClassTest
    {
        [TestMethod]
        public void One_Implementation_Is_Correct()
        {
            Assert.AreEqual(Example.ExampleClass.One(), 1);
        }

        [TestMethod]
        public void Two_Implementation_Is_Correct()
        {
            Assert.AreEqual(Example.ExampleClass.Two(), ""2"");   
        }
    }
}",
                SandboxEnvironmentId = "C# (MSUnit)"
            });

            context.SaveChanges();
        }
    }
}

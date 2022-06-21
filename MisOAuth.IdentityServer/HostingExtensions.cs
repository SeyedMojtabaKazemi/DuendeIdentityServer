using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MisOAuth.IdentityServer.Data;
using MisOAuth.IdentityServer.Models;
using Serilog;
using System.Security.Claims;

namespace MisOAuth.IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var migrationsAssembly = typeof(Program).Assembly.GetName().Name;


        builder.Services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("AspNetIdentityStoreCnn")));

        builder.Services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddRazorPages();

        #region Add default identity with test users
        //builder.Services.AddIdentityServer(options =>
        //{
        //    // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
        //    options.EmitStaticAudienceClaim = true;
        //})
        // .AddTestUsers(TestUsers.Users)
        //.AddInMemoryApiResources(Config.ApiResources)
        //.AddInMemoryIdentityResources(Config.IdentityResources)
        //.AddInMemoryApiScopes(Config.ApiScopes)
        //.AddInMemoryClients(Config.Clients);
        #endregion

        builder.Services.AddIdentityServer(options =>
        {
            options.EmitStaticAudienceClaim = true;
        })
        .AddConfigurationStore(options =>
        {
            options.ConfigureDbContext = b => b.UseSqlServer(builder.Configuration.GetConnectionString("ConfigurationStoreCnn"),
                sql => sql.MigrationsAssembly(migrationsAssembly));
        })
        .AddOperationalStore(options =>
        {
            options.ConfigureDbContext = b => b.UseSqlServer(builder.Configuration.GetConnectionString("OperationalStoreCnn"),
                sql => sql.MigrationsAssembly(migrationsAssembly));
        })
        .AddAspNetIdentity<User>();

        //builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
        //                                                    .AllowAnyMethod()
        //                                                     .AllowAnyHeader()));

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        InitializeDatabaseIdentityServer(app);
        InitializeDatabaseAspNetIdentity(app);

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }

    private static void InitializeDatabaseIdentityServer(IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        {
            serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

            context.Database.Migrate();

            if (!context.Clients.Any())
            {
                foreach (var client in Config.Clients)
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in Config.ApiScopes)
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
        }
    }

    private static async void InitializeDatabaseAspNetIdentity(WebApplication app)
    {
        using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

            var userMgr = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();

            if (!context.Users.Any())
            {
                var userDataForSeeds = app.Configuration.GetSection(nameof(ApplicationUserSeedData)).Get<List<ApplicationUserSeedData>>();

                foreach (var item in userDataForSeeds)
                {
                    var adduser = await userMgr.FindByNameAsync(item.ApplicationUser.UserName);
                    if (adduser is null)
                    {
                        var result = await userMgr.CreateAsync(item.ApplicationUser, "123");
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }
                        if (item.Claims != null)
                        {
                            result = await userMgr.AddClaimsAsync(item.ApplicationUser, item.Claims.Select(c => new Claim(c.Type, c.Value)));
                            if (!result.Succeeded)
                            {
                                throw new Exception(result.Errors.First().Description);
                            }
                        }

                        Log.Debug("{UserName} created", item.ApplicationUser.UserName);
                    }
                    else
                    {
                        Log.Debug("{UserName} already exists", item.ApplicationUser.UserName);
                    }
                }
            }
        }
    }
}

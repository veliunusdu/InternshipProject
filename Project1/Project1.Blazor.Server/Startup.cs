using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Project1.Blazor.Server.Services;
using Project1.Module.Services.Implementations;
using Project1.Core.Services.Interfaces;
using Project1.Module.Handlers;
using MediatR;

namespace Project1.Blazor.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            EmailSettings emailSettings = Configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
            services.AddSingleton(emailSettings);
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<ICrmNotificationService, CrmNotificationService>();

            services.AddSingleton<ISystemStatusService, SystemStatusService>();
            services.AddScoped<INonSecuredObjectSpaceFactory, CustomNonSecuredObjectSpaceFactory>();
            services.AddScoped<INoteService, NoteService>();
            
            // MediatR Configuration
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateNoteCommandHandler).Assembly));
            
            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpContextAccessor();
            services.AddScoped<CircuitHandler, CircuitHandlerProxy>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/LoginPage";
            });

            services.AddXaf(Configuration, builder =>
            {
                builder.UseApplication<Project1BlazorApplication>();
                builder.Modules
                    .AddConditionalAppearance()
                    .AddValidation(options =>
                    {
                        options.AllowValidationDetailsAccess = false;
                    })
                    .Add<Project1.Module.Project1Module>()
                    .Add<Project1BlazorModule>();

                builder.Security
                    .UseIntegratedMode(options =>
                    {
                        options.RoleType = typeof(PermissionPolicyRole);
                        options.UserType = typeof(PermissionPolicyUser);
                    })
                    .AddPasswordAuthentication();

                builder.ObjectSpaceProviders
                    .AddSecuredXpo((serviceProvider, options) =>
                    {
                        string connectionString = null;
                        if (Configuration.GetConnectionString("ConnectionString") != null)
                        {
                            connectionString = Configuration.GetConnectionString("ConnectionString");
                        }
#if EASYTEST
                        if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                            connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                        }
#endif
                        ArgumentNullException.ThrowIfNull(connectionString);
                        options.ConnectionString = connectionString;
                        options.ThreadSafe = true;
                        options.UseSharedDataStoreProvider = true;
                    })
                    .AddNonPersistent();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseXaf();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapXafEndpoints();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });
        }
    }

    public class CustomNonSecuredObjectSpaceFactory : INonSecuredObjectSpaceFactory
    {
        private readonly IObjectSpaceProviderService _providerService;

        public CustomNonSecuredObjectSpaceFactory(IObjectSpaceProviderService providerService)
        {
            _providerService = providerService;
        }

        public IObjectSpace CreateNonSecuredObjectSpace(Type objectType)
        {
            var provider = _providerService.GetObjectSpaceProvider(objectType);
            if (provider == null)
            {
                throw new InvalidOperationException($"No ObjectSpaceProvider found for type {objectType}.");
            }
            return provider.CreateUpdatingObjectSpace(true);
        }
    }
}

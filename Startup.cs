using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using MyCompany.Service;
using MyCompany.Domain.Repositories.Abstract;
using MyCompany.Domain.Repositories.EntityFramework;
using MyCompany.Domain;

namespace MyCompany
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            // Подключаем конфиг из appsettings.json
            Configuration.Bind("Project", new Config());

            /*services.AddIdentity<IdentityUser,IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();*/

            // Подключаем нужный функционал приложения в качестве сервисов
            services.AddTransient<ITextFieldsRepository, EFTextFieldsRepository>();
            services.AddTransient<IServiceItemsRepository, EFServiceItemsRepository>();
            services.AddTransient<DataManager>();

            // Подключаем контекст БД (База Данных)
            services.AddDbContext<AppDbContext>(x => x.UseSqlite(Config.ConnectionString));

            // Настраиваем identity системуty систему
            services.AddIdentity<IdentityUser, IdentityRole>(opts =>
                    {
                        opts.User.RequireUniqueEmail = true;
                        opts.Password.RequiredLength = 6;
                        opts.Password.RequireNonAlphanumeric = false;
                        opts.Password.RequireLowercase = false;
                        opts.Password.RequireUppercase = false;
                        opts.Password.RequireDigit = false;
                    }).AddEntityFrameworkStores<AppDbContext>()
                        .AddDefaultTokenProviders();

            // Настраиваем authentication cookie
            services.ConfigureApplicationCookie(options =>
                    {
                        options.Cookie.Name = "myCompanyAuth";
                        options.Cookie.HttpOnly = true;
                        options.LoginPath = "/account/login";
                        options.AccessDeniedPath = "/account/accessdenied";
                        options.SlidingExpiration = true;

                    });

            services.AddAuthorization(x =>
                    {
                        x.AddPolicy("AdminArea", policy => { policy.RequireRole("admin"); });
                    });

            // Добавляем поддержку контроллеров и представлений (MVC)
            services.AddControllersWithViews(x =>
                    {
                        x.Conventions.Add(new AdminAreaAuthorization("Admin", "AdminArea"));
                    })

                // Выставляем совместимость с Asp.NET Core 3.0 с Asp.NET Core 3.0 3.0 с Asp.NET Core 3.00 3.0 с Asp.NET Core 3.000 3.0 с Asp.NET Core 3.0
                /*.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)*/
                .AddSessionStateTempDataProvider();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute("admin", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                });
        }
    }
}

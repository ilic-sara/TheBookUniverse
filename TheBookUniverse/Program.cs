using Data;
using Repositories;
using Services;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using Microsoft.Extensions.FileProviders;
using MongoDB.Driver;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

//adding AutoMapper
builder.Services.AddAutoMapper(typeof(MapperService));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews();

//Dependency injection for Repositories and Services
builder.Services.AddTransient<IBookService, BookService>();
builder.Services.AddTransient<IBookRepository, BookRepository>();
builder.Services.AddTransient<IAuthorService, AuthorService>();
builder.Services.AddTransient<IAuthorRepository, AuthorRepository>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IBannerImageRepository, BannerImageRepository>();
builder.Services.AddTransient<IBannerImageService, BannerImageService>();
builder.Services.AddTransient<IBookCommentRepository, BookCommentRepository>();
builder.Services.AddTransient<IBookCommentService, BookCommentService>();
builder.Services.AddTransient<IFilterOptionsRepository, FilterOptionsRepository>();
builder.Services.AddTransient<IFilterOptionsService, FilterOptionsService>();
builder.Services.AddSingleton<IMongoClient, MongoClient>();

#region Identity services

builder.Services.AddIdentity<User, Role>()
    .AddRoleManager<RoleManager<Role>>()
    .AddMongoDbStores<User, Role, ObjectId>(
    builder.Configuration.GetSection("MongoSettings:Connection").Value,
    builder.Configuration.GetSection("MongoSettings:DatabaseName").Value
).AddSignInManager().AddDefaultTokenProviders();

//Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.";

    // Required unique email
    options.User.RequireUniqueEmail = true;

    // Email confirmation settings
    options.SignIn.RequireConfirmedEmail = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie Settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(354);

    options.LoginPath = "/identity/home/login";
    options.LogoutPath = "/identity/home/logout";
    options.AccessDeniedPath = "/identity/home/accessdenied";
    options.SlidingExpiration = true;
});

#endregion

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));


#region Authorize services

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
});

#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


var defaultCulture = new CultureInfo("en-US");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

app.UseRequestLocalization(localizationOptions);

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Common}/{controller=Home}/{action=ViewStartPage}/{id?}");

app.Run();

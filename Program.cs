using Microsoft.AspNetCore.Authentication.Cookies;
using WEBLINK_CRM.Models;
using WEBLINK_CRM.Repositories;
using WEBLINK_CRM.repository;
using WLSPL_ERP_CRM.Models;

var builder = WebApplication.CreateBuilder(args);


// ======================================================
// GOV KEY SETTINGS
// ======================================================

builder.Services.AddScoped(resolver =>
    builder.Configuration
        .GetSection("GovKey")
        .Get<GovKeySettings>());


// ======================================================
// MVC
// ======================================================

builder.Services.AddControllersWithViews();


// ======================================================
// REPOSITORIES
// ======================================================

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IinquiryRepo, InquiryRepo>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IcomapnymasterRepo, CompanymasterRepo>();
builder.Services.AddScoped<ICallandMeetingRepo, CallandMeetingRepo>();
builder.Services.AddScoped<IGoveServices, GovServicesRepo>();
builder.Services.AddScoped<IMailingRepo, MailingRepo>();
builder.Services.AddScoped<IWorkOrder, RepoWorkOrder>();
builder.Services.AddScoped<IServicesRepo, ServicesRepo>();


// ======================================================
// SESSION
// ======================================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    options.Cookie.Name = ".WEBLINK_CRM.Session";

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;
});


// ======================================================
// AUTHENTICATION
// ======================================================

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme
)
.AddCookie(options =>
{
    options.Cookie.Name = ".WEBLINK_CRM.Auth";

    options.LoginPath = "/Login/Index";

    options.AccessDeniedPath = "/Login/AccessDenied";

    options.ExpireTimeSpan =
        TimeSpan.FromMinutes(30);

    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;
});


builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<NoCacheFilter>();
});


// ======================================================
// AUTHORIZATION
// ======================================================

builder.Services.AddAuthorization();


var app = builder.Build();


// ======================================================
// ERROR HANDLING
// ======================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


// ======================================================
// MIDDLEWARE
// ======================================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();


// ======================================================
// ROUTING
// ======================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);


app.Run();
using WEBLINK_CRM.Models;
using WEBLINK_CRM.Repositories;
using WEBLINK_CRM.repository;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(resolver =>
    builder.Configuration.GetSection("GovKey").Get<GovKeySettings>());
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IcomapnymasterRepo, CompanymasterRepo>();
builder.Services.AddScoped<ICallandMeetingRepo, CallandMeetingRepo>();
builder.Services.AddScoped<IGoveServices, GovServicesRepo>();
builder.Services.AddScoped<IMailingRepo, MailingRepo>().AddScoped<IWorkOrder, RepoWorkOrder>();

// Temporary Session configuration
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
        //pattern: "{controller=Login}/{action=Index}/{id?}");
        pattern: "{controller=Login}/{action=Index}/{id?}");


app.Run();
using Admin.Components;
using Admin.Data.Events;
using Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddApplicationLayers();
builder.Services.AddScoped<EventViewService>();

if (builder.Environment.IsDevelopment())
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(LocalNetCoreAuth.IndividualAccounts.Components.Account.Pages.Login).Assembly);

app.MapAdditionalIdentityEndpoints();

app.UseAntiforgery();

app.Run();

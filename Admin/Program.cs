using Admin.Components;
using Admin.Data.Events;
using Admin.Data.Helpers;
using Core.Dto;
using Core.Extensions;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddApplicationLayers();
builder.Services.AddScoped<StateContainerSingle<ServiceTypeDto>>();
builder.Services.AddScoped<StateContainerSingle<DateTime>>();
builder.Services.AddTransient<AppointmentViewService>();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

if (builder.Environment.IsDevelopment())
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorComponents(options =>
    options.DetailedErrors = builder.Environment.IsDevelopment());

// TODO remove this if I split out the booking form to a different project.
builder.Services.AddAntiforgery(x => x.SuppressXFrameOptionsHeader = true);

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
    .AddApplicationAssemblies();

app.AddApplicationLayers();

app.UseAntiforgery();

app.Run();

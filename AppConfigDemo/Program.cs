using AppConfigDemo.Data;
using Azure.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseWebRoot("wwwroot").UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
//DEMO PART 3
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement()
    //DEMO PART 6
    .AddFeatureFilter<TimeWindowFilter>()
;

string? cs = Environment.GetEnvironmentVariable("AppConfigDemoConnectionString");
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    var settings = config.Build();

    config.AddAzureAppConfiguration(options =>
    {
        options.Connect(cs)
                    .Select(KeyFilter.Any, LabelFilter.Null)
                    .Select(KeyFilter.Any, hostingContext.HostingEnvironment.EnvironmentName)
                    .Select("WebApp:", LabelFilter.Null)
                //DEMO PART 2
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential());
                })
                //DEMO PART 3
                //.ConfigureRefresh(refresh =>
                //{
                //    refresh.Register("Sentinel", refreshAll: true).SetCacheExpiration(new TimeSpan(0, 1, 0));
                //})
                //DEMO PART 4
                .ConfigureRefresh(refresh =>
                {
                    refresh.Register("Sentinel", refreshAll: true);
                })
                //DEMO PART 5
                .UseFeatureFlags()
                ;       
    }); 
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

//DEMO PART 3
app.UseAzureAppConfiguration();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

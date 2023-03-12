using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = new ConfigurationBuilder();
builder.AddAzureAppConfiguration(Environment.GetEnvironmentVariable("AppConfigDemoConnectionString"));

var config = builder.Build();
Console.WriteLine(config["ConsoleApp:SampleValue"] ?? "Hello world!");
Console.ReadLine();
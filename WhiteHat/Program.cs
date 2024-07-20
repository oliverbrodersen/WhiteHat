using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WhiteHat;
using WhiteHat.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddScoped<HNFetcher>();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<IframeCheckerService>();

await builder.Build().RunAsync();

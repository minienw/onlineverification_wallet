using CheckInValidation.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using CheckInValidation.Client.Core;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace CheckInValidation.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //builder.Services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            builder.Logging.SetMinimumLevel(LogLevel.Debug); //TODO set from env

            builder.Services.AddHttpClient();
            builder.Services.AddTransient<HttpGetIdentityCommand>();
            builder.Services.AddTransient<HttpPostTokenCommand>();
            builder.Services.AddTransient<HttpPostValidateCommand>();
            builder.Services.AddTransient<HttpPostCallbackCommand>();
            builder.Services.AddTransient<VerificationWorkflow>();
            builder.Services.AddMudServices();

            var app = builder.Build();

            await app.RunAsync();
        }
    }
}
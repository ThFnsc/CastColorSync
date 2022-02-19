using ThFnsc.SpotifyColorSync;
using ThFnsc.SpotifyColorSync.Configuration;
using ThFnsc.SpotifyColorSync.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
        services.Configure<AppSettings>(context.Configuration.GetSection(nameof(AppSettings)));
        services.AddHass();
    })
    .Build();

await host.RunAsync();

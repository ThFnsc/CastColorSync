using ThFnsc.CastColorSync;
using ThFnsc.CastColorSync.Configuration;
using ThFnsc.CastColorSync.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<IColorPicker, ColorPicker>();
        services.Configure<AppSettings>(context.Configuration.GetSection(nameof(AppSettings)));
        services.AddHass();
    })
    .Build();

await host.RunAsync();

using ThFnsc.CastColorSync;
using ThFnsc.CastColorSync.Configuration;
using ThFnsc.CastColorSync.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<IColorPicker, ColorPicker>();
        services.AddOptions<AppSettings>()
            .BindConfiguration(nameof(AppSettings))
            .Validate(opt => opt.Validate());
        services.AddHass();
        services.AddWLed();
    })
    .Build();

await host.RunAsync();

using Microsoft.Extensions.Logging;
using RavenMobile.Features.WifiQr;
using RavenMobile.ViewModels;
using RavenMobile.Views;
using RavenMobile.Services;
using ZXing.Net.Maui.Controls;
#if ANDROID
using RavenMobile.Platforms.Android.WifiQr;
#endif

namespace RavenMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
#if ANDROID
builder.Services.AddSingleton<IWifiJoinService, AndroidWifiJoinService>();
#else
        builder.Services.AddSingleton<IWifiJoinService, FakeWifiJoinService>();
#endif

        builder.Services.AddSingleton<IWifiQrSenderService, WifiQrSenderService>();
#if ANDROID
        builder.Services.AddSingleton<IHotspotService, AndroidHotspotService>();

#else
        builder.Services.AddSingleton<IHotspotService, FakeHotspotService>();
#endif

        builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<IWifiQrTransferService, WifiQrTransferService>();
        builder.Services.AddSingleton<MenuPage>();
        builder.Services.AddSingleton<AppFlyoutPage>();
        builder.Services.AddSingleton<ITransferHistoryService, TransferHistoryService>();
        builder.Services.AddSingleton<HistoryViewModel>();
        builder.Services.AddSingleton<HistoryPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<AboutPage>();
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<OnboardingPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
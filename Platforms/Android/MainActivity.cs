using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace RavenMobile;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges =
        ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private const int PermissionRequestCode = 1001;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        ApplySystemBars();
        RequestPermissionsIfNeeded();
    }

    protected override void OnResume()
    {
        base.OnResume();

        ApplySystemBars();
    }

    private void ApplySystemBars()
    {
        try
        {
            if (Window == null)
                return;

            // Raven arka planıyla bütünleşen koyu status/navigation bar
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#05060A"));
            Window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#05060A"));

            // İçeriği status bar'ın altından başlatır.
            // Böylece üst bildirim çubuğu uygulama ile bütünleşir ama yazıların üstüne binmez.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                Window.SetDecorFitsSystemWindows(true);

                var controller = Window.InsetsController;

                if (controller != null)
                {
                    // LightStatusBars = koyu ikon demek.
                    // Biz bunu kapatıyoruz ki ikonlar beyaz kalsın.
                    controller.SetSystemBarsAppearance(
                        0,
                        (int)WindowInsetsControllerAppearance.LightStatusBars);

                    controller.SetSystemBarsAppearance(
                        0,
                        (int)WindowInsetsControllerAppearance.LightNavigationBars);
                }
            }
            else
            {
                var currentFlags = (int)Window.DecorView.SystemUiVisibility;

                // Açık temada Android'in verdiği siyah ikon modunu kapat.
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    currentFlags &= ~(int)SystemUiFlags.LightStatusBar;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    currentFlags &= ~(int)SystemUiFlags.LightNavigationBar;
                }

                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)currentFlags;
            }
        }
        catch
        {
        }
    }

    private void RequestPermissionsIfNeeded()
    {
        var permissions = new List<string>();

        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != Permission.Granted)
        {
            permissions.Add(Manifest.Permission.Camera);
        }

        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothScan) != Permission.Granted)
            {
                permissions.Add(Manifest.Permission.BluetoothScan);
            }

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) != Permission.Granted)
            {
                permissions.Add(Manifest.Permission.BluetoothConnect);
            }
        }

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.NearbyWifiDevices) != Permission.Granted)
            {
                permissions.Add(Manifest.Permission.NearbyWifiDevices);
            }
        }

        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
        {
            permissions.Add(Manifest.Permission.AccessFineLocation);
        }

        if (permissions.Count > 0)
        {
            ActivityCompat.RequestPermissions(
                this,
                permissions.ToArray(),
                PermissionRequestCode);
        }
    }
}
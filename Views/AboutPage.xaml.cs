namespace RavenMobile.Views;

public partial class AboutPage : ContentPage
{
    public string VersionText { get; }
    public string PlatformText { get; }

    public AboutPage()
    {
        InitializeComponent();

        VersionText = $"{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";
        PlatformText = $"{DeviceInfo.Current.Platform} • {DeviceInfo.Current.VersionString}";

        BindingContext = this;

        NavigationPage.SetHasNavigationBar(this, false);
    }

    private void MenuButton_Clicked(object? sender, EventArgs e)
    {
        OpenFlyout();
    }

    private void OpenFlyout()
    {
        var parent = Parent;

        while (parent != null)
        {
            if (parent is FlyoutPage flyoutPage)
            {
                flyoutPage.IsPresented = true;
                return;
            }

            parent = parent.Parent;
        }
    }
}
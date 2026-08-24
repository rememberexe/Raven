using System.Globalization;

namespace RavenMobile.Views;

public partial class OnboardingPage : ContentPage
{
    private const string OnboardingSeenKey = "Raven_OnboardingSeen";

    private readonly HomePage _homePage;

    private readonly AppFlyoutPage _appFlyoutPage;

    public OnboardingPage(AppFlyoutPage appFlyoutPage)
    {
        InitializeComponent();

        _appFlyoutPage = appFlyoutPage;

        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void StartButton_Clicked(object? sender, EventArgs e)
    {
        await FinishOnboardingAsync();
    }

    private async void SkipButton_Clicked(object? sender, EventArgs e)
    {
        await FinishOnboardingAsync();
    }

    private async Task FinishOnboardingAsync()
    {
        Preferences.Default.Set(OnboardingSeenKey, true);

        Application.Current!.Windows[0].Page = _appFlyoutPage;

        await Task.CompletedTask;
    }
}

public class OnboardingTitleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value?.ToString() ?? "";
        return Split(text, 0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string Split(string value, int index)
    {
        var parts = value.Split('|');
        return parts.Length > index ? parts[index] : "";
    }
}

public class OnboardingDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value?.ToString() ?? "";
        return Split(text, 1);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string Split(string value, int index)
    {
        var parts = value.Split('|');
        return parts.Length > index ? parts[index] : "";
    }
}

public class OnboardingIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value?.ToString() ?? "";
        return Split(text, 2);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string Split(string value, int index)
    {
        var parts = value.Split('|');
        return parts.Length > index ? parts[index] : "";
    }
}
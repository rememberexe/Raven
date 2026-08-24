using RavenMobile.Views;

namespace RavenMobile;

public partial class App : Application
{
    private const string OnboardingSeenKey = "Raven_OnboardingSeen";

    private readonly AppFlyoutPage _appFlyoutPage;
    private readonly OnboardingPage _onboardingPage;

    public App(
        AppFlyoutPage appFlyoutPage,
        OnboardingPage onboardingPage)
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Dark;
        _appFlyoutPage = appFlyoutPage;
        _onboardingPage = onboardingPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var hasSeenOnboarding = Preferences.Default.Get(OnboardingSeenKey, false);

        Page startPage = hasSeenOnboarding
            ? _appFlyoutPage
            : _onboardingPage;

        return new Window(startPage);
    }
}
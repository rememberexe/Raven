using RavenMobile.Helpers;

namespace RavenMobile.Views;

public class AppFlyoutPage : FlyoutPage
{
    private readonly MenuPage _menuPage;

    private readonly HomePage _homePage;
    private readonly HistoryPage _historyPage;
    private readonly SettingsPage _settingsPage;
    private readonly AboutPage _aboutPage;

    public AppFlyoutPage(
        MenuPage menuPage,
        HomePage homePage,
        HistoryPage historyPage,
        SettingsPage settingsPage,
        AboutPage aboutPage)
    {
        _menuPage = menuPage;

        _homePage = homePage;
        _historyPage = historyPage;
        _settingsPage = settingsPage;
        _aboutPage = aboutPage;

        Flyout = _menuPage;
        Detail = CreateNavigationPage(_homePage);

        FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

        _menuPage.SetActive("home");
        _menuPage.MenuSelected += OnMenuSelected;
    }

    private async void OnMenuSelected(string key)
    {
        Page targetPage = key switch
        {
            "home" => _homePage,
            "history" => _historyPage,
            "settings" => _settingsPage,
            "about" => _aboutPage,
            _ => _homePage
        };

        _menuPage.SetActive(key);

        var oldDetail = Detail;

        var newNavigationPage = CreateNavigationPage(targetPage);

        Detail = newNavigationPage;
        IsPresented = false;

        await PageTransitionHelper.AnimateNavigationSwap(oldDetail, newNavigationPage);
    }

    private static NavigationPage CreateNavigationPage(Page page)
    {
        NavigationPage.SetHasNavigationBar(page, false);

        return new NavigationPage(page)
        {
            BarBackgroundColor = Color.FromArgb("#07080C"),
            BarTextColor = Colors.White
        };
    }
}
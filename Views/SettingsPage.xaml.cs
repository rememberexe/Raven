using RavenMobile.ViewModels;

namespace RavenMobile.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

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
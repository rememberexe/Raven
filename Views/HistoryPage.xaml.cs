using RavenMobile.Models;
using RavenMobile.ViewModels;

namespace RavenMobile.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;

    public HistoryPage(HistoryViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }

    private void MenuButton_Clicked(object? sender, EventArgs e)
    {
        OpenFlyout();
    }

    private async void HistoryCollection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as TransferHistoryItem;

        if (item == null)
            return;

        HistoryCollection.SelectedItem = null;

        await Navigation.PushAsync(new HistoryDetailPage(item));
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
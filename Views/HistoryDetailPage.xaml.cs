using RavenMobile.Models;

namespace RavenMobile.Views;

public partial class HistoryDetailPage : ContentPage
{
    public HistoryDetailPage(TransferHistoryItem item)
    {
        InitializeComponent();

        BindingContext = item;

        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void BackButton_Clicked(object? sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
            await Navigation.PopAsync();
    }
}
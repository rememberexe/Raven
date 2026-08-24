using System.Collections.ObjectModel;
using RavenMobile.Models;

namespace RavenMobile.Views;

public partial class MenuPage : ContentPage
{
    public event Action<string>? MenuSelected;

    public ObservableCollection<MenuItemModel> Items { get; } = new()
    {
        new MenuItemModel
        {
            Title = "Ana Sayfa",
            Subtitle = "Al ve gönder ekranı",
            Icon = "🏠",
            Key = "home"
        },
        new MenuItemModel
        {
            Title = "Transfer Geçmişi",
            Subtitle = "Gönderilen ve alınan dosyalar",
            Icon = "🕘",
            Key = "history"
        },
        new MenuItemModel
        {
            Title = "Ayarlar",
            Subtitle = "Transfer davranışları",
            Icon = "⚙️",
            Key = "settings"
        },
        new MenuItemModel
        {
            Title = "Hakkında",
            Subtitle = "Raven hakkında",
            Icon = "ℹ️",
            Key = "about"
        }
    };

    public MenuPage()
    {
        InitializeComponent();

        MenuCollection.ItemsSource = Items;

        SetActive("home");
    }

    public void SetActive(string key)
    {
        foreach (var item in Items)
            item.IsActive = item.Key == key;
    }

    private void MenuCollection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as MenuItemModel;

        if (item == null)
            return;

        SetActive(item.Key);

        MenuSelected?.Invoke(item.Key);

        MenuCollection.SelectedItem = null;
    }
}
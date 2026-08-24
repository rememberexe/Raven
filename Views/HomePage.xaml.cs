using RavenMobile.Helpers;
using RavenMobile.ViewModels;

namespace RavenMobile.Views;

public partial class HomePage : ContentPage
{
    private bool _receiverAnimationRunning;
    private bool _transferPulseRunning;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        StartReceiverWaitingAnimation();
        StartTransferPulseAnimation();

        await PageTransitionHelper.AnimateIn(this);
    }

    protected override void OnDisappearing()
    {
        _receiverAnimationRunning = false;
        _transferPulseRunning = false;

        base.OnDisappearing();
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

    private async void StartReceiverWaitingAnimation()
    {
        if (_receiverAnimationRunning)
            return;

        _receiverAnimationRunning = true;

        await Task.Delay(500);

        while (_receiverAnimationRunning)
        {
            var dot1 = this.FindByName<VisualElement>("ReceiveDot1");
            var dot2 = this.FindByName<VisualElement>("ReceiveDot2");
            var dot3 = this.FindByName<VisualElement>("ReceiveDot3");

            if (dot1 == null || dot2 == null || dot3 == null)
            {
                await Task.Delay(500);
                continue;
            }

            await AnimateDot(dot1, 0);
            await AnimateDot(dot2, 80);
            await AnimateDot(dot3, 80);

            await Task.Delay(250);
        }
    }

    private async void StartTransferPulseAnimation()
    {
        if (_transferPulseRunning)
            return;

        _transferPulseRunning = true;

        await Task.Delay(400);

        while (_transferPulseRunning)
        {
            var glow = this.FindByName<VisualElement>("TransferPulseGlow");

            if (glow == null || !glow.IsVisible)
            {
                await Task.Delay(500);
                continue;
            }

            try
            {
                glow.Scale = 0.96;
                glow.Opacity = 0.16;

                await Task.WhenAll(
                    glow.ScaleTo(1.05, 850, Easing.CubicInOut),
                    glow.FadeTo(0.34, 850, Easing.CubicInOut)
                );

                await Task.WhenAll(
                    glow.ScaleTo(0.96, 850, Easing.CubicInOut),
                    glow.FadeTo(0.16, 850, Easing.CubicInOut)
                );
            }
            catch
            {
            }
        }
    }

    private static async Task AnimateDot(VisualElement dot, uint delay)
    {
        if (delay > 0)
            await Task.Delay((int)delay);

        await dot.ScaleTo(1.45, 180, Easing.CubicOut);
        await dot.FadeTo(0.35, 180, Easing.CubicIn);
        await dot.ScaleTo(1.0, 180, Easing.CubicInOut);
        await dot.FadeTo(1.0, 180, Easing.CubicOut);
    }
}
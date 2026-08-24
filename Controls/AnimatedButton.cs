namespace RavenMobile.Controls;

public class AnimatedButton : Button
{
    private bool _isAnimating;

    public AnimatedButton()
    {
        Pressed += OnPressed;
        Released += OnReleased;
        Clicked += OnClicked;
    }

    private async void OnPressed(object? sender, EventArgs e)
    {
        if (_isAnimating || !IsEnabled)
            return;

        _isAnimating = true;

        try
        {
            await this.ScaleTo(0.96, 70, Easing.CubicOut);
            await this.FadeTo(0.88, 70, Easing.CubicOut);
        }
        catch
        {
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async void OnReleased(object? sender, EventArgs e)
    {
        try
        {
            await this.ScaleTo(1.0, 110, Easing.CubicOut);
            await this.FadeTo(1.0, 110, Easing.CubicOut);
        }
        catch
        {
        }
    }

    private async void OnClicked(object? sender, EventArgs e)
    {
        try
        {
            await this.ScaleTo(0.98, 45, Easing.CubicOut);
            await this.ScaleTo(1.0, 100, Easing.CubicOut);
        }
        catch
        {
        }
    }
}
namespace RavenMobile.Helpers;

public static class PageTransitionHelper
{
    public static async Task AnimateIn(Page page)
    {
        try
        {
            page.Opacity = 0;
            page.TranslationY = 18;

            await Task.WhenAll(
                page.FadeTo(1, 210, Easing.CubicOut),
                page.TranslateTo(0, 0, 210, Easing.CubicOut)
            );
        }
        catch
        {
        }
    }

    public static async Task AnimateNavigationSwap(Page? oldPage, Page newPage)
    {
        try
        {
            if (oldPage != null)
            {
                await oldPage.FadeTo(0, 90, Easing.CubicIn);
            }

            newPage.Opacity = 0;
            newPage.TranslationX = 18;

            await Task.Delay(60);

            await Task.WhenAll(
                newPage.FadeTo(1, 190, Easing.CubicOut),
                newPage.TranslateTo(0, 0, 190, Easing.CubicOut)
            );
        }
        catch
        {
        }
    }
}
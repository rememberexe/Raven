using RavenMobile.Models;

namespace RavenMobile.Services;

public class AppSettingsService : IAppSettingsService
{
    private const string VibrateOnTransferCompleteKey = "Raven_Settings_VibrateOnTransferComplete";
    private const string AutoClearFilesAfterSendKey = "Raven_Settings_AutoClearFilesAfterSend";
    private const string AutoStopReceiveAfterTransferKey = "Raven_Settings_AutoStopReceiveAfterTransfer";

    private const string OnboardingSeenKey = "Raven_OnboardingSeen";

    public AppSettings GetSettings()
    {
        return new AppSettings
        {
            VibrateOnTransferComplete = Preferences.Default.Get(VibrateOnTransferCompleteKey, true),
            AutoClearFilesAfterSend = Preferences.Default.Get(AutoClearFilesAfterSendKey, true),
            AutoStopReceiveAfterTransfer = Preferences.Default.Get(AutoStopReceiveAfterTransferKey, false)
        };
    }

    public void SaveSettings(AppSettings settings)
    {
        Preferences.Default.Set(VibrateOnTransferCompleteKey, settings.VibrateOnTransferComplete);
        Preferences.Default.Set(AutoClearFilesAfterSendKey, settings.AutoClearFilesAfterSend);
        Preferences.Default.Set(AutoStopReceiveAfterTransferKey, settings.AutoStopReceiveAfterTransfer);
    }

    public void SetVibrateOnTransferComplete(bool value)
    {
        Preferences.Default.Set(VibrateOnTransferCompleteKey, value);
    }

    public void SetAutoClearFilesAfterSend(bool value)
    {
        Preferences.Default.Set(AutoClearFilesAfterSendKey, value);
    }

    public void SetAutoStopReceiveAfterTransfer(bool value)
    {
        Preferences.Default.Set(AutoStopReceiveAfterTransferKey, value);
    }

    public void ResetOnboarding()
    {
        Preferences.Default.Set(OnboardingSeenKey, false);
    }
}
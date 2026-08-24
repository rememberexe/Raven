using RavenMobile.Models;

namespace RavenMobile.Services;

public interface IAppSettingsService
{
    AppSettings GetSettings();

    void SaveSettings(AppSettings settings);

    void SetVibrateOnTransferComplete(bool value);

    void SetAutoClearFilesAfterSend(bool value);

    void SetAutoStopReceiveAfterTransfer(bool value);

    void ResetOnboarding();
}
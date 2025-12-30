using System.Diagnostics;
using System.Runtime.Versioning;
using NAudio.CoreAudioApi;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public static class AudioMuteService
{
    /// <summary>
    /// 指定プロセス名の音声出力をミュート/アンミュート切り替え
    /// </summary>
    public static bool ToggleMute(string processName)
    {
        var session = FindAudioSession(processName);
        if (session == null) return false;

        var newMuteState = !session.SimpleAudioVolume.Mute;
        session.SimpleAudioVolume.Mute = newMuteState;
        return newMuteState;
    }

    /// <summary>
    /// 指定プロセス名の現在のミュート状態を取得
    /// </summary>
    public static bool? GetMuteState(string processName)
    {
        var session = FindAudioSession(processName);
        return session?.SimpleAudioVolume.Mute;
    }

    private static AudioSessionControl? FindAudioSession(string processName)
    {
        using var enumerator = new MMDeviceEnumerator();
        var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var sessionManager = device.AudioSessionManager;

        for (var i = 0; i < sessionManager.Sessions.Count; i++)
        {
            var session = sessionManager.Sessions[i];
            var processId = (int)session.GetProcessID;
            if (processId == 0) continue;

            try
            {
                var process = Process.GetProcessById(processId);
                if (process.ProcessName == processName)
                {
                    return session;
                }
            }
            catch
            {
                // プロセスが見つからない場合は無視
            }
        }

        return null;
    }
}

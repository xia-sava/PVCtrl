using System;
using System.Media;
using System.Runtime.Versioning;
using PVCtrl.Properties;
using Timer = System.Timers.Timer;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
static class RecTimerService
{
    private static Timer? _recTimer;
    private static Action<bool>? _recStopHandler;

    public static void StartRecTimer(int minutes, int alarmMinute, Action<DateTime> elapsedHandler,
        Action<bool> stopHandler)
    {
        var alarmed = alarmMinute == 0;
        var stopTime = DateTime.Now.AddMinutes(minutes);
        _recStopHandler = stopHandler;
        _recTimer = new Timer(1000);
        _recTimer.Elapsed += (_, _) =>
        {
            elapsedHandler(stopTime);
            if (stopTime.AddMinutes(-alarmMinute) < DateTime.Now)
            {
                if (!alarmed)
                {
                    new SoundPlayer(Resources.TimeStopSound).Play();
                    alarmed = true;
                }
            }

            if (stopTime < DateTime.Now)
            {
                StopRecTimer(true);
            }
        };
        _recTimer.Start();
    }

    public static void StopRecTimer(bool recStop)
    {
        if (_recTimer?.Enabled != true) return;
        _recTimer.Stop();
        _recStopHandler?.Invoke(recStop);
    }
}

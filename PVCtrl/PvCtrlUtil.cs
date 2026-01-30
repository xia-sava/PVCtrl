using System;
using System.Media;
using System.Runtime.Versioning;
using PVCtrl.Properties;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
static class RecTimerService
{
    private static IDisposable? _tickSubscription;
    private static Action<bool>? _recStopHandler;

    public static void StartRecTimer(int minutes, int alarmMinute, Action<DateTime> elapsedHandler,
        Action<bool> stopHandler)
    {
        var alarmed = alarmMinute == 0;
        var stopTime = DateTime.Now.AddMinutes(minutes);
        _recStopHandler = stopHandler;
        _tickSubscription = TickService.Subscribe(1, () =>
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
        });
    }

    public static void StopRecTimer(bool recStop)
    {
        if (_tickSubscription == null) return;
        _tickSubscription.Dispose();
        _tickSubscription = null;
        _recStopHandler?.Invoke(recStop);
    }
}

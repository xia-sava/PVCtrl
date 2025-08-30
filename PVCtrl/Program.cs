using System;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace PVCtrl;

static class Program
{
    /// <summary>
    /// アプリケーションのメイン エントリ ポイントです。
    /// </summary>
    [STAThread]
    [SupportedOSPlatform("windows6.1")]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new PvCtrl());
    }
}
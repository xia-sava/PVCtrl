using System.Runtime.Versioning;
using System.Windows.Forms;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public partial class WheelableNumericUpDown
{
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (e is HandledMouseEventArgs hme)
        {
            hme.Handled = true;
        }

        if (e.Delta > 0 && Value + Increment <= Maximum)
        {
            Value += Increment;
        }
        else if (e.Delta < 0 && Value - Increment >= Minimum)
        {
            Value -= Increment;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Helpers
{
    public interface IMasterDisplayWindow
    {
        System.Windows.Window GetWindow();
        IntPtr GetHandle();
        System.Windows.Rect GetWindowRect();
        double GetInitialWidth();
        double GetInitialHeight();
        double GetWidth();
        double GetHeight();
        double GetLeft();
        double GetTop();
        double GetSizeRatio();
    }
}

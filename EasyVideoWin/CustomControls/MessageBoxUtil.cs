using EasyVideoWin.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EasyVideoWin.CustomControls
{
    class MessageBoxUtil
    {
        public static Rect GetOwnerWindowRect(Window messageBox, IMasterDisplayWindow owner)
        {
            if (null == owner)
            {
                return new Rect(0, 0, 0, 0);
            }
            double ratio = 1;
            double ownerLeft = 0;
            double ownerTop = 0;
            double ownerWidth = 0;
            double ownerHeight = 0;

            ownerWidth = owner.GetWidth();
            ownerHeight = owner.GetHeight();
            ownerLeft = owner.GetLeft();
            ownerTop = owner.GetTop();
            ratio = owner.GetSizeRatio();

            ratio = ratio > 0 ? ratio : 1;
            if (ratio != 1)
            {
                double width = messageBox.Width * ratio;
                double height = messageBox.Height * ratio;
                double left = ownerLeft + (ownerWidth - width) / 2;
                double top = ownerTop + (ownerHeight - height) / 2;
                return new Rect(left, top, width, height);
            }
            else
            {
                return new Rect(messageBox.Left, messageBox.Top, messageBox.Width, messageBox.Height);
            }
        }

    }
}

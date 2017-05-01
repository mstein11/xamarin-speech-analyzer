using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Happimeter.UserControls
{
    public class BetterButton : Button
    {
        public BetterButton() : base()
        {
            const int _animationTime = 100;
            Clicked += async (sender, e) =>
            {
                var btn = (BetterButton)sender;
                await btn.ScaleTo(1.2, _animationTime);
                await btn.ScaleTo(1, _animationTime);
            };
        }
    }
}

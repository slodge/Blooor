using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace System.Drawing
{
    public class Bitmap
    {
        private Android.Graphics.Bitmap _backingBitmap = null;

        public Bitmap(Android.Graphics.Bitmap backingBitmap)
        {
            _backingBitmap = backingBitmap;
        }

        public Color GetPixel(int x, int y)
        {
            int argb = _backingBitmap.GetPixel(x, y);
            return Color.FromArgb(argb);
        }
    }
}
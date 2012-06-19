# zxing.MonoDroid
ZXing (pronounced "zebra crossing") is an open-source, multi-format 1D/2D barcode image processing library implemented in Java. Our focus is on using the built-in camera on mobile phones to photograph and decode barcodes on the device, without communicating with a server.
This project is built from the official csharp port from SVN and may be missing functionality.

## Usage
A simple example of using zxing.MonoDroid might look like this:

    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Drawing;
    using com.google.zxing;
    using com.google.zxing.common;

    using Android.App;
    using Android.Content;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using Android.OS;

    namespace Camera.MonoDroid
    {
        [Activity(Label = "BarcodeScanner.NET", MainLauncher = true)]
        public class Activity1 : Activity
        {
            protected override void OnCreate(Bundle bundle)
            {
                base.OnCreate(bundle);

                // Set our view from the "main" layout resource
                SetContentView(Resource.layout.main);

                TextView text = FindViewById<TextView>(Resource.id.barcodeText);

                try
                {
                    var webRequest = (HttpWebRequest)WebRequest.Create(
                        "http://www.theipadfan.com/wp-content/uploads/2010/07/barcode.png"
                    );
                    var response = webRequest.GetResponse();
                    var stream = response.GetResponseStream();
                    var _backbitmap = Android.Graphics.BitmapFactory.DecodeStream(stream);
                    var image = new System.Drawing.Bitmap(_backbitmap);

                    Reader barcodeReader = new MultiFormatReader();
                    LuminanceSource source = new RGBLuminanceSource(image, _backbitmap.Width, _backbitmap.Height);
                    BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));
                    var result = barcodeReader.decode(bitmap);
                    text.Text = result.Text;
                }
                catch (Exception ex)
                {
                    text.Text = ex.ToString();
                }
            }
        }
    }

## zxing
ZXing is released under the Apache 2.0 license.
ZXing can be found here: http://code.google.com/p/zxing/
A copy of the Apache 2.0 license can be found here: http://www.apache.org/licenses/LICENSE-2.0

## System.Drawing
The System.Drawing classes included are from the mono source code which is property of Novell.
Copyright notice is intact in source code files.

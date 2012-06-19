using System;
using System.Collections.Generic;
using Ahwa.Attila.Core.Android.ViewModels.ScanViewModels;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Java.Lang;
using com.google.zxing;
using com.google.zxing.common;
using com.google.zxing.qrcode;
using Camera = Android.Hardware.Camera;
using Double = System.Double;
using Exception = System.Exception;
using Math = System.Math;

namespace Ahwa.Attila.UI.Android.Views.Scan
{
    [Activity(Label = "Scan", WindowSoftInputMode = SoftInput.AdjustPan)]
    public class ScanView : BaseView<ScanViewModel>
    {
        public ScanView()
            : base()
        {
        }

        private Preview preview;

        protected override void OnViewModelSet()
        {
            // Create our Preview view and set it as the content of our activity.
            preview = new Preview(this, this);
            SetContentView(preview);
            preview.Touch += PreviewOnTouch;
        }

        private void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            // should probably do more here
            preview.FindFocus();
        }
    }

    sealed class Preview : SurfaceView, ISurfaceHolderCallback, Camera.IPreviewCallback
    {
        ISurfaceHolder surface_holder;
        Camera camera;
        private ScanView scanView;
        private QRCodeReader _reader;

        public Preview(Context context, ScanView scanView)
            : base(context)
        {
            this.scanView = scanView;
            // Install a SurfaceHolder.Callback so we get notified when the
            // underlying surface is created and destroyed.
            surface_holder = Holder;
            surface_holder.AddCallback(this);
            surface_holder.SetType(SurfaceType.PushBuffers);
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            // The Surface has been created, acquire the camera and tell it where
            // to draw.
            camera = Camera.Open();

            try
            {
                camera.SetPreviewDisplay(holder);
                camera.SetPreviewCallback(this);
            }
            catch (Exception)
            {
                camera.Release();
                camera = null;
                // TODO: add more exception handling logic here
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            // Surface will be destroyed when we return, so stop the preview.
            // Because the CameraDevice object is not a shared resource, it's very
            // important to release it when the activity is paused.
            camera.StopPreview();
            camera.Release();
            camera = null;
        }

        private Camera.Size GetOptimalPreviewSize(IList<Camera.Size> sizes, int w, int h)
        {
            const double ASPECT_TOLERANCE = 0.05;
            double targetRatio = (double)w / h;

            if (sizes == null)
                return null;

            Camera.Size optimalSize = null;
            double minDiff = Double.MaxValue;

            int targetHeight = h;

            // Try to find an size match aspect ratio and size
            for (int i = 0; i < sizes.Count; i++)
            {
                Camera.Size size = sizes[i];
                double ratio = (double)size.Width / size.Height;

                if (Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE)
                    continue;

                if (Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = Math.Abs(size.Height - targetHeight);
                }
            }

            // Cannot find the one match the aspect ratio, ignore the requirement
            if (optimalSize == null)
            {
                minDiff = Double.MaxValue;
                for (int i = 0; i < sizes.Count; i++)
                {
                    Camera.Size size = sizes[i];

                    if (Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = Math.Abs(size.Height - targetHeight);
                    }
                }
            }

            return optimalSize;
        }

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int w, int h)
        {
            // Now that the size is known, set up the camera parameters and begin
            // the preview.
            Camera.Parameters parameters = camera.GetParameters();

            IList<Camera.Size> sizes = parameters.SupportedPreviewSizes;
            Camera.Size optimalSize = GetOptimalPreviewSize(sizes, w, h);

            parameters.SetPreviewSize(optimalSize.Width, optimalSize.Height);

            camera.SetDisplayOrientation(90);
            camera.SetParameters(parameters);
            int dataBufferSize = (int)(optimalSize.Width * optimalSize.Height *
                                           (ImageFormat.GetBitsPerPixel(camera.GetParameters().PreviewFormat) / 8.0));
            _reader = new QRCodeReader();
            camera.StartPreview();
        }

        private bool sentinel = false;
        private DateTime lastScanUtc = DateTime.UtcNow;

        public void DoScan()
        {
            if (camera != null)
            {
                //sentinel = true;
                //camera.SetOneShotPreviewCallback(this);
            }
        }

        #region Implementation of IPreviewCallback

        /*
        private sbyte HackByteConvert(byte input)
        {
            if (input < 128)
            {
                return (sbyte) input;
            }
            else
            {
                return (sbyte)(127 - input);
            }
        }
                sbyte[] sdata = new sbyte[data.Length];
                for (int i = 0; i < data.Length; i++)
                {
                    sdata[i] = HackByteConvert(data[i]);
                }
        */

        public void OnPreviewFrame(byte[] data, Camera camera)
        {
            if (sentinel)
                return;

            if ((DateTime.UtcNow - lastScanUtc).TotalMilliseconds < 400.0)
            {
                return;
            }

            sentinel = true;
            try
            {
                var size = camera.GetParameters().PreviewSize;
                var sdata = (sbyte[])(Array)data;
                //Trace.Info("Length w h {0} {1} {2}", data.Length, size.Width, size.Height);
                var source = new PlanarYUVLuminanceSource(sdata, size.Width, size.Height, 0, 0, size.Width, size.Height, false);
                var binarizer = new HybridBinarizer(source);
                var binBitmap = new BinaryBitmap(binarizer);
                var result = _reader.decode(binBitmap);
                //Trace.Info("A RESULT {0}", result.Text);
                var t = result.Text;


                this.scanView.ViewModel.Scan(result.Text);

                //LuminanceSource source = new RGBLuminanceSource(data, size.Width, size.Height);
                //BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));
                //var result = barcodeReader.decode(bitmap);
            }
            catch (Exception exception)
            {
                //Trace.Info("Exception masked - no qr found");// + exception.ToLongString());
            }
            finally
            {
                sentinel = false;
                lastScanUtc = DateTime.UtcNow;
            }
        }

        #endregion
    }

    public class PlanarYUVLuminanceSource : LuminanceSource
    {

        private readonly sbyte[] yuvData;
        private readonly int dataWidth;
        private readonly int dataHeight;
        private readonly int left;
        private readonly int top;

        public PlanarYUVLuminanceSource(sbyte[] yuvData,
                                        int dataWidth,
                                        int dataHeight,
                                        int left,
                                        int top,
                                        int width,
                                        int height,
                                        bool reverseHorizontal)
            : base(width, height)
        {

            if (left + width > dataWidth || top + height > dataHeight)
            {
                throw new IllegalArgumentException("Crop rectangle does not fit within image data.");
            }

            this.yuvData = yuvData;
            this.dataWidth = dataWidth;
            this.dataHeight = dataHeight;
            this.left = left;
            this.top = top;
            if (reverseHorizontal)
            {
                this.reverseHorizontal(width, height);
            }
        }

        public override sbyte[] getRow(int y, sbyte[] row)
        {
            if (y < 0 || y >= Height)
            {
                throw new IllegalArgumentException("Requested row is outside the image: " + y);
            }
            int width = Width;
            if (row == null || row.Length < width)
            {
                row = new sbyte[width];
            }
            int offset = (y + top) * dataWidth + left;
            Array.Copy(yuvData, offset, row, 0, width);
            return row;
        }

        public override sbyte[] Matrix
        {
            get
            {
                int width = Width;
                int height = Height;

                // If the caller asks for the entire underlying image, save the copy and give them the
                // original data. The docs specifically warn that result.length must be ignored.
                if (width == dataWidth && height == dataHeight)
                {
                    return yuvData;
                }

                int area = width * height;
                sbyte[] matrix = new sbyte[area];
                int inputOffset = top * dataWidth + left;

                // If the width matches the full width of the underlying data, perform a single copy.
                if (width == dataWidth)
                {
                    Array.Copy(yuvData, inputOffset, matrix, 0, area);
                    return matrix;
                }

                // Otherwise copy one cropped row at a time.
                sbyte[] yuv = yuvData;
                for (int y = 0; y < height; y++)
                {
                    int outputOffset = y * width;
                    Array.Copy(yuv, inputOffset, matrix, outputOffset, width);
                    inputOffset += dataWidth;
                }
                return matrix;
            }
        }

        public override bool CropSupported
        {
            get
            {
                return true;
            }
        }

        public override LuminanceSource crop(int left, int top, int width, int height)
        {
            return new PlanarYUVLuminanceSource(yuvData,
                                                dataWidth,
                                                dataHeight,
                                                this.left + left,
                                                this.top + top,
                                                width,
                                                height,
                                                false);
        }

        public Bitmap renderCroppedGreyscaleBitmap()
        {
            int width = Width;
            int height = Height;
            int[] pixels = new int[width * height];
            sbyte[] yuv = yuvData;
            int inputOffset = top * dataWidth + left;

            for (int y = 0; y < height; y++)
            {
                int outputOffset = y * width;
                for (int x = 0; x < width; x++)
                {
                    int grey = yuv[inputOffset + x] & 0xff;
                    pixels[outputOffset + x] = (int)(0xFF000000 | (grey * 0x00010101));
                }
                inputOffset += dataWidth;
            }

            Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            bitmap.SetPixels(pixels, 0, width, 0, 0, width, height);
            return bitmap;
        }

        private void reverseHorizontal(int width, int height)
        {
            sbyte[] yuvData = this.yuvData;
            for (int y = 0, rowStart = top * dataWidth + left; y < height; y++, rowStart += dataWidth)
            {
                int middle = rowStart + width / 2;
                for (int x1 = rowStart, x2 = rowStart + width - 1; x1 < middle; x1++, x2--)
                {
                    sbyte temp = yuvData[x1];
                    yuvData[x1] = yuvData[x2];
                    yuvData[x2] = temp;
                }
            }
        }
    }
}
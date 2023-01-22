using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace uwpwindowcapture
{
    sealed partial class App : Application
    {

        public CanvasControl canvas;

        GraphicsCaptureSession gfx_session;
        CanvasBitmap curr_frame;

        public App()
        {
            InitializeComponent();
        }

        
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                Window.Current.Content = rootFrame = new Frame();
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }

            Capture();
            canvas.Draw += (_, args) =>
            {
                if (curr_frame != null)
                {
                    args.DrawingSession.DrawImage(curr_frame);
                }
            };
        }

        public async void Capture()
        {

            var picker = new GraphicsCapturePicker();
            GraphicsCaptureItem item = await picker.PickSingleItemAsync();

            Console.WriteLine($"{canvas.Device}");

            var framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                canvas.Device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                item.Size
                );

            canvas.Width = item.Size.Width;
            canvas.Height = item.Size.Height;

            framePool.FrameArrived += (pool, args) =>
            {
                using (var frame = pool.TryGetNextFrame())
                {
                    if (frame != null)
                    {
                        
                        curr_frame = CanvasBitmap.CreateFromDirect3D11Surface(
                            canvas.Device,
                            frame.Surface);
                        canvas.Invalidate();
                    }
                }
            };

            gfx_session = framePool.CreateCaptureSession(item);
            gfx_session.StartCapture();
        }


    }
}

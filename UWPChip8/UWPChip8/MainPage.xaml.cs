using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.Brushes;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI.Core;
using Windows.System;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Diagnostics;

namespace UWPChip8
{
    /// <summary>
    /// Our main draw page
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// An image that we consider to be the pixel 'on' color, ie. a pixel byte of 1.
        /// </summary>
        CanvasImageBrush _brush;

        /// <summary>
        /// An image that we consider to be the pixel 'off' color, ie. a pixel byte of 0.
        /// </summary>
        CanvasImageBrush _darkBrush;

        /// <summary>
        /// Reference to our emulator that handles the ticks
        /// </summary>
        Emulator _emulator;

        /// <summary>
        /// Pixel density. A representation of a pixel with a given width/height. Use for scaling to resolutions
        /// ie. default Chip 8 is 64x32, so for 640x320 the pixel size should be 10.
        /// </summary>
        int pixelSize = 10;

        /// <summary>
        /// The constructor for this instance of MainPage
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            _emulator = new Emulator(); // This could be moved to CreateResources if there were any asset dependencies.
        }

        /// <summary>
        /// Handle the Control loaded event for this page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            // Register for keyboard events
            Window.Current.CoreWindow.KeyDown += KeyDown_UIThread;
            Window.Current.CoreWindow.KeyUp += KeyUp_UIThread;
            chooseROMButton.Click += new RoutedEventHandler(ChooseROM_Click);
        }

        /// <summary>
        /// Handle the Control unloaded event for this page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= KeyDown_UIThread;
            Window.Current.CoreWindow.KeyUp -= KeyUp_UIThread;

            animatedControl.RemoveFromVisualTree();
            animatedControl = null;
        }

        /// <summary>
        /// Handle the key up event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void KeyUp_UIThread(CoreWindow sender, KeyEventArgs args)
        {
            char pressedLetter = GetPressedLetter(args);

            if (pressedLetter == 0)
                return;

            args.Handled = true;

            var action = animatedControl.RunOnGameLoopThreadAsync(() => _emulator.ProcessKey(pressedLetter, 0));
        }

        /// <summary>
        /// Handle the key down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void KeyDown_UIThread(CoreWindow sender, KeyEventArgs args)
        {
            char pressedLetter = GetPressedLetter(args);

            if (pressedLetter == 0)
            {
                return;
            }

            args.Handled = true;

            var action = animatedControl.RunOnGameLoopThreadAsync(() => _emulator.ProcessKey(pressedLetter, 1));
        }

        /// <summary>
        /// Converts virtual keys into chars. This is taken from an example on keyboard handling in Win2D
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static char GetPressedLetter(KeyEventArgs args)
        {
            var key = args.VirtualKey;
            char pressed = (char)0;

            switch (key)
            {
                case VirtualKey.Number1: pressed = '1'; break;
                case VirtualKey.Number2: pressed = '2'; break;
                case VirtualKey.Number3: pressed = '3'; break;
                case VirtualKey.Number4: pressed = '4'; break;
                case VirtualKey.A: pressed = 'A'; break;
                case VirtualKey.C: pressed = 'C'; break;
                case VirtualKey.D: pressed = 'D'; break;
                case VirtualKey.E: pressed = 'E'; break;
                case VirtualKey.F: pressed = 'F'; break;
                case VirtualKey.Q: pressed = 'Q'; break;
                case VirtualKey.R: pressed = 'R'; break;
                case VirtualKey.S: pressed = 'S'; break;
                case VirtualKey.V: pressed = 'V'; break;
                case VirtualKey.W: pressed = 'W'; break;
                case VirtualKey.X: pressed = 'X'; break;
                case VirtualKey.Z: pressed = 'Z'; break;
            }

            return pressed;
        }

        /// <summary>
        /// Our 60hz update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CanvasAnimatedControl_Update(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {
            if (!_emulator.PoweredUp)
                return;

            _emulator.ExecuteIteration();
        }

        /// <summary>
        /// Draws the 'pixel' blocks stored in the CPUs display buffer at a density of _pixelSize;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CanvasAnimatedControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            if (!_emulator.PoweredUp)
                return;

            bool[] displayBuffer = _emulator.DisplayBuffer;
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (displayBuffer[(y * 64) + x] != false)
                    {
                        args.DrawingSession.FillRectangle(new Windows.Foundation.Rect(x * pixelSize, y * pixelSize, pixelSize, pixelSize), _brush);
                    }
                    else
                    {
                        args.DrawingSession.FillRectangle(new Windows.Foundation.Rect(x * pixelSize, y * pixelSize, pixelSize, pixelSize), _darkBrush);
                    }
                }
            }
        }

        /// <summary>
        /// Handle the Create Resources event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CanvasAnimatedControl_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            // Ensure the CreateResources task is completed before considering that the internal create resources is itself completed.
            args.TrackAsyncAction(CreateResources(sender).AsAsyncAction());
        }

        /// <summary>
        /// Task to load our resources
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        async Task CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender)
        {
            _darkBrush = new CanvasImageBrush(sender);
            _darkBrush.Image = await CanvasBitmap.LoadAsync(sender, "Assets/dark_brush.jpg");
            _brush = new CanvasImageBrush(sender);
            _brush.Image = await CanvasBitmap.LoadAsync(sender, "Assets/brush.jpg");
        }

        /// <summary>
        /// Handles the file picker event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChooseROM_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add("*");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                _emulator.Initialize();
                await _emulator.LoadRom(file);
                
                Debug.WriteLine(file.Path);
            }
            else
            {
                Debug.WriteLine("Cancelled");
            }
        }
    }
}

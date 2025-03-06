using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PhotoUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private DispatcherTimer timer;
        private bool isStarted;
        private CameraCaptureUI dialog;
        public MainPage()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 1, 1);
            
            timer.Start();
            
        }

        private async void Timer_Tick(object sender, object e)
        {
            if (!isStarted)
            {
                timer.Stop();
                CameraCaptureUI cCapture = new CameraCaptureUI();
                                
                cCapture.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
                cCapture.PhotoSettings.CroppedSizeInPixels = new Size(1200, 1500);
                StorageFile sf = await cCapture.CaptureFileAsync(CameraCaptureUIMode.Photo);
                if (sf != null)
                {
                    StorageFolder sfolder = KnownFolders.PicturesLibrary;

                    IStorageItem item = await sfolder.TryGetItemAsync("face.bmp");
                    if (item != null)
                    {
                        if (item.IsOfType(StorageItemTypes.File))
                        {
                            StorageFile sfExisting = (StorageFile)item;
                            await sf.CopyAndReplaceAsync(sfExisting);
                        }
                    }
                    else
                    {
                        await sf.CopyAsync(sfolder, "face.bmp");
                    }
                }
                               
                isStarted = true;
                
                CoreApplication.Exit();
            }
        }
    }
}

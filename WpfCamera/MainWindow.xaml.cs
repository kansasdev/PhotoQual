using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.System;
using WpfAboutView;
using Path = System.IO.Path;

namespace WpfCamera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private BackgroundWorker _bw;
        private QAFace.FaceAnalysisEntity _fae;
        public MainWindow()
        {
            InitializeComponent();
            DeleteFaceFile(false);
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, new EventHandler(_timer_Elapsed), Dispatcher.CurrentDispatcher);
            _bw = new BackgroundWorker();
            _bw.DoWork += _bw_DoWork;
            _bw.RunWorkerCompleted += _bw_RunWorkerCompleted;

            _fae = new QAFace.FaceAnalysisEntity();
            
        }

        #region Backround worker methods
        private void _bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnImage.IsEnabled = true;
            btnQuality.IsEnabled = true;
            btnAbout.IsEnabled = true;
            btnPrerequisites.IsEnabled = true;
            pbWaitProgress.IsIndeterminate = false;
            if (e.Error == null)
            {
                string results = e.Result.ToString();
                if (!string.IsNullOrEmpty(results))
                {
                    string[] resSplitted = results.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    tbUnifiedQualityScore.Text = resSplitted[0];
                    tbBackgroundUniformity.Text = resSplitted[1];
                    tbIlluminationUniformity.Text = resSplitted[2];
                    tbLuminanceMean.Text = resSplitted[3];
                    tbLuminanceVariance.Text = resSplitted[4];
                    tbUnderExposurePrevention.Text = resSplitted[5];
                    tbOverExposurePrevention.Text = resSplitted[6];
                    tbDynamicRange.Text = resSplitted[7];
                    tbSharpness.Text = resSplitted[8];
                    tbCompressionArtifacts.Text = resSplitted[9];
                    tbNaturalColour.Text = resSplitted[10];
                    tbSingleFacePresent.Text = resSplitted[11];
                    tbEyesOpen.Text = resSplitted[12];
                    tbMouthClosed.Text = resSplitted[13];
                    tbEyesVisible.Text = resSplitted[14];
                    tbMouthOcclussionPrevention.Text = resSplitted[15];
                    tbFaceOcclusionPrevention.Text = resSplitted[16];
                    tbInterEyeDistance.Text = resSplitted[17];
                    tbHeadSize.Text = resSplitted[18];
                    tbLeftwardCropOfTheFaceImage.Text = resSplitted[19];
                    tbRightwardCropOfTheFaceImage.Text= resSplitted[20];
                    tbMarginAboveOfTheFaceImage.Text=resSplitted[21]; 
                    tbMarginBelowOfTheFaceImage.Text = resSplitted[22];
                    tbHeadPoseYaw.Text = resSplitted[23];
                    tbHeadPosePitch.Text = resSplitted[24];
                    tbHeadPoseRoll.Text = resSplitted[25];
                    tbExpressionNeutrality.Text = resSplitted[26];
                    tbNoHeadCoverings.Text = resSplitted[27];

                }
            }
            else
            {
                MessageBox.Show("Error: " + e.Error.Message);
            }

        }

        private void _bw_DoWork(object sender, DoWorkEventArgs e)
        {
            
            int res = _fae.CalculateQuality();
            if (res > 0)
            {
                e.Result = _fae.GetQualityResults();
            }
            else
            {
                throw new Exception("Error inside QAFace library: "+_fae.GetExceptionMessage());
            }
        }

        #endregion

        #region Button events
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            DeleteFaceFile(true);
            _fae.SetQualityResults();
            _fae.SetExceptionMessage();
            _timer.Stop();
            _timer.IsEnabled = false;
            var res = Launcher.LaunchUriAsync(new Uri("photo.photouwp:"));
            if (res.Status == Windows.Foundation.AsyncStatus.Started || res.Status==Windows.Foundation.AsyncStatus.Completed)
            {
                _timer.Start();
                _timer.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("No PhotoUWP installed");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DeleteFaceFile(true);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            _fae.SetQualityResults();
            _fae.SetExceptionMessage();
            if (openFileDialog.ShowDialog() == true)
            {
                
                byte[] b = File.ReadAllBytes(openFileDialog.FileName);
                DeleteFaceFile(false);
                if(!_timer.IsEnabled)
                {
                    _timer.Start();
                }
                File.WriteAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\face.bmp", b);
                
            }
        }
        private void Button_About(object sender, RoutedEventArgs e)
        {
            new AboutDialog
            {
                Owner = this,
                AboutView = (AboutView)Resources["WpfCameraAboutView"]
            }.ShowDialog();

        }

        private void Button_Prerequisites(object sender, RoutedEventArgs e)
        {
            bool result = AreThereAnyModelFileMissing();

            

                MessageBoxResult resultQuestion = MessageBox.Show(
            result==true?"Apllication is going to download from ISO site required models to data folder. Are there write permissions set up?": "All models exists, do you want to download it again? Are there write permissions set up?",
            "Question",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

                if (resultQuestion == MessageBoxResult.Yes)
                {
                    BackgroundWorker bwDonload = new BackgroundWorker();
                    btnImage.IsEnabled = false;
                    btnQuality.IsEnabled = false;
                    btnAbout.IsEnabled = false;
                    btnPrerequisites.IsEnabled = false;
                    pbWaitProgress.IsIndeterminate = true;
                    bwDonload.DoWork += (s, ee) =>
                    {
                        DownloadAndUnpackRequiredModels();
                    };
                    bwDonload.RunWorkerCompleted += (ss, eee) =>
                    {
                        btnImage.IsEnabled = true;
                        btnQuality.IsEnabled = true;
                        btnAbout.IsEnabled = true;
                        btnPrerequisites.IsEnabled = true;
                        pbWaitProgress.IsIndeterminate = false;
                        if(eee.Error != null)
                        {
                            MessageBox.Show("Error: " + eee.Error.Message);
                        }
                    };
                    bwDonload.RunWorkerAsync();
                }
                else
                {
                    if(result)
                    {
                        MessageBox.Show("There are required models missing inside data folder, please redownload. Remember about write permissions to directory");
                    }
                    

                }
            
        }

        #endregion

        #region Functions
        private void DeleteFaceFile(bool doRemoveTextAndImage)
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\face.bmp"))
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\face.bmp");
            }
            if(doRemoveTextAndImage)
            {
                imageHost.Source = null;
                tbUnifiedQualityScore.Text = "";
                tbBackgroundUniformity.Text = "";
                tbIlluminationUniformity.Text = "";
                tbLuminanceMean.Text ="";
                tbLuminanceVariance.Text = "";
                tbUnderExposurePrevention.Text = "";
                tbOverExposurePrevention.Text = "";
                tbDynamicRange.Text = "";
                tbSharpness.Text = "";
                tbCompressionArtifacts.Text = "";
                tbNaturalColour.Text = "";
                tbSingleFacePresent.Text = "";
                tbEyesOpen.Text = "";
                tbMouthClosed.Text = "";
                tbEyesVisible.Text = "";
                tbMouthOcclussionPrevention.Text = "";
                tbFaceOcclusionPrevention.Text = "";
                tbInterEyeDistance.Text = "";
                tbHeadSize.Text = "";
                tbLeftwardCropOfTheFaceImage.Text = "";
                tbRightwardCropOfTheFaceImage.Text = "";
                tbMarginAboveOfTheFaceImage.Text = "";
                tbMarginBelowOfTheFaceImage.Text = "";
                tbHeadPoseYaw.Text = "";
                tbHeadPosePitch.Text = "";
                tbHeadPoseRoll.Text = "";
                tbExpressionNeutrality.Text = "";
                tbNoHeadCoverings.Text = "";
            }
        }

        private bool AreThereAnyModelFileMissing()
        {
            bool requiredModelDoesntExist = false;

            if(!File.Exists("data\\models\\expression_neutrality\\grimmer\\hse_1_2_C_adaboost.yml.gz"))
            {
                requiredModelDoesntExist = true;
            }
            if (!File.Exists("data\\models\\expression_neutrality\\hsemotion\\enet_b0_8_best_vgaf_embed_zeroed.onnx"))
            {
                requiredModelDoesntExist = true;
            }
            if(!File.Exists("data\\models\\expression_neutrality\\hsemotion\\enet_b2_8_embed_zeroed.onnx"))
            {
                requiredModelDoesntExist = true;
            }


            if (!File.Exists("data\\models\\face_landmark_estimation\\ADNet.onnx"))
            {
                requiredModelDoesntExist = true;
            }

            if (!File.Exists("data\\models\\face_occlusion_segmentation\\face_occlusion_segmentation_ort.onnx"))
            {
                requiredModelDoesntExist = true;
            }
            if (!File.Exists("data\\models\\face_parsing\\bisenet_400.onnx"))
            {
                requiredModelDoesntExist = true;
            }
            if(!File.Exists("data\\models\\head_pose_estimation\\mb1_120x120.onnx"))
            {
                requiredModelDoesntExist = true;
            }
            if(!File.Exists("data\\models\\no_compression_artifacts\\ssim_248_model.onnx"))
            {
                requiredModelDoesntExist = true;
            }
            if(!File.Exists("data\\models\\sharpness\\face_sharpness_rtree.xml.gz"))
            {
                requiredModelDoesntExist = true;
            }
            if(!File.Exists("data\\models\\unified_quality_score\\magface_iresnet50_norm.onnx"))
            {
                requiredModelDoesntExist = true;
            }

            return requiredModelDoesntExist;
        }

        private void DownloadAndUnpackRequiredModels()
        {
            WebClient wc = new WebClient();
            byte[] models = wc.DownloadData("https://standards.iso.org/iso-iec/29794/-5/ed-1/en/OFIQ-MODELS.zip");
            string dir = "data";
            DecompressZipByteArray(models, dir);
        }

        private void DecompressZipByteArray(byte[] zipBytes, string outputFolder)
        {
            using (MemoryStream zipStream = new MemoryStream(zipBytes))
            {
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationPath = Path.Combine(outputFolder, entry.FullName);

                        // Ensure the directory exists
                        string directoryPath = Path.GetDirectoryName(destinationPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Extract the file
                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            entry.ExtractToFile(destinationPath, overwrite: true);
                        }
                    }
                }
            }
        }
        private void _timer_Elapsed(object sender, System.EventArgs e)
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\face.bmp"))
            {
                _timer.Stop();
                _timer.IsEnabled = false;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bi.UriSource = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\face.bmp");
                bi.EndInit();
                imageHost.Source = bi;
                bi = null;

                btnImage.IsEnabled = false;
                btnQuality.IsEnabled = false;
                btnAbout.IsEnabled = false;
                btnPrerequisites.IsEnabled = false;
                pbWaitProgress.IsIndeterminate = true;

                _bw.RunWorkerAsync();

            }
        }
        #endregion

    }
}
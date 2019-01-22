using System;
using System.Threading;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace WebBrowser_UWP_Project
{

    public sealed partial class DownloadsTab : Page
    {
        Point initialPoint;

        DownloadOperation operation;
        CancellationTokenSource cts;
        BackgroundDownloader downloader = new BackgroundDownloader();
        internal static int DownloadTrack = 0;
        static int[] copyies = new int[5];
        static Uri[] multiple = new Uri[5];

        public DownloadsTab()
        {
            this.InitializeComponent();
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                Uri url = new Uri(e.Parameter.ToString());
                bool AlreadyAdded = false;
                for (int i = 0; i < 5; i++)
                {
                    if (url == multiple[i])
                    {
                        AlreadyAdded = true;
                        break;
                    }
                }

                if (AlreadyAdded == true)
                {
                    var DownloadExistDialog = new ContentDialog()
                    {
                        Title = "Download already exist"
                    };
                    var panel = new StackPanel();
                    var messageText = new TextBlock();
                    messageText.Text = "Another download with the same server credentials already exist.";
                    var attentionText = new TextBlock();
                    attentionText.Text = "Would you like to add this download in the download's list again";
                    var DontAddBtn = new Button();
                    DontAddBtn.Content = "Don't add again";
                    var AddHyper = new HyperlinkButton();
                    AddHyper.Content = "Yes add it again";

                    panel.Children.Add(messageText);
                    panel.Children.Add(attentionText);
                    panel.Children.Add(DontAddBtn);
                    panel.Children.Add(AddHyper);
                    DownloadExistDialog.Content = panel;
    
                    await DownloadExistDialog.ShowAsync();

                    if (DontAddBtn.IsPressed == true)
                    {
                        DownloadExistDialog.Hide();
                        string fileName = ResolveName(e.Parameter.ToString());
                        StartNewDownload(url, fileName);
                    }
                    else if (AddHyper.IsPressed == true)
                        DownloadExistDialog.Hide();

                }
                else
                {
                    string fileName = ResolveName(e.Parameter.ToString());
                    StartNewDownload(url, fileName);
                }

            }
            else
            {

            }
        }

        private async void StartNewDownload(Uri uri,string fileName)
        {
            
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder downloadfolder = await folderPicker.PickSingleFolderAsync();

            if (folderPicker != null)
            {
                try
                {
                    StorageFile downloadFile = await downloadfolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);                
                    operation = downloader.CreateDownload(uri, downloadFile);

                    int downloadTrack = RegisterNewDownload(uri);

                    TextBlock fileNameText = new TextBlock();
                    fileNameText.Text = fileName;
                    ProgressBar ProgressIndicator = new ProgressBar();
                    ProgressIndicator.Name = "progress" + downloadTrack;
                    TextBlock progressText = new TextBlock();
                    progressText.Name = "progressText" + downloadTrack;

                    StackPanel panel = new StackPanel();
                    panel.Children.Add(fileNameText);
                    panel.Children.Add(ProgressIndicator);
                    panel.Children.Add(progressText);
                    DownloadsStack.Items.Add(panel);

                    cts = new CancellationTokenSource();
                    Progress<DownloadOperation> progressCallBack = new Progress<DownloadOperation>(Progress);
                    await operation.StartAsync().AsTask(cts.Token, progressCallBack);
                    
                }
                catch (Exception)
                {                   
                    operation = null;
                }
            }
        }

        private void Progress(DownloadOperation operation)
        {
            Uri uri = operation.RequestedUri;
            int currentObject = GetOngoingDownload(uri);
            

            ProgressBar c = new ProgressBar();
            c = (ProgressBar)(DownloadsStack.FindName("progress" + currentObject));
            TextBlock c1 = new TextBlock();
            c1 = (TextBlock)(DownloadsStack.FindName("progressText" + currentObject));

            double received = operation.Progress.BytesReceived;
            double toReceive = operation.Progress.TotalBytesToReceive;
            double progress = received * 100 / toReceive;
            ((ProgressBar)c).Value = progress;
            c1.Text = received + "KB received of " + toReceive + "KB";

            if (received == toReceive)
            {
                c1.Text = "Download Completed";
                RemoveFinishedDownload(currentObject);

            }
        }

        private string ResolveName(string fileName)
        {
            if (fileName.Contains("/"))
            {
                int last = fileName.LastIndexOf("/");
                fileName = fileName.Substring(last + 1, fileName.Length - last - 1);
            }
            if (fileName.Contains("%20"))
            {
                fileName = fileName.Replace("%20", " ");
            }

            return fileName;
        }

        private int RegisterNewDownload(Uri uri)
        {
            if (DownloadTrack >= 5)
                DownloadTrack = 0;

            for(int i = 0; i < 5; i++)
            {
                if (multiple[i] == null)
                {
                    multiple[i] = uri;
                    copyies[i] = i;
                    DownloadTrack = i;
                    break;
                }
            }           
            return DownloadTrack++;
        }

        private int GetOngoingDownload(Uri uri)
        {
            int i, r = 0;
            for (i = 0; i < 5; i++)
            {
                if (multiple[i] == uri)
                {
                    r = i;
                    break;
                }
            }
            return r;
        }

        private void RemoveFinishedDownload(int finishedTrack)
        {
            multiple[finishedTrack] = null;
            copyies[finishedTrack] = 99;
        }

        private void DownloadsStack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DownloadsStack.ContextFlyout.ShowAt(sender as FrameworkElement);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Bookmarkbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BookmarksTab));
        }

        private void Historybtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HistoryTab));
        }

        private void RelativePanel_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += DownloadsTab_BackRequested;
        }

        private void DownloadsTab_BackRequested(object sender, BackRequestedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void Page_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            initialPoint = e.Position;
        }

        private void Page_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            Point finalPoint = e.Position;

            if (finalPoint.X - initialPoint.X > 50)
            {
                this.Frame.Navigate(typeof(HistoryTab));
            }
        }
    }
}

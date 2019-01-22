using System;
using System.IO;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WebBrowser_UWP_Project
{
    public sealed partial class HistoryTab : Page
    {
        Point initialPoint;

        ListBox urlBox = new ListBox();

        public HistoryTab()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += HistoryTab_BackRequested;

            try
            {
                StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                StorageFile bHistoryUri = await tempFolder.GetFileAsync("bHistoryURL.txt");
                StorageFile bHistoryTitle = await tempFolder.GetFileAsync("bHistoryName.txt");

                string wholeTitle = await FileIO.ReadTextAsync(bHistoryTitle);
                using (StringReader Reader = new StringReader(wholeTitle))
                {
                    string newline;
                    while ((newline = Reader.ReadLine()) != null)
                    {
                        HistoryList.Items.Add(newline);
                    }
                }

                string wholeUri = await FileIO.ReadTextAsync(bHistoryUri);
                using (StringReader Reader = new StringReader(wholeUri))
                {
                    string newline;
                    while ((newline = Reader.ReadLine()) != null)
                    {
                        urlBox.Items.Add(newline);
                    }
                }
            }
            catch (Exception)
            {
                StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                StorageFile firsttime = await tempFolder.CreateFileAsync("bHistoryURL.txt");
                firsttime = await tempFolder.CreateFileAsync("bHistoryName.txt");
            }
        }

        private void HistoryTab_BackRequested(object sender, BackRequestedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int selectedHistoryIndex = HistoryList.SelectedIndex;
                string HistoryItem = urlBox.Items[selectedHistoryIndex].ToString();
                App.urlFromOtherWindows = HistoryItem;
                this.Frame.Navigate(typeof(MainPage));
            }
            catch (Exception)
            {
                var errorMessage = new MessageDialog("Selected element is not valid or doesn't have valid URL.", "Error ");
                await errorMessage.ShowAsync();
            }
        }

        private void Bookmarkbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BookmarksTab));
        }

        private void Downloadbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DownloadsTab));
        }

        private void Page_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            initialPoint = e.Position;
        }

        private void Page_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial)
            {
                Point finalPoint = e.Position;
                if (finalPoint.X - initialPoint.X > 50)
                {
                    e.Complete();
                    this.Frame.Navigate(typeof(DownloadsTab));
                }
                else if(initialPoint.X-finalPoint.X > 100)
                {
                    e.Complete();
                    this.Frame.Navigate(typeof(BookmarksTab));
                }
            }
        }

    }
}

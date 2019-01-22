using System;
using System.IO;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace WebBrowser_UWP_Project
{
    public sealed partial class BookmarksTab : Page
    {
        private Point initialPoint;

        ListBox urlBox = new ListBox();

        public BookmarksTab()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += BookmarksTab_BackRequested;

            try
            {
                StorageFolder bookFolder = ApplicationData.Current.LocalFolder;
                StorageFile bookTitleFile = await bookFolder.GetFileAsync("BookTitle.txt");
                StorageFile bookUriFile = await bookFolder.GetFileAsync("BookUri.txt");

                string wholeTitle = await FileIO.ReadTextAsync(bookTitleFile);
                using (StringReader Reader = new StringReader(wholeTitle))
                {
                    string newline;
                    while ((newline = Reader.ReadLine()) != null)
                    {
                        BookmarkTitleList.Items.Add(newline);
                    }
                }

                string wholeUri = await FileIO.ReadTextAsync(bookUriFile);
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
                StorageFolder bookFolder = ApplicationData.Current.LocalFolder;
                StorageFile firstTime = await bookFolder.CreateFileAsync("BookTitle.txt");
                firstTime = await bookFolder.CreateFileAsync("BookUri.txt");
            }
        }

        private void BookmarksTab_BackRequested(object sender, BackRequestedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void BookmarksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedBookmarkIndex = BookmarkTitleList.SelectedIndex;
            string selectedBookmark = urlBox.Items[selectedBookmarkIndex].ToString();
            App.urlFromOtherWindows = selectedBookmark;
            this.Frame.Navigate(typeof(MainPage));
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
                if (initialPoint.X - finalPoint.X > 50)
                {
                    e.Complete();
                    this.Frame.Navigate(typeof(HistoryTab));
                }
            }
        }

        private void Historybtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HistoryTab));
        }

        private void Downloadbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DownloadsTab));
        }

    }
}

using System;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Popups;
using Windows.Storage.Streams;
using System.Threading;
using Windows.UI.Core;

namespace WebBrowser_UWP_Project
{
    public sealed partial class MainPage : Page
    {
        
        //make webform not loading anything by default
        private bool navigating = false;

        //settings container/variable
        public ApplicationDataContainer AllSettings = ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            this.InitializeComponent();
        }

        //first method to be called upon starting the app
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            //check to see if app is loaded just now
            if (App.started == true)
            {
                try
                {  //if app is loaded just now then try to navigate to homepage
                    MainWeb.Navigate(new Uri((string)AllSettings.Values["HomePage"]));
                    AllSettings.Values["Incognito"] = "Off";
                }
                catch (Exception)
                {
                    //if try fails, probably this is the first time app is launched and hence we need to set up envionment
                    AllSettings.Values["HomePage"] = " ";
                    AllSettings.Values["Incognito"] = "Off";
                }

                try
                {
                    var downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
                    foreach (var download in downloads)
                    {
                        CancellationTokenSource cts = new CancellationTokenSource();
                        await download.AttachAsync().AsTask(cts.Token);
                        cts.Cancel();
                    }
                }
                catch (Exception)
                {

                }


                //app is launched just now hence no history or future or present, so disable all buttons
                Backbtn.IsEnabled = false;
                Forwardbtn.IsEnabled = false;
                Stop_Refreshbtn.IsEnabled = false;
                App.started = false;//give sign that app is now running so that next time, we won't fall here
            }
            //if we've got a url from history that needs to launched then check its existence
            else if (!string.IsNullOrEmpty(App.urlFromOtherWindows))
            {
                MainWeb.Navigate(new Uri(App.urlFromOtherWindows));//and navigate to it
                App.urlFromOtherWindows = null;
            }

        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (MainWeb.CanGoBack)
            {
                MainWeb.GoBack();
                e.Handled = true;
            }
            else
            {

            }
        }


        /*this method writes the specified 'content' string to the 'filename' string at the begining*/
        private static async void WriteHistoryFile(string filename, string content)
        {
            try
            {
                StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;//open app temp folder

                StorageFile oldHistory = await tempFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);//open the specified file
                string oldContent = await FileIO.ReadTextAsync(oldHistory);//copy old content

                StorageFile newHistory = await tempFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);//create a new copy of the file

                content += Environment.NewLine;//add newline to the file
                await FileIO.WriteTextAsync(newHistory, content);//write current string provided
                await FileIO.AppendTextAsync(newHistory, oldContent);//append the old string to new one
            }
            catch (Exception)
            {
                StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;//open app temp folder
                StorageFile oldHistory = await tempFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            }
            
        }

        private static async void WriteBookmarkFile(string filename, string content)
        {
            try
            {
                StorageFolder bookFolder = ApplicationData.Current.LocalFolder;
                StorageFile BookmarkFile = await bookFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(BookmarkFile, content + Environment.NewLine);
            }
            catch (Exception)
            {
                StorageFolder bookFolder = ApplicationData.Current.LocalFolder;
                StorageFile BookmarkFile = await bookFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            }
        }

        //main menu/hamburger button method
        private void MenuClick(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        //method is called when webpage has just started loading
        private void MainWeb_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            //following boolean value indicates that something is being loaded currently
            navigating = true;

            //indicate that browser is loading something
            LoadingRing.IsActive = true;

            //change content of url textbox to absolute address
            string currentURL = MainWeb.Source.AbsoluteUri.ToString();
            URLtb.Text = currentURL;

            //make rest of the changes to button controls
            Stop_Refreshbtn.IsEnabled = true;
            Stop_Refreshbtn.Content = "\xE711";
            Stop_RefreshText.Text = "Stop";

        }

        //method is called when webpage has completed loading
        private void MainWeb_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            //browser is not loading anything
            navigating = false;

            //indicate that browser is not loading anything
            LoadingRing.IsActive = false;

            //make rest of the changes to button controls
            Stop_Refreshbtn.IsEnabled = true;
            Stop_Refreshbtn.Content = "\xE72C";
            Stop_RefreshText.Text = "Refresh";

            //if there's a previous page in history, then enable backbutton
            if (MainWeb.CanGoBack == true)
                Backbtn.IsEnabled = true;
            else   //otherwise keep it disabled
                Backbtn.IsEnabled = false;

            //if there's next page available , then enable forwardbutton
            if (MainWeb.CanGoForward == true)
                Forwardbtn.IsEnabled = true;
            else  //otherwise keep it disabled
                Forwardbtn.IsEnabled = false;

            string getURL;

            if (((string)AllSettings.Values["Incognito"]) == "Off")
            {
                //write page title currently loaded into historyname file
                string getName = MainWeb.DocumentTitle.ToString();
                WriteHistoryFile("bHistoryName.txt", getName);

                //write page url currently loaded into historyurl file
                getURL = MainWeb.Source.AbsoluteUri.ToString();
                WriteHistoryFile("bHistoryURL.txt", getURL);
            }
            else
            {
                getURL = MainWeb.Source.AbsoluteUri.ToString();
            }

            //set url textbox to absolute url of web page
            if (getURL.Contains("Startup_Page.html") == true)
                URLtb.Text = "Magnum Startup Page";
            else if (getURL.Contains("Cannot_Load_Error.html") == true)
                URLtb.Text = "Navigation to the Webpage Cancelled";
            else
                URLtb.Text = getURL;


        }

        //method is called if webpage couldn't be loaded
        private async void MainWeb_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            //we want to display the error page that is saved in app installation directory
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Cannot_Load_Error.html"));
            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Folder", CreationCollisionOption.OpenIfExists);
            //we cannot display the original error page, hence copy it to another location
            await file.CopyAsync(folder, "Cannot_Load_Error.html", NameCollisionOption.ReplaceExisting);
            //show the error page
            MainWeb.Navigate(new Uri("ms-appdata:///local/Folder/Cannot_Load_Error.html"));
        }

        //method enforces browser to open new pages in the current screen if it request for new page
        private void MainWeb_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            Uri fileURL = args.Uri;
            /*string fileName = MainWeb.Source.AbsolutePath;
            if (fileName.Contains("/"))
            {
                int last = fileName.LastIndexOf("/");
                fileName = fileName.Substring(last + 1, fileName.Length).Trim();
            }*/
            
            
        }

        private void MainWeb_ContainsFullScreenElementChanged(WebView sender, object args)
        {
            var applicationView = ApplicationView.GetForCurrentView();
            if (sender.ContainsFullScreenElement)
            {
                applicationView.TryEnterFullScreenMode();
            }
            else if (applicationView.IsFullScreenMode)
            {
                applicationView.ExitFullScreenMode();
            }
        }

        private void MainWeb_UnviewableContentIdentified(WebView sender, WebViewUnviewableContentIdentifiedEventArgs args)
        {
            this.Frame.Navigate(typeof(DownloadsTab), args.Uri);
        }


        //method is called when enter is pressed after typing url
        private async void URLtb_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //try to resolve the url
            try
            {
                int searchIndex = (int) AllSettings.Values["SearchEngine"];

                //accept the final url only if 'enter' key is pressed
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    string bURL = URLtb.Text.ToString();
                    string fURL;
                   
                    if(bURL.Contains(".com")==true || bURL.Contains(".")==true)
                    {
                        string extra = "http://";

                        //if url is already resolved then don't do anything
                        if (bURL.StartsWith("http://") == true || bURL.StartsWith("https://") == true || bURL.Contains("://") == true)
                            fURL = bURL;
                        //if url doesn't contain 'www' but ends with '.com' then add 'www.'
                        else if (bURL.StartsWith("www.") == false && bURL.EndsWith(".com") == true)
                            fURL = extra + "www." + bURL;
                        //if url is of different type other than containing '.com'
                        else if (bURL.Contains(".") == true)
                            fURL = extra + bURL;
                        //if url doesn't contain 'www' and 'com' as well then add both
                        else if (bURL.EndsWith(".com") == false)
                            fURL = extra + "www." + bURL + ".com";
                        //if url contains both then add only 'http://' prefix
                        else
                            fURL = extra + bURL;
                    }
                    else if (searchIndex == 0)
                    {
                        fURL = "https://www.google.co.in/?gfe_rd=cr&ei=DXE1WaOLH9eB1ATb3ZXwDQ#q=" + bURL;
                    }
                    else if (searchIndex == 1)
                    {
                        fURL = "https://www.bing.com/search?q=" + bURL;
                    }
                    else if (searchIndex == 2)
                    {
                        fURL = "https://in.search.yahoo.com/search;_ylc=X3oDMTFiaHBhMnJmBF9TAzIwMjM1MzgwNzUEaXRjAzEEc2VjA3NyY2hfcWEEc2xrA3NyY2hhc3Q-?p=" + bURL;
                    }
                    else if (searchIndex == 3)
                    {
                        fURL = "https://duckduckgo.com/?q=" + bURL;
                    }
                    else
                    {
                        string extra = "http://";

                        //if url is already resolved then don't do anything
                        if (bURL.StartsWith("http://") == true || bURL.StartsWith("https://") == true)
                            fURL = bURL;
                        //if url doesn't contain 'www' but ends with '.com' then add 'www.'
                        else if (bURL.StartsWith("www.") == false && bURL.EndsWith(".com") == true)
                            fURL = extra + "www." + bURL;
                        //if url is of different type other than containing '.com'
                        else if (bURL.Contains(".") == true)
                            fURL = extra + bURL;
                        //if url doesn't contain 'www' and 'com' as well then add both
                        else if (bURL.EndsWith(".com") == false)
                            fURL = extra + "www." + bURL + ".com";
                        //if url contains both then add only 'http://' prefix
                        else
                            fURL = extra + bURL;
                    }

                    //finally navigate to url
                    MainWeb.Navigate(new Uri(fURL));

                    //url textbox shouldn't contain blinking line
                    LoseFocus(sender);
                }
            }
            catch (Exception)
            {   
                //if url cannot be resolved then dignosis
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Cannot_Load_Error.html"));
                StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Folder", CreationCollisionOption.OpenIfExists);
                //we cannot display the original error page, hence copy it to another location
                await file.CopyAsync(folder, "Cannot_Load_Error.html", NameCollisionOption.ReplaceExisting);
                //show the error page
                MainWeb.Navigate(new Uri("ms-appdata:///local/Folder/Cannot_Load_Error.html"));
            }
        }

        //method is called when back button is pressed
        private void Backbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWeb.GoBack();
            }
            catch (Exception)
            {
                Backbtn.IsEnabled = false;
            }
        }

        //mehtod is called when forward button is pressed
        private void Forwrdbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWeb.GoForward();
            }
            catch (Exception)
            {
                Forwardbtn.IsEnabled = false;
            }
        }

        //handles stop-refresh button clicks
        private void Stop_Refreshbtn_Click(object sender, RoutedEventArgs e)
        {
            //if webpage is navigating then stop it
            if (navigating == true)              
                MainWeb.Stop();
            else    //otherwise refresh it
                MainWeb.Refresh();
        }

        //handles home button clicks
        private void Homebtn_Click(object sender, RoutedEventArgs e)
        {
            try//try to navigate to home address
            {
                //gets the homepage address from 'HomePage' Settings
                string HomeURLstring = ((string)AllSettings.Values["HomePage"]);

                if (HomeURLstring != " ")//check if it is null
                {
                    Uri HomeURL = new Uri(HomeURLstring);
                    MainWeb.Navigate(HomeURL);
                }
            }
            catch (Exception)
            {
                //if try fails, probably this is the first time app is launched and hence we need to set up envionment
                AllSettings.Values["HomePage"] = " ";
            }
        }

        //open bookmarks page upon clicking 
        private void Bookmarksbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BookmarksTab));
        }

        //opens history page upon clicking history button
        private void Historybtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HistoryTab));
        }

        //open download page 
        private void Downloadbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DownloadsTab));
        }

        //open settings page
        private void Settingsbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsTab));
        }

        //method to be invoked when url textbox is clicked
        private void URLtb_GotFocus(object sender, RoutedEventArgs e)
        {
            //select all text of url textbox
            URLtb.SelectAll();
        }

        //return focus from control object
        private void LoseFocus(object sender)
        {
            var control = sender as Control;
            var isTabStop = control.IsTabStop;
            control.IsTabStop = false;
            control.IsEnabled = false;
            control.IsEnabled = true;
            control.IsTabStop = isTabStop;
        }
        
       
  
        //show the context menu if right mouse is pressed
        private void MainWeb_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            MainWeb.ContextFlyout.ShowAt(sender as FrameworkElement);
        }

        private void MainWeb_Holding(object sender, HoldingRoutedEventArgs e)
        {
            MainWeb.ContextFlyout.ShowAt(sender as FrameworkElement);
        }

        private async void BookmarkPage_Click(object sender, RoutedEventArgs e)
        {

            var BookMarkThisPage = new ContentDialog()
            {
                Title = "Bookmark current Page"
            };
            var savedBookmarkMessage = new MessageDialog("Bookmark Saved successfully.","Message");
            

            var panel = new StackPanel();

            var titleText = new TextBlock();
            titleText.Text = "Title";

            var UriText = new TextBlock();
            UriText.Text = "URL";

            var titleBox = new TextBox();
            titleBox.Text = MainWeb.DocumentTitle.ToString();

            var UriBox = new TextBox();
            UriBox.Text = MainWeb.Source.AbsoluteUri.ToString();

            panel.Children.Add(titleText);
            panel.Children.Add(titleBox);
            panel.Children.Add(UriText);
            panel.Children.Add(UriBox);
            BookMarkThisPage.Content = panel;

            BookMarkThisPage.PrimaryButtonText = "Save";
            BookMarkThisPage.SecondaryButtonText = "Cancel";

            var resultOfBookmarkDialog = await BookMarkThisPage.ShowAsync();

            if (resultOfBookmarkDialog == ContentDialogResult.Primary)
            {
                WriteBookmarkFile("BookTitle.txt", titleBox.Text);
                WriteBookmarkFile("BookUri.txt", UriBox.Text);
                BookMarkThisPage.Hide();
                await savedBookmarkMessage.ShowAsync();
            }
            else
                BookMarkThisPage.Hide();
        }

        public void OpenNewTab_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Tabsbtn_Click(object sender, RoutedEventArgs e)
        {
            await MainWeb.CapturePreviewToStreamAsync(App.streamsource);
            this.Frame.Navigate(typeof(AllTabsTab));
        }

        private void MainWeb_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            Point initialPoint = e.Position;
        }

        private void MainWeb_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.F5)
            {
                MainWeb.Refresh();
            }
        }



        /*private static async Task<BitmapSource> CreateImageFromStreamAsync(IRandomAccessStream source)
        {
            WriteableBitmap bitmap = new WriteableBitmap(100, 100);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(source);
            BitmapTransform transform = new BitmapTransform();
            transform.ScaledHeight = 100;
            transform.ScaledWidth = 100;

            PixelDataProvider pixelData = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);

            pixelData.DetachPixelData().CopyTo(bitmap.PixelBuffer);
            return bitmap;
        }*/

    }   
}

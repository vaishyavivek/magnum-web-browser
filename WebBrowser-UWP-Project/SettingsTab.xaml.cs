using System;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace WebBrowser_UWP_Project
{

    public sealed partial class SettingsTab : Page
    {
        
        private ApplicationDataContainer AllSettings = ApplicationData.Current.LocalSettings;
        

        public SettingsTab()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += SettingsTab_BackRequested;

            try
            {
                //incognito toggle setting
                if (((string)AllSettings.Values["Incognito"]) == "On")
                    Incognito.IsOn = true;
            }
            catch (Exception) { }


            try//startup page combo box setting
            {
                StartupPageCB.SelectedIndex = ((int)AllSettings.Values["StartupPageSelection"]);
            }
            catch (Exception)
            {
                //in case app is launched for the first time, this will set up the environment
                AllSettings.Values["StartupPageSelection"] = 2;
                StartupPageCB.SelectedIndex = 2;
            }


            try//try to set the homepage according to saved setting
            {    
                SpecificPageTB.Text = ((string)AllSettings.Values["HomePage"]);
            }
            catch (Exception)
            {
                //in case app is launched for the first time, we need to set up the environment
                AllSettings.Values["HomePage"] = " ";
                SpecificPageTB.Text = "Blank Page";
            }

            try//try to set the default search engine
            {
                int SEIndex = (int) AllSettings.Values["SearchEngine"];
                if (SEIndex != 100)//index '100' for no data
                    SearchEngineCB.SelectedIndex = SEIndex;
                    
            }
            catch (Exception)
            {
                //in case app is launched for the first time, we need to set up the environment
                AllSettings.Values["SearchEngine"] = 100;
            }
        }

        private void SettingsTab_BackRequested(object sender, BackRequestedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        //toggle to enable or disable incognito browsing
        private void Incognito_Toggled(object sender, RoutedEventArgs e)
        {
            if (Incognito.IsOn == true)
                AllSettings.Values["Incognito"] = "On";
            else
                AllSettings.Values["Incognito"] = "Off";
        }

        //method handles the startup page combo page selection change event and makes changes to settings
        private async void StartupPageCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = StartupPageCB.SelectedIndex;//get the new index selected
            if (index == 0)
            {
                AllSettings.Values["StartupPageSelection"] = 0;//store new settings
                AllSettings.Values["HomePage"] = "";//make the homepage null
                SpecificPageTB.Text = "Blank Page";//change specificpage textbox
                SpecificPageTB.IsEnabled = false;//then disable it, if was disabled
            }
            else if (index == 1)
            {
                AllSettings.Values["StartupPageSelection"] = 1; //store the new setting first
                SpecificPageTB.Text = "http://";  //then prepare specificpage textbox for getting new homepage url
                SpecificPageTB.IsEnabled = true;//now enable it, if was disabled earlier
            }
            else
            {
                AllSettings.Values["StartupPageSelection"] = 2;
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Startup_Page.html"));
                StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Folder", CreationCollisionOption.OpenIfExists);
                //we cannot have the original startup page, hence copy it to another location
                await file.CopyAsync(folder, "Startup_Page.html", NameCollisionOption.ReplaceExisting);
                AllSettings.Values["HomePage"] = "ms-appdata:///local/Folder/Startup_Page.html";
                SpecificPageTB.Text = "Magnum Start Page";//change contents of specificpage textbox 
                SpecificPageTB.IsEnabled = false;//then disable it
            }
        }

        //event is fired when an attempt to alter the homepage is made through typing in specificpage textbox
        private void SpecificPageTB_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)//change value only if enter key is pressed
            {
                AllSettings.Values["HomePage"] = SpecificPageTB.Text;//save the new homepage url in settings
                LoseFocus(SpecificPageTB);//focus is taken from textbox
            }

        }

        private void LoseFocus(object sender)
        {
            var control = sender as Control;
            var isTabStop = control.IsTabStop;
            control.IsTabStop = false;
            control.IsEnabled = false;
            control.IsEnabled = true;
            control.IsTabStop = isTabStop;
        }

        private void SearchEngineCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = SearchEngineCB.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    AllSettings.Values["SearchEngine"] = 0;
                    break;
                case 1:
                    AllSettings.Values["SearchEngine"] = 1;
                    break;
                case 2:
                    AllSettings.Values["SearchEngine"] = 2;
                    break;
                case 3:
                    AllSettings.Values["SearchEngine"] = 3;
                    break;
                default:
                    AllSettings.Values["SearchEngine"] = 100;
                    break;
            }
        }

        private async void ClearDataBTN_Click(object sender, RoutedEventArgs e)
        {
            var clearData = new ContentDialog()
            {
                Title = "Clear selected content only"
            };

            var panel = new StackPanel();

            var History = new CheckBox();
            History.Content = "Browsing History";
            panel.Children.Add(History);
            History.IsChecked = true;

            var Cookies = new CheckBox();
            Cookies.Content = "Cookies and saved website data";
            panel.Children.Add(Cookies);
            Cookies.IsChecked = true;

            var DownloadHistory = new CheckBox();
            DownloadHistory.Content = "Downloads History";
            panel.Children.Add(DownloadHistory);

            var Passwords = new CheckBox();
            Passwords.Content = "Passwords";
            panel.Children.Add(Passwords);

            clearData.Content = panel;

            clearData.PrimaryButtonText = "Clear";
            clearData.SecondaryButtonText = "Cancel";

            var resultOfClearDialog = await clearData.ShowAsync();

            if (resultOfClearDialog == ContentDialogResult.Primary)
            {
                if(History.IsChecked==true)
                {
                    StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;//open app temp folder
       
                    StorageFile HistoryFile = await tempFolder.CreateFileAsync("bHistoryURL.txt", CreationCollisionOption.OpenIfExists);
                    await HistoryFile.DeleteAsync();

                    HistoryFile = await tempFolder.CreateFileAsync("bHistoryName.txt", CreationCollisionOption.OpenIfExists);
                    await HistoryFile.DeleteAsync();
                }
                if(Cookies.IsChecked==true)
                {

                }
                if(DownloadHistory.IsChecked==true)
                { }
                if(Passwords.IsChecked==true)
                { }
                clearData.Hide();
            }
            else
            {
                clearData.Hide();
            }

        }

        private void blogBtn_Click(object sender, RoutedEventArgs e)
        {
            App.urlFromOtherWindows = "http://mytechstreet.blogspot.in/2017/06/stormtech-magnum-beta-011-released.html";
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}

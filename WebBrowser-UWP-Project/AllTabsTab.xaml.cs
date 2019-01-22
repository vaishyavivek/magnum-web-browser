using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WebBrowser_UWP_Project
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AllTabsTab : Page
    {
        public AllTabsTab()
        {
            this.InitializeComponent();
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += AllTabsTab_BackRequested;

            BitmapImage image = new BitmapImage();
            image.SetSource(App.streamsource);

            Image newimg = new Image();
            newimg.Source = image;

            var stack = new StackPanel();
            stack.Width = 100;
            stack.Height = 100;
            stack.Children.Add(newimg);

            MainPanel.Children.Add(stack);
        }

        private void AllTabsTab_BackRequested(object sender, BackRequestedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void CloseWindowbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void NewTabBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewIncognitoTabBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }
    }
}

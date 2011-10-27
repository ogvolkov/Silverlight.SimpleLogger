using System;
using System.Windows;
using System.Windows.Controls;

using Silverlight.SimpleLogger;

namespace demo
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void GoodButton_Click(object sender, RoutedEventArgs e)
        {            
            SilverlightLogger.Info("Entering good button click with random value={0}", new Random().Next());
        }

        private void BadButton_Click(object sender, RoutedEventArgs e)
        {
            // simulate error
            var x = 0;
            x = x / x;
        }
    }
}

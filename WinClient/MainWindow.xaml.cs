using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Relational.Octapus.WinClient.ViewModels;
using Relational.Octapus.WinClient.Helpers;
using System.Windows.Threading;
using System.ComponentModel;

namespace WinClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ICallbackProvider
    {
        private MainWindowViewModel windowViewModel;
        bool flag = false;

        public MainWindow()
        {
            InitializeComponent();
            windowViewModel = new MainWindowViewModel(this);
            DataContext = windowViewModel;
        }


        public void CallBack(object obj)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {

                windowViewModel.HasLoggerData = true;
                windowViewModel.LoggerData.Add((MainWindowViewModel.LogData)obj);
                Logger_ListBox.ScrollIntoView(obj);
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //Handle(sender as CheckBox);
            flag = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //Handle(sender as CheckBox);
            flag = false;
        }

        void Handle(CheckBox checkBox)
        {
            flag = checkBox.IsChecked.Value;
        }



    }
}

using System.Windows;
using System.ComponentModel;

namespace Relational.Octapus.WinClient.DependencyProperties
{
    public class WindowProperties
    {
        public static readonly DependencyProperty WindowClosingProperty =
           DependencyProperty.RegisterAttached("WindowClosing", typeof(RelayCommand), typeof(WindowProperties), new UIPropertyMetadata(null, WindowClosing));

        public static object GetWindowClosing(DependencyObject depObj)
        {
            return (RelayCommand)depObj.GetValue(WindowClosingProperty);
        }

        public static void SetWindowClosing(DependencyObject depObj, RelayCommand value)
        {
            depObj.SetValue(WindowClosingProperty, value);
        }

        private static void WindowClosing(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var element = (Window)depObj;

            if (element != null)
                element.Closing += OnWindowClosing;

        }

        private static void OnWindowClosing(object sender, CancelEventArgs e)
        {
            RelayCommand command = (RelayCommand)GetWindowClosing((DependencyObject)sender);

            if (command.CanExecute(null))
                command.Execute((Window)sender);
            else
                e.Cancel = true;

            


        }
    }
}

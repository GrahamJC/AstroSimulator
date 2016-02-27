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

namespace AstroSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Construction
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Application.Current;

            // Add event handlers
            Setup.Click += new RoutedEventHandler(Setup_Click);
        }

        // Methods
        public void LogMessage(String format, params Object[] args)
        {
            String message = String.Format(format, args);
            MessageLog.Text += String.Concat(message, "\r\n");
        }

        // Event handlers
        void Setup_Click(object sender, RoutedEventArgs e)
        {
            SetupDialog setup = new SetupDialog();
            setup.ShowDialog();
        }
    }
}

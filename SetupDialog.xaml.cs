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
    /// Interaction logic for SetupDialog.xaml
    /// </summary>
    public partial class SetupDialog : Window
    {
        // Construction
        public SetupDialog()
        {
            InitializeComponent();
            PreferencesGroup.DataContext = ((App)Application.Current).Preferences;
            SiteInformationGroup.DataContext = ((App)Application.Current).SiteInformation;
            TelescopeGroup.DataContext = ((App)Application.Current).Telescope;

            // Add event handlers
            GSCPathBrowse.Click += new RoutedEventHandler(GSCPathBrowse_Click);
            CloseDialog.Click += new RoutedEventHandler(CloseDialog_Click);
        }

        // Methods

        // Event handlers
        void GSCPathBrowse_Click(object sender, RoutedEventArgs e)
        {
            Preferences preferences = ((App)Application.Current).Preferences;
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (!String.IsNullOrEmpty(preferences.GSCPath))
                folderDialog.SelectedPath = preferences.GSCPath;
            else
                folderDialog.SelectedPath = System.IO.Directory.GetCurrentDirectory();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                preferences.GSCPath = folderDialog.SelectedPath;
        }

        void CloseDialog_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Forms = System.Windows.Forms;



namespace kenshi_mod_combiner
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}
		private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new Forms.FolderBrowserDialog())
			{
				dialog.Description = "Select a folder";
				dialog.ShowNewFolderButton = true;
				if (dialog.ShowDialog() == Forms.DialogResult.OK)
				{
					FolderPathTextBox.Text = dialog.SelectedPath;
					FolderContentsListBox.ItemsSource = Directory.GetFileSystemEntries(dialog.SelectedPath);

				}
			}
		}

		private void BrowseFolderButton2_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new Forms.FolderBrowserDialog())
			{
				dialog.Description = "Select a folder";
				dialog.ShowNewFolderButton = true;
				if (dialog.ShowDialog() == Forms.DialogResult.OK)
				{
					FolderPathTextBox2.Text = dialog.SelectedPath;
					FolderContentsListBox2.ItemsSource = Directory.GetFileSystemEntries(dialog.SelectedPath);

				}
			}
		}

		private void CopyModsButton_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}

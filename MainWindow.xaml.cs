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
using Path = System.IO.Path;



namespace kenshi_mod_combiner
{
	public class ModItem
	{
		public string DisplayName { get; set; }
		public string FolderName { get; set; }
		public bool IsChecked { get; set; }
	}
	public partial class MainWindow : Window
	{
		private const string LastKenshiPathFile = "last_kenshi_path.txt";
		private const string LastSteamPathFile = "last_steam_path.txt";

		public MainWindow()
		{
			InitializeComponent();
			LoadLastPaths();
		}

		private void LoadLastPaths()
		{
			string lastKenshi = ReadSavedPath(LastKenshiPathFile);
			if (IsValidFolderPath(lastKenshi))
			{
				FolderPathTextBox.Text = lastKenshi;
				UpdateKenshiModList(lastKenshi);
			}

			string lastSteam = ReadSavedPath(LastSteamPathFile);
			if (IsValidFolderPath(lastSteam))
			{
				FolderPathTextBox2.Text = lastSteam;
				UpdateSteamModList(lastSteam);
			}
		}

		private string ReadSavedPath(string filename)
		{
			try
			{
				if (File.Exists(filename))
					return File.ReadAllText(filename).Trim();
			}
			catch
			{
				// Ignore errors, treat as no saved path
			}
			return "";
		}

		private void SavePath(string filename, string path)
		{
			try
			{
				File.WriteAllText(filename, path);
			}
			catch
			{
				// ignore save errors
			}
		}

		private bool IsValidFolderPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return false;

			if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
				return false;

			return Directory.Exists(path);
		}

		private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new FolderBrowserDialog()
			{
				Description = "Select the Kenshi mods folder",
				ShowNewFolderButton = true
			})
			{
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					FolderPathTextBox.Text = dialog.SelectedPath;
					UpdateKenshiModList(dialog.SelectedPath);
					SavePath(LastKenshiPathFile, dialog.SelectedPath);
				}
			}
		}

		private void BrowseFolderButton2_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new FolderBrowserDialog()
			{
				Description = "Select the Steam Kenshi mods folder",
				ShowNewFolderButton = true
			})
			{
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					FolderPathTextBox2.Text = dialog.SelectedPath;
					UpdateSteamModList(dialog.SelectedPath);
					SavePath(LastSteamPathFile, dialog.SelectedPath);
				}
			}
		}

		// Only list folders that contain at least one *.mod file
		private void UpdateKenshiModList(string folderPath)
		{
			if (!IsValidFolderPath(folderPath))
			{
				FolderContentsListBox.ItemsSource = null;
				return;
			}

			var items = new List<string>();

			foreach (var dir in Directory.GetDirectories(folderPath))
			{
				var modFiles = Directory.GetFiles(dir, "*.mod", SearchOption.TopDirectoryOnly);
				if (modFiles.Length > 0)
					items.Add(Path.GetFileName(dir));
			}

			foreach (var file in Directory.GetFiles(folderPath, "*.mod"))
				items.Add(Path.GetFileNameWithoutExtension(file));

			items.Sort(StringComparer.OrdinalIgnoreCase);
			FolderContentsListBox.ItemsSource = items;
		}

		private void UpdateSteamModList(string folderPath)
		{
			FolderContentsListBox2.ItemsSource = null;

			if (!IsValidFolderPath(folderPath))
				return;

			var existingMods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (FolderContentsListBox.ItemsSource is IEnumerable<string> existing)
			{
				foreach (var item in existing)
					existingMods.Add(item);
			}

			var steamMods = new List<ModItem>();

			foreach (var dir in Directory.GetDirectories(folderPath))
			{
				var modFiles = Directory.GetFiles(dir, "*.mod", SearchOption.TopDirectoryOnly);
				if (modFiles.Length == 0) continue;

				string modName = Path.GetFileNameWithoutExtension(modFiles[0]);
				string label = modName;

				if (existingMods.Contains(modName))
					label += " [exists in mods already]";

				steamMods.Add(new ModItem
				{
					DisplayName = $"{label}  ({Path.GetFileName(dir)})",
					FolderName = Path.GetFileName(dir),
					IsChecked = false
				});
			}

			steamMods.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
			FolderContentsListBox2.ItemsSource = steamMods;
		}


		private void CopyModsButton_Click(object sender, RoutedEventArgs e)
		{
			string steamFolder = FolderPathTextBox2.Text;
			string kenshiFolder = FolderPathTextBox.Text;

			if (!Directory.Exists(steamFolder) || !Directory.Exists(kenshiFolder))
			{
				System.Windows.MessageBox.Show("Both mod folders must be selected first.");
				return;
			}

			// Extract mod items
			var modItems = FolderContentsListBox2.Items.OfType<ModItem>().Where(m => m.IsChecked).ToList();
			if (modItems.Count == 0)
			{
				System.Windows.MessageBox.Show("Inga mods valda! Skärp dig! 😠");
				return;
			}

			foreach (var modItem in modItems)
			{
				string workshopFolder = Path.Combine(steamFolder, modItem.FolderName);

				if (Directory.Exists(workshopFolder))
				{
					CopyDirectorySmart(workshopFolder, kenshiFolder);
				}
			}

			// Refresh views
			UpdateKenshiModList(kenshiFolder);
			UpdateSteamModList(steamFolder);

			System.Windows.MessageBox.Show("Mods copied!");
		}


		// Recursively copy directory from source to dest (overwrite files)
		private void CopyDirectory(string sourceDir, string destinationDir)
		{
			Directory.CreateDirectory(destinationDir);

			foreach (var file in Directory.GetFiles(sourceDir))
			{
				string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
				File.Copy(file, destFile, true);
			}

			foreach (var dir in Directory.GetDirectories(sourceDir))
			{
				string destSubDir = Path.Combine(destinationDir, Path.GetFileName(dir));
				CopyDirectory(dir, destSubDir);
			}
		}

		private void CopyDirectorySmart(string sourceDir, string destinationBase)
		{
			var modFiles = Directory.GetFiles(sourceDir, "*.mod", SearchOption.TopDirectoryOnly);
			if (modFiles.Length == 0)
				return;

			string modFileName = Path.GetFileNameWithoutExtension(modFiles[0]);
			string destinationDir = Path.Combine(destinationBase, modFileName);

			// If destination exists, move to backup folder
			if (Directory.Exists(destinationDir))
			{
				string backupDir = Path.Combine(destinationBase, "_overwritten", $"{modFileName}_{DateTime.Now:yyyyMMdd_HHmmss}");
				Directory.CreateDirectory(Path.GetDirectoryName(backupDir));
				Directory.Move(destinationDir, backupDir);

				// Log what was replaced
				string logPath = Path.Combine(backupDir, "mod_backup_log.txt");
				File.WriteAllText(logPath, $"[{DateTime.Now}] Overwrote mod: {modFileName}\nOriginal backed up from: {destinationDir}\n");
			}

			// Copy the new mod folder into renamed destination
			CopyDirectory(sourceDir, destinationDir);
		}

	}
}
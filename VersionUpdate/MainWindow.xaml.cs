using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VersionUpdate
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

        private void btnPathChoose_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            txtPath.Text = dialog.SelectedPath;
        }

        bool CheckVersionFormat(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return false;
            }

            var regexPattern = @"(\d+)\.(\d{4})\.(\d+)\.(\d+)";
            return Regex.IsMatch(version, regexPattern);
        }

        string GetVersionWithAuto(string version)
        {
            var regexPattern = @".(\d+)$";
            return Regex.Replace(version, regexPattern, ".*");
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var version = txtVersion.Text;
            if (!CheckVersionFormat(version))
            {
                MessageBox.Show("版本號格式不正確, ex: 5.2110.1102.0");
                return;
            }
            var versionWithAuto = GetVersionWithAuto(version);

            var path = txtPath.Text.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("請選擇目錄");
                return;
            }

            try
            {
                var rootDir = new DirectoryInfo(path);
                var files = rootDir.GetFiles("AssemblyInfo.cs", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var assemblyContent = File.ReadAllText(file.FullName);
                    var assemblyVersionPattern = @"(?<!\/\/ )\[assembly: AssemblyVersion\(""(.*)""\)\]";
                    var assemblyFileVersionPattern = @"(?<!\/\/ )\[assembly: AssemblyFileVersion\(""(.*)""\)\]";
                    assemblyContent = Regex.Replace(assemblyContent, assemblyVersionPattern, m => ReplaceGroup(m, versionWithAuto));
                    assemblyContent = Regex.Replace(assemblyContent, assemblyFileVersionPattern, m => ReplaceGroup(m, version));
                    File.WriteAllText(file.FullName, assemblyContent);
                }

                MessageBox.Show($"執行完畢，共 {files.Count()} 檔案被更改!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string ReplaceGroup(Match m, string replacement)
        {
            return m.Value.Replace(m.Groups[1].Value, replacement);
        }
    }
}
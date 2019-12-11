using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;

namespace Nuix_JStack_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ObservableCollection<NuixProcess> nuixProcesses;
        private bool jstacksPending = false;
        private Regex removeNonNumbers = new Regex("[^0-9]+", RegexOptions.Compiled);
        private DispatcherTimer jstackGenerationTimer;

        public MainWindow()
        {
            InitializeComponent();
            nuixProcesses = new ObservableCollection<NuixProcess>();
            listNuixProcesses.ItemsSource = nuixProcesses;
            refreshProcessList();

            var processListUpdater = new DispatcherTimer();
            processListUpdater.Interval = TimeSpan.FromSeconds(2);
            processListUpdater.Tick += (a, b) =>
            {
                refreshProcessList();
            };
            processListUpdater.Start();

            jstackGenerationTimer = new DispatcherTimer();
            jstackGenerationTimer.Interval = TimeSpan.FromSeconds(5);
            jstackGenerationTimer.Tick += (z, x) =>
            {
                jstackAllSelected();
            };
        }

        private void refreshProcessList()
        {
            var procs = NuixProcess.GetRunningNuixProcesses();
            lock (nuixProcesses)
            {
                for (int i = 0; i < procs.Count; i++)
                {
                    if (!nuixProcesses.Any(p => p.ProcessId == procs[i].ProcessId))
                    {
                        nuixProcesses.Add(procs[i]);
                    }
                }

                for (int i = nuixProcesses.Count - 1; i >= 0; i--)
                {
                    if (nuixProcesses[i].HasExited)
                        nuixProcesses.RemoveAt(i);
                }
            }
        }

        public void log(string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtLog.AppendText(message + "\n");
                txtLog.ScrollToEnd();
            }));
        }

        public void jstackAllSelected()
        {
            if (jstacksPending)
                return;

            string outputDirectory = txtOutputDirectory.Text;

            Thread t = new Thread(() =>
            {
                lock (nuixProcesses)
                {
                    foreach (var process in nuixProcesses.Where(p => p.IsToBeMonitored))
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            log(string.Format("[{1}] : Creating JStack for {0}", process.Details, DateTime.Now));
                        }));
                        jstacksPending = true;
                        try
                        {
                            process.CreateJStack(outputDirectory);
                        }
                        catch (Exception exc)
                        {
                            log(string.Format("[{1}] : Error creating JStack for {0}", process.Details, DateTime.Now));
                            log(exc.ToString());
                        }
                        jstacksPending = false;
                    }
                }
            });
            t.Start();
        }

        private void btnJStackNow_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = txtOutputDirectory.Text;
            try
            {
                if (!System.IO.Directory.Exists(outputDirectory))
                {
                    System.IO.Directory.CreateDirectory(outputDirectory);
                }
            }
            catch
            {
                MessageBox.Show("Unable to create output directory: " + outputDirectory);
                return;
            }
            jstackAllSelected();
        }

        private void beginJstacking_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = txtOutputDirectory.Text;
            try
            {
                if (!System.IO.Directory.Exists(outputDirectory))
                {
                    System.IO.Directory.CreateDirectory(outputDirectory);
                }
            }
            catch
            {
                MessageBox.Show("Unable to create output directory: " + outputDirectory);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtInterval.Text))
                txtInterval.Text = "5";
            jstackGenerationTimer.Interval = TimeSpan.FromSeconds(int.Parse(txtInterval.Text));
            jstackGenerationTimer.Start();
            txtInterval.IsEnabled = false;
            btnJStackNow.IsEnabled = false;
            beginJstacking.IsEnabled = false;
            stopJstacking.IsEnabled = true;
            btnSelectOutputDirectory.IsEnabled = false;
            txtOutputDirectory.IsEnabled = false;
        }

        private void stopJstacking_Click(object sender, RoutedEventArgs e)
        {
            jstackGenerationTimer.Stop();
            txtInterval.IsEnabled = true;
            btnJStackNow.IsEnabled = true;
            beginJstacking.IsEnabled = true;
            stopJstacking.IsEnabled = false;
            btnSelectOutputDirectory.IsEnabled = true;
            txtOutputDirectory.IsEnabled = true;
        }

        private void btnSelectOutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = txtOutputDirectory.Text;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtOutputDirectory.Text = fbd.SelectedPath;
            }
        }

        private void txtInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (removeNonNumbers.IsMatch(txtInterval.Text))
                txtInterval.Text = removeNonNumbers.Replace(txtInterval.Text, "");
        }

        private void txtInterval_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isLetter = e.Key >= Key.A && e.Key <= Key.Z;
            if (isLetter)
            {
                e.Handled = true;
            }
        }

    }
}

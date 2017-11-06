using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace FileSearch {
    class ListingMonitor {
        public bool stayAlive { get; set; }
        public DirectoryListing directoryListing { get; set; }
        public void run() {
            Window window = new Window();
            window.Width = 500;
            window.Height = 100;
            window.Show();
            while(stayAlive) {
                window.Title = "Dir cnt " + directoryListing.directoryCount + " -  file cnt " + directoryListing.fileCount;
                Thread.Sleep(750);
            }
        }
    }
    class DirectoryListing {
        //public TextBox filesFound { get; set; }
        public StringBuilder filesFound { get; set; }
        public string searchForFile { get; set; }
        public string searchForText { get; set; }
        public string searchInPath { get; set; }
        public int directoryCount { get; set; }
        public int fileCount { get; set; }
        private int privInt {  get;  set;}
        protected int protInt { get; private set; }
        public int pubInt { get; protected set; }
        public int pubIntII { get; private set; }
        int vedIkkeInt { get; set; } //private

        Regex filenameRegex;
        Regex contentRegex;

        public void run() {
            filesFound.Clear();
            directoryCount = 0;
            fileCount = 0;
            if(searchForFile.Length>0)
                filenameRegex = new Regex(searchForFile, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if(searchForText.Length>0)
                contentRegex = new Regex(searchForText, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            listDirectoriesInDirectory((searchInPath.Length > 0)?searchInPath:"C:\\");
            //listDirectoriesInDirectory((searchInPath.Length > 0) ? searchInPath : "Z:\\");
        }
        private void listDirectoriesInDirectory(string path) {
            directoryCount++;
            try {
                foreach(string s in Directory.GetDirectories(path)) {
                    //if(s.Contains ("adm"))
                        listDirectoriesInDirectory(s);
                }

                if(searchForFile.Length == 0)
                    listAllFilesInDirectory(path);
                else
                    listFilesInDirectory(path);

            } catch(System.UnauthorizedAccessException) { }
            catch(System.IO.PathTooLongException) { }
        }
        private void appendText(string text) {
            if(filesFound.Length > 0)
                filesFound.AppendLine("");
            filesFound.Append(text);
        }
        private void addDirectory(string path) {
            appendText(path);
        }
        private void listAllFilesInDirectory(string path) {
            foreach(string s in Directory.GetFiles(path)) {
                fileCount++;
                if(contentMatch(s))
                    appendText(s);
            }
        }
        private void listFilesInDirectory(string path) {
            foreach(string s in Directory.GetFiles(path)) {
                fileCount++;
                if(filenameRegex.IsMatch(s) && contentMatch(s))
                    appendText(s);
            }
        }

        private bool contentMatch(string pathAndFilename) {
            if(searchForText.Length == 0)
                return true;
            string line;
            StreamReader file = new StreamReader(pathAndFilename);
            while((line = file.ReadLine()) != null) {
                if(contentRegex.IsMatch(line)) {
                    file.Close();
                    return true;
                }
            }
            return false;
        }
    }


    class xxx:  DirectoryListing {
        public void metode () {
            int i=protInt;
           
        }
    }

    public partial class MainWindow : Window {
        DirectoryListing directoryListing = new DirectoryListing();
        public MainWindow() {
            InitializeComponent();
        }

        //  https://www.codeproject.com/Articles/21248/A-Simple-WPF-Explorer-Tree
        Object dummyNode = new Object();

        private void folders_Loaded(object sender, RoutedEventArgs e) {
            foreach(string s in Directory.GetLogicalDrives()) {
                TreeViewItem item = new TreeViewItem();
                item.Header = s;
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                folders.Items.Add(item);
            }
        }
        void folder_Expanded(object sender, RoutedEventArgs e) {
            TreeViewItem item = (TreeViewItem)sender;
            if(item.Items.Count == 1 && item.Items[0] == dummyNode) {
                item.Items.Clear();
                try {
                    foreach(string s in Directory.GetDirectories(item.Tag.ToString())) {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                } catch(Exception) { }
            }
        }
        private void button_Click(object sender, RoutedEventArgs e) {
            filesFound.Clear();
            StringBuilder filesFoundStringBuilder = new StringBuilder();
            directoryListing.filesFound = filesFoundStringBuilder;
            directoryListing.searchForFile = this.searchForFile.Text;
            directoryListing.searchForText = this.searchForText.Text;
            try { 
                directoryListing.searchInPath = ((TreeViewItem)folders.SelectedItem).Tag.ToString();
            } catch(Exception) {
                directoryListing.searchInPath = string.Empty;
            }

            Thread directoryLister = new Thread(new ThreadStart(directoryListing.run));
            directoryLister.Start();

            ListingMonitor listingMonitor = new ListingMonitor();
            Thread listingMonitorThread = new Thread(new ThreadStart(listingMonitor.run));
            listingMonitorThread.SetApartmentState(ApartmentState.STA);
            listingMonitor.stayAlive = true;
            listingMonitor.directoryListing = directoryListing;
            listingMonitorThread.Start();

            directoryLister.Join();
            //listingMonitorThread.Abort();
            listingMonitor.stayAlive = false;

            filesFound.AppendText(directoryListing.filesFound.ToString());
        }

        /*
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);*/
        private void gridSplitter_MouseMove(object sender, MouseEventArgs e) {
            if(e.RightButton==MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed) {
                //gridSplitter.Margin = new Thickness(mouseX, 10, 0, 10);
              
            }
            //SetCursorPos(10, 10);
        }
        private void gridSplitter_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            gridSplitter.Margin = new Thickness(e.GetPosition(this).X, 10, 0, 10);
            filesFound.Width = gridSplitter.Margin.Left - 9;
            Thickness margin = folders.Margin;
            margin.Left = gridSplitter.Margin.Left + 14;
            folders.Margin = margin;
            folders.Width = MyWindow.Width - margin.Left - 20;
        }
    }
}

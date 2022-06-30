using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace Kate_s_DLL_Injector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    using System.Runtime.InteropServices;

    public partial class MainWindow : Window
    {
        string DLLP;
        static string processp;
        public MainWindow()
        {
            InitializeComponent();
            Process[] processCollection = Process.GetProcesses().Where(p => (long)p.MainWindowHandle != 0).ToArray();
            foreach (Process p in processCollection)
            {
                comboBox.Items.Add(p.ProcessName);
            }
        }

        #region UI Behaviour

        private void Twitter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://twitter.com/ardiiumx");
        }

        private void Discord_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://discord.com/invite/2yqk4twhs9");
        }

        private void Twitter_MouseEnter(object sender, MouseEventArgs e)
        {
            Twitter.Foreground = Brushes.White;
        }

        private void Discord_MouseEnter(object sender, MouseEventArgs e)
        {
            Discord.Foreground = Brushes.White;
        }

        private void Discord_MouseLeave(object sender, MouseEventArgs e)
        {
            Discord.Foreground = (Brush)FindResource("DefaultColor");
        }

        private void Twitter_MouseLeave(object sender, MouseEventArgs e)
        {
            Twitter.Foreground = (Brush)FindResource("DefaultColor");

        }
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            processp = (string)comboBox.SelectedItem;
        }

        private void Refresh_Processes_Click(object sender, RoutedEventArgs e)
        {
            comboBox.Items.Clear();
            Process[] processCollection2 = Process.GetProcesses().Where(p => (long)p.MainWindowHandle != 0).ToArray();
            foreach (Process p2 in processCollection2)
            {
                comboBox.Items.Add(p2.ProcessName);
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void minButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        #endregion

        private void InjectDLL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBox.SelectedItem == null)
                {
                    MessageBox.Show("No process is selected!");
                }
                else
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Title = "Select DLL to inject";
                    ofd.DefaultExt = "dll";
                    ofd.Filter = "DLL (*.dll)|*.dll";
                    ofd.CheckFileExists = true;
                    ofd.CheckPathExists = true;
                    ofd.ShowDialog();
                    DLLP = ofd.FileName;
                    InjectDLLFunc(processp, DLLP);
                }
            }
            catch
            {
                return;
            }

        }

        #region Functions, DLL Imports, extras
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        public enum VirtualMemoryProtection
        {
            PAGE_NOACCESS = 1,
            PAGE_READONLY = 2,
            PAGE_READWRITE = 4,
            PAGE_WRITECOPY = 8,
            PAGE_EXECUTE = 0x010,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, VirtualMemoryProtection flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);


        public bool InjectDLLFunc(string processp, string dllPath)
        {
            try
            {
                const int PROCESS_CREATE_THREAD = 0x0002;
                const int PROCESS_QUERY_INFORMATION = 0x0400;
                const int PROCESS_VM_OPERATION = 0x0008;
                const int PROCESS_VM_WRITE = 0x0020;
                const int PROCESS_VM_READ = 0x0010;

                Process targetProcess = Process.GetProcessesByName(processp)[0];

                IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);

                // finding LoadLibraryA and storing it as a pointer
                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                // allocating enough memory for the DLL's name
                IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), AllocationType.Commit | AllocationType.Reserve, VirtualMemoryProtection.PAGE_READWRITE);

                int bytesWritten = 0;
                WriteProcessMemory((IntPtr)procHandle, allocMemAddress, Encoding.Default.GetBytes(dllPath), (int)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), ref bytesWritten);

                // call LoadLibraryA
                CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}

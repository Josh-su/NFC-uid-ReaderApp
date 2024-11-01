using Hardcodet.Wpf.TaskbarNotification;
using System.ComponentModel;
using System.Windows;
using PCSC;
using PCSC.Iso7816;
using System.IO;
using System.Windows;

namespace NFCReaderApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker worker;
        private string lastUID = string.Empty;
        private string currentUID = string.Empty;
        private SCardContext context;

        public MainWindow()
        {
            InitializeComponent();

            // Set the window position to bottom right
            this.Left = SystemParameters.WorkArea.Width - this.Width;
            this.Top = SystemParameters.WorkArea.Height - this.Height;

            // Access the TaskbarIcon defined in XAML
            var notifyIcon = (TaskbarIcon)FindResource("MyNotifyIcon");

            // Start monitoring for card reader and card UID
            StartMonitoring();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                StopMonitoring();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            StopMonitoring();
        }

        private void ShowWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            StartMonitoring();
        }

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            StopMonitoring();
            Application.Current.Shutdown();
        }

        private void StartMonitoring()
        {
            // Initialize the card reader context
            context = new SCardContext();
            context.Establish(SCardScope.System); // You can use User or System scope depending on your need

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true; // Enable cancellation support
            worker.DoWork += MonitorReaderAndCard;
            worker.RunWorkerAsync();
        }

        private void StopMonitoring()
        {
            if (worker != null && worker.IsBusy)
            {
                worker.CancelAsync(); // Request cancellation of the worker
                worker.DoWork -= MonitorReaderAndCard; // Unsubscribe from the event
                worker = null; // Clean up the worker
            }
        }

        private async void MonitorReaderAndCard(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = sender as BackgroundWorker;

            while (!bgWorker.CancellationPending) // Check for cancellation request
            {
                // Asynchronously wait for the UID
                string detectedUID = await CheckForCardUIDAsync(); // Only check once per loop iteration

                if (!string.IsNullOrEmpty(detectedUID))
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Only update lastUID if detectedUID is new
                        if (detectedUID != currentUID)
                        {
                            if (!string.IsNullOrEmpty(currentUID))
                            {
                                txtLastUID.Text = currentUID; // Set lastUID to previous currentUID
                            }

                            txtCurrentUID.Text = detectedUID; // Update current UID
                            currentUID = detectedUID; // Save current UID
                        }
                    });
                }

                await Task.Delay(100); // Reduced polling interval
            }

            e.Cancel = true;
        }


        private async Task<string> CheckForCardUIDAsync()
        {
            try
            {
                using (var reader = new SCardReader(context))
                {
                    var readerNames = context.GetReaders();
                    if (readerNames.Length == 0)
                    {
                        return ""; // No readers available
                    }

                    var readerName = readerNames[0];
                    var sc = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);

                    if (sc == SCardError.Success)
                    {
                        var apdu = new CommandApdu(IsoCase.Case2Short, reader.ActiveProtocol)
                        {
                            CLA = 0xFF,
                            Instruction = InstructionCode.GetData,
                            P1 = 0x00,
                            P2 = 0x00,
                            Le = 0x07 // Expected UID length
                        };

                        var receivePci = new SCardPCI();
                        var sendPci = SCardPCI.GetPci(reader.ActiveProtocol);

                        var receiveBuffer = new byte[256];
                        var command = apdu.ToArray();

                        sc = reader.Transmit(sendPci, command, receivePci, ref receiveBuffer);

                        if (sc == SCardError.Success)
                        {
                            var uid = BitConverter.ToString(receiveBuffer.Take(7).ToArray()).Replace("-", "");
                            if (uid != "6300" && uid != lastUID)
                            {
                                return uid; // Return the detected UID immediately
                            }
                        }

                        reader.Disconnect(SCardReaderDisposition.Leave);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions (e.g., smart card reader not connected)
                Console.WriteLine($"Error: {ex.Message}");
            }

            return ""; // Return empty if no card is detected or an error occurs
        }

        private void CopyLastUID_Click(object sender, RoutedEventArgs e)
        {
            // Copy the current UID to clipboard
            Clipboard.SetText(txtLastUID.Text);
        }

        private void CopyCurrentUID_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtCurrentUID.Text);
        }
    }
}
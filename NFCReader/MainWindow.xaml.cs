using PCSC;
using PCSC.Iso7816;
using System.IO;
using System.Windows;

namespace NFCReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string lastUID = null;
        private SCardContext context;
        private string filePath; 
        private string? lastLogMessage;

        public MainWindow()
        {
            InitializeComponent();
            SetupNFCReader();
            LoadLastFile();
        }

        private void SetupNFCReader()
        {
            context = new SCardContext();
            context.Establish(SCardScope.System);
            StartCardReader();
        }

        private void LoadLastFile()
        {
            string directoryPath = Path.Combine(Path.GetTempPath(), "tsvFiles");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Load the latest TSV file
            var lastFile = Directory.GetFiles(directoryPath, "*.tsv")
                                    .OrderByDescending(f => f)
                                    .FirstOrDefault();

            if (lastFile == null)
            {
                CreateNewTsvFile();
            }
            else
            {
                filePath = lastFile;
                lblCurrentFile.Content = $"Current TSV File: {Path.GetFileName(filePath)}";
            }
        }

        private void CreateNewTsvFile()
        {
            string directoryPath = Path.Combine(Path.GetTempPath(), "tsvFiles");
            filePath = Path.Combine(directoryPath, $"{DateTime.Now:yyyyMMddHHmmss}_uids.tsv");

            using (File.Create(filePath)) { }

            lblCurrentFile.Content = $"Current TSV File: {Path.GetFileName(filePath)}";
        }

        private async void StartCardReader()
        {
            while (true) // Continuously listen for cards
            {
                using (var reader = new SCardReader(context))
                {
                    var readerNames = context.GetReaders();
                    if (readerNames.Length == 0)
                    {
                        LogMessage("No card reader found.");
                        return;
                    }
                    var readerName = readerNames[0];

                    var sc = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);

                    if (sc == SCardError.Success)
                    {
                        LogMessage("Card connected.");

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
                                txtUID.Text = uid;
                                lastUID = uid;
                                txtUserName.IsEnabled = true;
                                btnSaveUID.IsEnabled = true;
                                UpdateLogs($"Card UID: {uid}");
                            }
                        }

                        reader.Disconnect(SCardReaderDisposition.Leave);
                    }
                    else
                    {
                        LogMessage("No card to read.");
                    }
                }
                await Task.Delay(1000); // Polling delay
            }
        }

        private void btnSaveUID_Click(object sender, RoutedEventArgs e)
        {
            var uid = txtUID.Text;
            var username = txtUserName.Text;

            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please scan a card and enter a username.");
                return;
            }

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"{username}\t\t\t\t\t\t\t{uid}");
            }

            MessageBox.Show("UID saved successfully.");
            txtUID.Clear();
            txtUserName.Clear();
            txtUserName.IsEnabled = false;
            btnSaveUID.IsEnabled = false;
            UpdateLogs($"UID {uid} saved with username {username}");
        }

        private void LogMessage(string message)
        {
            if (message != lastLogMessage)
            {
                txtlogs.Text += message + Environment.NewLine;
                txtlogs.ScrollToEnd();  // Scroll to the latest log entry
                lastLogMessage = message;
            }
        }

        private void UpdateLogs(string message)
        {
            txtlogs.AppendText($"{DateTime.Now}: {message}\n");
            txtlogs.ScrollToEnd();
        }

        private void btnCreateNewTsv_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTsvFile();
            UpdateLogs("New TSV file created.");

        }

        private void btnOpenTsvDirectory_Click(object sender, RoutedEventArgs e)
        {
            string tsvDirectoryPath = Path.Combine(Path.GetTempPath(), "tsvFiles");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = tsvDirectoryPath,
                UseShellExecute = true
            });
        }
    }
}
using System;
using System.IO;
using System.Linq;
using PCSC;
using PCSC.Iso7816;

class Program
{
    static void Main(string[] args)
    {
        using (var context = new SCardContext())
        {
            Console.WriteLine("\n███╗   ██╗███████╗ ██████╗    ██████╗ ███████╗ █████╗ ██████╗ ███████╗██████╗      █████╗ ██████╗ ██████╗ \r\n████╗  ██║██╔════╝██╔════╝    ██╔══██╗██╔════╝██╔══██╗██╔══██╗██╔════╝██╔══██╗    ██╔══██╗██╔══██╗██╔══██╗\r\n██╔██╗ ██║█████╗  ██║         ██████╔╝█████╗  ███████║██║  ██║█████╗  ██████╔╝    ███████║██████╔╝██████╔╝\r\n██║╚██╗██║██╔══╝  ██║         ██╔══██╗██╔══╝  ██╔══██║██║  ██║██╔══╝  ██╔══██╗    ██╔══██║██╔═══╝ ██╔═══╝ \r\n██║ ╚████║██║     ╚██████╗    ██║  ██║███████╗██║  ██║██████╔╝███████╗██║  ██║    ██║  ██║██║     ██║     \r\n╚═╝  ╚═══╝╚═╝      ╚═════╝    ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═════╝ ╚══════╝╚═╝  ╚═╝    ╚═╝  ╚═╝╚═╝     ╚═╝     \r\n                                                                                                          ");
            Console.WriteLine("1. Connect the card reader to your computer\n2. Read the card with the card reader(not your mouth)\n3. Write the name that go with the uid card\n4. Do it all over again until you run out of cards\n5. And at the end get the csv file\n\n");

            context.Establish(SCardScope.System);

            var readerNames = context.GetReaders();

            if (readerNames.Length == 0)
            {
                Console.WriteLine("                                       ___  _    ____   /  /  /                                            \r\n                                       |__] |    [__   /  /  /                                             \r\n                                       |    |___ ___] .  .  .                                              \r\n                                                                                                           \r\n____ ____ _  _ _  _ ____ ____ ___    ___ _  _ ____    ____ ____ ____ ___     ____ ____ ____ ___  ____ ____ \r\n|    |  | |\\ | |\\ | |___ |     |      |  |__| |___    |    |__| |__/ |  \\    |__/ |___ |__| |  \\ |___ |__/ \r\n|___ |__| | \\| | \\| |___ |___  |      |  |  | |___    |___ |  | |  \\ |__/    |  \\ |___ |  | |__/ |___ |  \\ \r\n                                                                                                           \r\n____ _  _ ___     ___ _  _ ____ _  _    _    ____ _  _ _  _ ____ _  _    ___ _  _ ____    ____ ___  ___    \r\n|__| |\\ | |  \\     |  |__| |___ |\\ |    |    |__| |  | |\\ | |    |__|     |  |__| |___    |__| |__] |__]   \r\n|  | | \\| |__/     |  |  | |___ | \\|    |___ |  | |__| | \\| |___ |  |     |  |  | |___    |  | |    |      \r\n                                                                                                           ");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            var readerName = readerNames[0]; // Use the first reader, you can modify this if needed

            string lastUID = null;
            bool errorDisplayed = false; // Flag to track whether the error message has been displayed

            while (true) // Continuously listen for cards
            {
                using (var reader = new SCardReader(context))
                {
                    var sc = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);

                    if (sc == SCardError.Success)
                    {
                        if (errorDisplayed)
                        {
                            Console.WriteLine("Connected to the reader.");
                            errorDisplayed = false; // Reset the flag
                        }

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

                            if (uid != lastUID && uid != "6300")
                            {
                                Console.WriteLine($"UID: {uid}");

                                Console.Write("Enter a name for this card: ");
                                string name = Console.ReadLine();

                                using (StreamWriter writer = new("uids.csv", true))
                                {
                                    writer.WriteLine($"{name}, {uid}");
                                }

                                lastUID = uid;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Error: {sc}");
                        }

                        reader.Disconnect(SCardReaderDisposition.Leave);
                    }
                    else
                    {
                        if (!errorDisplayed)
                        {
                            Console.WriteLine($"Failed to connect to the reader: {sc}");
                            errorDisplayed = true; // Set the flag
                        }
                    }
                }
            }
        }
    }
}

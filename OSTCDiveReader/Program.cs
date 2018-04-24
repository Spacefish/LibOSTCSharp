using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSTCDiveReader
{
    public static class OSTCCommands
    {
        public static byte START_COMMUNICATION = 0xBB;
        public static byte SEND_HEADERS = 0x61;
    }

    public static class OSTCReplys
    {
        public static byte READY_FOR_COMMAND = 0x4D;
    }
    class Program
    {
        static void Main(string[] args)
        {
            FileStream fs = File.OpenRead("dive_headers.bin");
            OSTCBinaryReader br = new OSTCBinaryReader(fs);
            List<OSTCDiveHeader> headers = new List<OSTCDiveHeader>();
            while (true)
            {
                OSTCDiveHeader header = br.ReadDiveHeader();
                if (header != null)
                    headers.Add(header);
                else
                    break;
            }
            Console.ReadKey();
            return;
            SerialPort sp = new SerialPort("COM3");
            sp.BaudRate = 115200;
            sp.ReadTimeout = 3000;
            sp.Open();

            try
            {
                WriteByte(sp, 0xBB);

                if (!(sp.ReadByte() == OSTCCommands.START_COMMUNICATION))
                    throw new InvalidDataException("Expected 0xBB");
                if (!(sp.ReadByte() == OSTCReplys.READY_FOR_COMMAND))
                    throw new InvalidDataException("Expected 0x4D");

                WriteByte(sp, OSTCCommands.SEND_HEADERS);

                if (!(sp.ReadByte() == OSTCCommands.SEND_HEADERS))
                    throw new InvalidDataException("Expected " + OSTCCommands.SEND_HEADERS.ToString("X2"));

                byte[] diveHeadersBuffer = new byte[65536];

                int bytesToRead = 65536;
                do
                {
                    int bytesRead = sp.Read(diveHeadersBuffer, 65536 - bytesToRead, bytesToRead);
                    bytesToRead -= bytesRead;
                    Console.WriteLine("got " + bytesRead.ToString() + " bytes " + bytesToRead.ToString() + " remaining!");
                }
                while (bytesToRead > 0);

                File.WriteAllBytes("dive_headers.bin", diveHeadersBuffer);

                if (!(sp.ReadByte() == OSTCReplys.READY_FOR_COMMAND))
                    throw new InvalidDataException("Expected 0x4D");
            }
            catch(Exception ex)
            {
                try
                {
                    WriteByte(sp, 0xFF);
                }
                catch(Exception eoce)
                {
                    Console.WriteLine("Exception while sending END OF COM" + eoce.ToString());
                }
                Console.WriteLine("Got exceptioN!: " + ex.ToString());
                sp.Close();
            }

            Console.WriteLine("END!");
            Console.ReadLine();
        }

        private static void WriteByte(SerialPort sp, byte data)
        {
            sp.Write(new byte[] { data }, 0, 1);
        }
    }
}

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
        public static byte GET_HARDWARE_FEATURES = 0x60;
        public static byte GET_HEADERS_FULL = 0x61;
        public static byte GET_HEADERS_COMPACT = 0x6D;
        public static byte SET_TIME_AND_DATE = 0x62;
        public static byte SET_CUSTOM_TEXT = 0x63;
        public static byte GET_DIVE_PROFILE = 0x66;
        public static byte GET_VERSION_AND_IDENTITY = 0x69;
        public static byte DISPLAY_TEXT = 0x6E;
        public static byte GET_HARDWARE_DESCRIPTOR = 0x6A;
        public static byte RESET_ALL_SETTINGS = 0x78;
        public static byte SET_SETTING = 0x77;
        public static byte GET_SETTING = 0x72;
        public static byte CLOSE_COMMUNICATION = 0xFF;
    }

    public static class OSTCReplys
    {
        public static byte READY_FOR_COMMAND = 0x4D;
    }
    class Program
    {
        private static SerialPort sp;

        static void Main(string[] args)
        {
            FileStream fs = File.OpenRead("dive_headers.bin");
            OSTCBinaryReader br = new OSTCBinaryReader(fs);
            List<OSTCDiveHeader> headers = new List<OSTCDiveHeader>();
            byte tocIndex = 0;
            while (true)
            {
                OSTCDiveHeader header = br.ReadDiveHeader(tocIndex++);
                if (header != null)
                    headers.Add(header);
                else
                    break;
            }

            Console.ReadKey();
            return;

            sp = new SerialPort("COM3");
            sp.BaudRate = 115200;
            sp.ReadTimeout = 3000;
            sp.Open();

            try
            {
                WriteByte(sp, OSTCCommands.START_COMMUNICATION);

                if (!(sp.ReadByte() == OSTCCommands.START_COMMUNICATION))
                    throw new InvalidDataException("Expected 0xBB");
                if (!(sp.ReadByte() == OSTCReplys.READY_FOR_COMMAND))
                    throw new InvalidDataException("Expected 0x4D");

                GetFullHeaders();

            }
            catch(Exception ex)
            {
                Console.WriteLine("Got exceptioN!: " + ex.ToString());
            }
            finally
            {
                try
                {
                    WriteByte(sp, OSTCCommands.CLOSE_COMMUNICATION);
                }
                catch (Exception eoce)
                {
                    Console.WriteLine("Exception while sending END OF COM" + eoce.ToString());
                }

                sp.Close();
            }

            Console.WriteLine("END!");
            Console.ReadLine();
        }

        private static void GetDiveProfile(byte profileNumber)
        {

        }

        private static void GetFullHeaders()
        {
            WriteByte(sp, OSTCCommands.GET_HEADERS_FULL);

            if (!(sp.ReadByte() == OSTCCommands.GET_HEADERS_FULL))
                throw new InvalidDataException("Expected " + OSTCCommands.GET_HEADERS_FULL.ToString("X2"));

            byte[] diveHeadersBuffer = new byte[65536];

            int bytesToRead = 65536;
            do
            {
                int bytesRead = sp.Read(diveHeadersBuffer, 65536 - bytesToRead, bytesToRead);
                bytesToRead -= bytesRead;
                Console.WriteLine("got " + bytesRead.ToString() + " bytes " + bytesToRead.ToString() + " remaining!");
            }
            while (bytesToRead > 0);

            if (!(sp.ReadByte() == OSTCReplys.READY_FOR_COMMAND))
                throw new InvalidDataException("Expected 0x4D");

            File.WriteAllBytes("dive_headers.bin", diveHeadersBuffer);
        }

        private static void WriteByte(SerialPort sp, byte data)
        {
            sp.Write(new byte[] { data }, 0, 1);
        }
    }
}

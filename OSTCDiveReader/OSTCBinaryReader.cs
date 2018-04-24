using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSTCDiveReader
{
    public class OSTCBinaryReader : BinaryReader
    {
        public OSTCBinaryReader(Stream input) : base(input)
        {
        }

        public OSTCDiveHeader ReadDiveHeader()
        {
            return OSTCDiveHeader.FromBinary(this);
        }

        /// <summary>
        /// reads a 24bit Address
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt24()
        {
            uint low = this.ReadByte();
            uint high = this.ReadByte();
            uint upper = this.ReadByte();
            return low + (high << 8) + (upper << 16);
        }

        internal DateTime ReadDate()
        {
            int year = this.ReadByte()+2000;
            int month = this.ReadByte();
            int day = this.ReadByte();
            int hour = this.ReadByte();
            int minute = this.ReadByte();

            return new DateTime(year, month, day, hour, minute, 0);
        }

        internal TimeSpan ReadTimeSpan24()
        {
            var ts = TimeSpan.FromMinutes(ReadUInt16());
            ts.Add(TimeSpan.FromSeconds(this.ReadByte()));
            return ts;
        }

        internal TimeSpan ReadTimeSpan16()
        {
            return TimeSpan.FromMinutes(this.ReadUInt16());
        }

        internal OSTCGasInfo ReadGas32()
        {
            return OSTCGasInfo.FromBinary(this);
        }

        internal OSTCSetpointInfo ReadSetpoint16()
        {
            return OSTCSetpointInfo.FromBinary(this);
        }

        internal Version ReadFWVersion()
        {
            return new Version(this.ReadByte(), this.ReadByte());
        }
    }

    public class OSTCSetpointInfo
    {
        internal static OSTCSetpointInfo FromBinary(OSTCBinaryReader br)
        {
            br.ReadUInt16();
            // TODO
            return new OSTCSetpointInfo
            {

            };
        }
    }

    public class OSTCGasInfo
    {
        internal static OSTCGasInfo FromBinary(OSTCBinaryReader br)
        {
            // GAS info is 4 byte long
            br.ReadInt32();

            return new OSTCGasInfo
            {

            };
        }
    }
}

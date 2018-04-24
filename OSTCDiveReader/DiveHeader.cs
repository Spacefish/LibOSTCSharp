using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSTCDiveReader
{
    public class DiveHeader
    {
        internal static DiveHeader FromBinary(OSTCBinaryReader br)
        {
            if (br.ReadByte() != 0xFA || br.ReadByte() != 0xFA)
                throw new InvalidDataException("Expected 0xFA as header start but got something else!");

            return new DiveHeader
            {
                DataStartAddress = br.ReadUInt24(),
                DataStopAddress = br.ReadUInt24(),
                ProfileVersion = (LogbookProfileVersion)br.ReadByte(),
                DataLength = br.ReadUInt24(),
                Date = br.ReadDate(),
                MaxPressureMbar = br.ReadUInt16(),
                DiveTime = br.ReadTimeSpan24(),
                MinWaterTemperature = br.ReadInt16() / 10.0,
                SurfacePressureMBar = br.ReadUInt16(),
                DesaturationTime = br.ReadTimeSpan16(),
                Gases = new List<OSTCGasInfo>(new OSTCGasInfo[] {
                    br.ReadGas32(), // GAS1
                    br.ReadGas32(), // GAS2
                    br.ReadGas32(), // GAS3
                    br.ReadGas32(), // GAS4
                    br.ReadGas32()  // GAS5
                })
            };
        }

        public uint DataStartAddress { get; private set; }
        public uint DataStopAddress { get; private set; }
        public LogbookProfileVersion ProfileVersion { get; private set; }
        public uint DataLength { get; private set; }
        public DateTime Date { get; private set; }
        public ushort MaxPressureMbar { get; private set; }
        public double MaxDepthMeters
        {
            get
            {
                // caluclated according to Note 1;
                return MaxPressureMbar * 9.80665 / 1000.0;
            }
        }
        public TimeSpan DiveTime { get; private set; }
        public double MinWaterTemperature { get; private set; }
        public ushort SurfacePressureMBar { get; private set; }
        public TimeSpan DesaturationTime { get; private set; }
        public IEnumerable<OSTCGasInfo> Gases { get; private set; }
    }

    public enum LogbookProfileVersion
    {
        V1 = 0x23,
        V2 = 0x24
    }
}

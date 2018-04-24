using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSTCDiveReader
{
    public class OSTCDiveHeader
    {
        internal static OSTCDiveHeader FromBinary(OSTCBinaryReader br)
        {
            try
            {
                byte a = br.ReadByte();
                byte b = br.ReadByte();
                // if this is an empty dive!
                if (a == 0xFF && b == 0xFF)
                    return null;
                if (a != 0xFA || b != 0xFA)
                    throw new InvalidDataException("Expected 0xFAFA as dive header start but got something else!");
            }
            catch(EndOfStreamException)
            {
                return null;
            }

            var dh = new OSTCDiveHeader
            {
                DataStartAddress = br.ReadUInt24(),
                DataStopAddress = br.ReadUInt24(),
                ProfileVersion = (LogbookProfileVersion)br.ReadByte(),
                DataLength = br.ReadUInt24(),
                Date = br.ReadDate(),
                WaterPressureMbarMax = br.ReadUInt16(),
                DiveTime = br.ReadTimeSpan24(),
                WaterTemperatureMin = br.ReadInt16() / 10.0,
                SurfacePressureMBar = br.ReadUInt16(),
                DesaturationTime = br.ReadTimeSpan16(),
                Gases = new List<OSTCGasInfo>(new OSTCGasInfo[] {
                    br.ReadGas32(), // GAS1
                    br.ReadGas32(), // GAS2
                    br.ReadGas32(), // GAS3
                    br.ReadGas32(), // GAS4
                    br.ReadGas32()  // GAS5
                }),
                FirmwareVersion = br.ReadFWVersion(),
                BatteryVoltage = br.ReadUInt16() / 1000.0,
                SamplingRate = TimeSpan.FromSeconds(br.ReadByte()),
                CNSDiveStartPercent = br.ReadUInt16() / 12.0,
                GFDiveStartPercent = br.ReadByte() / 12.0,
                GFDiveEndPercent = br.ReadByte() / 12.0,
                LogbookOffset = br.ReadUInt16(),
                BatteryInformation = br.ReadByte(),
                Setpoints = new List<OSTCSetpointInfo>(new OSTCSetpointInfo[]
                {
                    br.ReadSetpoint16(), // SP1
                    br.ReadSetpoint16(), // SP2
                    br.ReadSetpoint16(), // SP3
                    br.ReadSetpoint16(), // SP4
                    br.ReadSetpoint16()  // SP5
                }),
                Salinity = br.ReadByte(),
                MaxCNSPercent = br.ReadUInt16() / 12.0,
                WaterPressureMBarAverage = br.ReadUInt16(),
                TotalDivetime = TimeSpan.FromSeconds(br.ReadUInt16()),
                DecoModelInfoA = br.ReadByte(),
                DecoModelInfoB = br.ReadByte(),
                DecoModel = (DecoModel)br.ReadByte(),
                DiveNumber = br.ReadUInt16(),
                DiveMode = (DiveMode)br.ReadByte(),
                CompartmentsN2DesatTime = br.ReadBytes(16),
                CompartmentsN2 = br.ReadBytes(64),
                CompartmentsHEDesatTime = br.ReadBytes(16),
                CompartmentsHE = br.ReadBytes(64),
                LastDecoStopMeters = br.ReadByte(),
                AssumedDistanceToShownStopCm = br.ReadByte(),
                HWHudBatteryVoltage = br.ReadUInt16() / 1000.0,
                HWHudLastStatus = br.ReadByte(),
                BatteryGauge = br.ReadBytes(6)
            };
            if (br.ReadByte() != 0xFB || br.ReadByte() != 0xFB)
            {
                throw new InvalidDataException("Expected 0xFBFB as dive header end but got something else!");
            }
            return dh;
        }

        public uint DataStartAddress { get; private set; }
        public uint DataStopAddress { get; private set; }
        public LogbookProfileVersion ProfileVersion { get; private set; }
        public uint DataLength { get; private set; }
        public DateTime Date { get; private set; }
        public ushort WaterPressureMbarMax { get; private set; }
        public double MaxDepthMeters
        {
            get
            {
                // caluclated according to Note 1;
                return WaterPressureMbarMax * 9.80665 / 1000.0;
            }
        }
        public TimeSpan DiveTime { get; private set; }
        public double WaterTemperatureMin { get; private set; }
        public ushort SurfacePressureMBar { get; private set; }
        public TimeSpan DesaturationTime { get; private set; }
        public IEnumerable<OSTCGasInfo> Gases { get; private set; }
        public Version FirmwareVersion { get; private set; }
        public double BatteryVoltage { get; private set; }
        public TimeSpan SamplingRate { get; private set; }
        public double CNSDiveStartPercent { get; private set; }
        public double GFDiveStartPercent { get; private set; }
        public double GFDiveEndPercent { get; private set; }
        public ushort LogbookOffset { get; private set; }
        public byte BatteryInformation { get; private set; }
        public IEnumerable<OSTCSetpointInfo> Setpoints { get; private set; }
        public int Salinity { get; private set; }
        public double MaxCNSPercent { get; private set; }
        public ushort WaterPressureMBarAverage { get; private set; }
        public double AverageDepthMeters
        {
            get
            {
                // caluclated according to Note 1;
                return WaterPressureMBarAverage * 9.80665 / 1000.0;
            }
        }
        public TimeSpan TotalDivetime { get; private set; }
        private byte DecoModelInfoA { get; set; }
        private byte DecoModelInfoB { get; set; }
        public DecoModel DecoModel { get; private set; }
        public ushort DiveNumber { get; private set; }
        public DiveMode DiveMode { get; private set; }
        /// <summary>
        /// 16bytes of desat time for compartments, TODO
        /// </summary>
        public byte[] CompartmentsN2DesatTime { get; private set; }
        /// <summary>
        /// 64bytes comparment data, TODO
        /// </summary>
        public byte[] CompartmentsN2 { get; private set; }
        /// <summary>
        /// 16bytes of desat time for compartments, TODO
        /// </summary>
        public byte[] CompartmentsHEDesatTime { get; private set; }
        /// <summary>
        /// 64bytes comparment data, TODO
        /// </summary>
        public byte[] CompartmentsHE { get; private set; }
        public int LastDecoStopMeters { get; private set; }
        public int AssumedDistanceToShownStopCm { get; private set; }
        public double HWHudBatteryVoltage { get; private set; }
        /// <summary>
        /// Todo: Interpret bitmask
        /// </summary>
        public byte HWHudLastStatus { get; private set; }
        /// <summary>
        /// 48bit / 6bytes of "battery registers" in 1*10^-9As
        /// </summary>
        public byte[] BatteryGauge { get; private set; }
    }

    public enum DiveMode
    {
        OC = 0,
        CC = 1,
        Gauge = 2,
        Apnea = 3
    }

    public enum DecoModel
    {
        ZH_L16 = 0,
        ZH_L16_GF = 1
    }

    public enum LogbookProfileVersion
    {
        V1 = 0x23,
        V2 = 0x24
    }
}

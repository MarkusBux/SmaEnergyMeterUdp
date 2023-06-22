using System;
using System.Text;

namespace SMA;

public class EnergyMeterTelegram
{
    private Dictionary<ObisIdentifier, ulong> values = new();
    private Dictionary<ObisIdentifier, DataPoint> dataPoints = new();
    private readonly byte[] rawData;

    public EnergyMeterTelegram(ReadOnlySpan<byte> data)
	{
        if (data.Length != 608) throw new ArgumentOutOfRangeException(nameof(data), $"Parameter does not match required lengt!");

        rawData = data.ToArray();

        Vendor = Encoding.ASCII.GetString(data[..3]);

        //var b3 = GetUInt16Value(data, 14); // SMA NET 2 = 16 / 0x0010
        ProtocolId = GetUInt16Value(data, 16); // Protocol Id = 24681 / 0x6069
        SUSyID = GetUInt16Value(data, 18); // SUSyID
        SerialNumber = GetUIntValue(data, 20);
        TimeTick = GetUIntValue(data, 24); // Ticker in ms; Überlaufend;

        var position = 28;
        var toPositions = data.Length - 8;

        while (position < toPositions)
        {
            var obisId = new ObisIdentifier(data.Slice(position, 4));
            position += 4;

            var value = data.Slice(position, obisId.DataLength);
            DataPoint? dataPoint = obisId switch
            {
                // Current Sum
                ObisIdentifier when obisId == DataPoint.PowerConsume => new DataPoint(DataPoint.PowerConsume, value, DataPoint.MeasurmentUnit.Watt),
                ObisIdentifier when obisId == DataPoint.PowerSell => new DataPoint(DataPoint.PowerSell, value, DataPoint.MeasurmentUnit.Watt),
                ObisIdentifier when obisId == DataPoint.ReactivePowerConsume => new DataPoint(DataPoint.ReactivePowerConsume, value, DataPoint.MeasurmentUnit.Watt, "var"),
                ObisIdentifier when obisId == DataPoint.ReactivePowerSell => new DataPoint(DataPoint.ReactivePowerSell, value, DataPoint.MeasurmentUnit.Watt, "var"),
                ObisIdentifier when obisId == DataPoint.ApparentPowerConsume => new DataPoint(DataPoint.ApparentPowerConsume, value, DataPoint.MeasurmentUnit.Watt, "VA"),
                ObisIdentifier when obisId == DataPoint.ApparentPowerSell => new DataPoint(DataPoint.ApparentPowerSell, value, DataPoint.MeasurmentUnit.Watt, "VA"),

                // Current Phase 1
                ObisIdentifier when obisId == DataPoint.L1PowerConsume => new DataPoint(DataPoint.L1PowerConsume, value, DataPoint.MeasurmentUnit.Watt),
                ObisIdentifier when obisId == DataPoint.L1PowerSell => new DataPoint(DataPoint.L1PowerSell, value, DataPoint.MeasurmentUnit.Watt),
                ObisIdentifier when obisId == DataPoint.L1ReactivePowerConsume => new DataPoint(DataPoint.L1ReactivePowerConsume, value, DataPoint.MeasurmentUnit.Watt, "var"),
                ObisIdentifier when obisId == DataPoint.L1ReactivePowerSell => new DataPoint(DataPoint.L1ReactivePowerSell, value, DataPoint.MeasurmentUnit.Watt, "var"),
                ObisIdentifier when obisId == DataPoint.L1ApparentPowerConsume => new DataPoint(DataPoint.L1ApparentPowerConsume, value, DataPoint.MeasurmentUnit.Watt, "VA"),
                ObisIdentifier when obisId == DataPoint.L1ApparentPowerSell => new DataPoint(DataPoint.L1ApparentPowerSell, value, DataPoint.MeasurmentUnit.Watt, "VA"),
                ObisIdentifier when obisId == DataPoint.L1Voltage => new DataPoint(DataPoint.L1Voltage, value, DataPoint.MeasurmentUnit.MilliVoltage),
                ObisIdentifier when obisId == DataPoint.L1Ampere => new DataPoint(DataPoint.L1Ampere, value, DataPoint.MeasurmentUnit.MilliAmpere),

                // Current Phase 2
                ObisIdentifier when obisId == DataPoint.L2PowerConsume => new DataPoint(DataPoint.L2PowerConsume, value, DataPoint.MeasurmentUnit.Watt),
                ObisIdentifier when obisId == DataPoint.L2PowerSell => new DataPoint(DataPoint.L2PowerSell, value, DataPoint.MeasurmentUnit.Watt),
                ObisIdentifier when obisId == DataPoint.L2ReactivePowerConsume => new DataPoint(DataPoint.L2ReactivePowerConsume, value, DataPoint.MeasurmentUnit.Watt, "var"),
                ObisIdentifier when obisId == DataPoint.L2ReactivePowerSell => new DataPoint(DataPoint.L2ReactivePowerSell, value, DataPoint.MeasurmentUnit.Watt, "var"),
                ObisIdentifier when obisId == DataPoint.L2ApparentPowerConsume => new DataPoint(DataPoint.L2ApparentPowerConsume, value, DataPoint.MeasurmentUnit.Watt, "VA"),
                ObisIdentifier when obisId == DataPoint.L2ApparentPowerSell => new DataPoint(DataPoint.L2ApparentPowerSell, value, DataPoint.MeasurmentUnit.Watt, "VA"),
                ObisIdentifier when obisId == DataPoint.L2Voltage => new DataPoint(DataPoint.L2Voltage, value, DataPoint.MeasurmentUnit.MilliVoltage),
                ObisIdentifier when obisId == DataPoint.L2Ampere => new DataPoint(DataPoint.L2Ampere, value, DataPoint.MeasurmentUnit.MilliAmpere),

                // Current Phase 3
                ObisIdentifier when obisId == DataPoint.L3PowerConsume => new DataPoint(DataPoint.L3PowerConsume, value, DataPoint.MeasurmentUnit.Watt),
                ObisIdentifier when obisId == DataPoint.L3PowerSell => new DataPoint(DataPoint.L3PowerSell, value, DataPoint.MeasurmentUnit.Watt),
                ObisIdentifier when obisId == DataPoint.L3ReactivePowerConsume => new DataPoint(DataPoint.L3ReactivePowerConsume, value, DataPoint.MeasurmentUnit.Watt, "var"),
                ObisIdentifier when obisId == DataPoint.L3ReactivePowerSell => new DataPoint(DataPoint.L3ReactivePowerSell, value, DataPoint.MeasurmentUnit.Watt, "var"),
                ObisIdentifier when obisId == DataPoint.L3ApparentPowerConsume => new DataPoint(DataPoint.L3ApparentPowerConsume, value, DataPoint.MeasurmentUnit.Watt, "VA"),
                ObisIdentifier when obisId == DataPoint.L3ApparentPowerSell => new DataPoint(DataPoint.L3ApparentPowerSell, value, DataPoint.MeasurmentUnit.Watt, "VA"),
                ObisIdentifier when obisId == DataPoint.L3Voltage => new DataPoint(DataPoint.L3Voltage, value, DataPoint.MeasurmentUnit.MilliVoltage),
                ObisIdentifier when obisId == DataPoint.L3Ampere => new DataPoint(DataPoint.L3Ampere, value, DataPoint.MeasurmentUnit.MilliAmpere),

                // SW Version
                ObisIdentifier when obisId.Channel == 144 => null,

                // everything else
                _ => null
            };

            if (dataPoint is not null)
                dataPoints.Add(dataPoint.Identifier, dataPoint);
        }
    }

    public string Vendor { get; }
    public uint ProtocolId { get; }
    public uint SerialNumber { get; }
    public uint TimeTick { get; }
    public ushort SUSyID { get; }

    public IReadOnlyDictionary<ObisIdentifier, DataPoint> DataPoints => dataPoints.AsReadOnly();

    internal static ulong GetUInt64Value(ReadOnlySpan<byte> data, int offset, bool isBigEndian = true)
    {
        var buffer = new byte[8];
        data.Slice(offset, 8).ToArray().CopyTo(buffer, 0);
        if (isBigEndian)
            return BitConverter.ToUInt64(buffer.Reverse().ToArray());
        else
            return BitConverter.ToUInt64(buffer);
    }

    internal static uint GetUIntValue(ReadOnlySpan<byte> data, int offset, bool isBigEndian = true)
    {
        var buffer = new byte[4];
        data.Slice(offset, 4).ToArray().CopyTo(buffer, 0);
        if (isBigEndian)
            return BitConverter.ToUInt32(buffer.Reverse().ToArray());
        else
            return BitConverter.ToUInt32(buffer);
    }

    internal static ushort GetUInt16Value(ReadOnlySpan<byte> data, int offset, bool isBigEndian = true)
    {
        var buffer = new byte[2];
        data.Slice(offset, 2).ToArray().CopyTo(buffer, 0);
        if (isBigEndian)
            return BitConverter.ToUInt16(buffer.Reverse().ToArray());
        else
            return BitConverter.ToUInt16(buffer);
    }
}



public record class DataPoint {

    internal record class MeasurmentUnit {
        private readonly Func<ulong, double> converter;
        private readonly string convertedValueSuffix;

        private MeasurmentUnit(Func<ulong, double> converter, string convertedValueSuffix) {
            this.converter = converter;
            this.convertedValueSuffix = convertedValueSuffix;
        }

        public double ConvertValue(ulong value) => converter(value);
        public string ConvertedValueSuffix => convertedValueSuffix;

        internal static MeasurmentUnit WattSeconds { get; } = new(v => v / 3600, "Wh");
        internal static MeasurmentUnit Watt { get; } = new(v => v * 0.1, "W");
        internal static MeasurmentUnit MilliVoltage { get; } = new(v => v * 0.001, "V");
        internal static MeasurmentUnit MilliAmpere { get; } = new(v => v * 0.001, "A");

    }

    private readonly ulong rawValue;
    private readonly MeasurmentUnit measurmentUnit;
    private readonly string? customValueSuffix;

    internal DataPoint(ObisIdentifier identifier, ReadOnlySpan<byte> value, MeasurmentUnit measurmentUnit, string? customValueSuffix = null)
    {
        rawValue = value.Length switch {
            4 => EnergyMeterTelegram.GetUIntValue(value,0),
            8 => EnergyMeterTelegram.GetUInt64Value(value,0),
            _ => throw new ArgumentOutOfRangeException()
        };

        Identifier = identifier;
        this.measurmentUnit = measurmentUnit;
        this.customValueSuffix = customValueSuffix;
        Value = measurmentUnit.ConvertValue(rawValue);
    }

    public ObisIdentifier Identifier { get; }
    public double Value { get; }
    public ulong RawValue => rawValue;

    public string ValueSuffix => customValueSuffix ?? measurmentUnit.ConvertedValueSuffix;

    public override string ToString() => $"{Value:0.00} {ValueSuffix}";


    /// <summary>
    /// Summe Wirkleistung Bezug (Wh)
    /// </summary>
	public static ObisIdentifier PowerConsume { get; } = new(new byte[] { 0, 1, 4, 0 });

    /// <summary>
    /// Summe Wirkleistung Einspeisung (Wh)
    /// </summary>
    public static ObisIdentifier PowerSell => new(new byte[] { 0, 2, 4, 0 });

    /// <summary>
    /// Summe Blindleistung Bezug (var)
    /// </summary>
    public static ObisIdentifier ReactivePowerConsume => new(new byte[] { 0, 3, 4, 0 });

    /// <summary>
    /// Summe Blindleistung Einspeisung (var)
    /// </summary>
    public static ObisIdentifier ReactivePowerSell => new(new byte[] { 0, 4, 4, 0 });

    /// <summary>
    /// Summe Scheinleistung Bezug (VA)
    /// </summary>
    public static ObisIdentifier ApparentPowerConsume => new(new byte[] { 0, 9, 4, 0 });

    /// <summary>
    /// Summe Scheinleistung Einspeisung (VA)
    /// </summary>
    public static ObisIdentifier ApparentPowerSell => new(new byte[] { 0, 10, 4, 0 });

    /// <summary>
    /// Leistungsfaktor
    /// </summary>
    public static ObisIdentifier CosPhi => new(new byte[] { 0, 13, 4, 0 });



    /// <summary>
    /// Phase1 Wirkleistung Bezug (Wh)
    /// </summary>
	public static ObisIdentifier L1PowerConsume => new(new byte[] { 0, 21, 4, 0 });

    /// <summary>
    /// Phase1 Wirkleistung Einspeisung (Wh)
    /// </summary>
    public static ObisIdentifier L1PowerSell => new(new byte[] { 0, 22, 4, 0 });

    /// <summary>
    /// Phase1 Blindleistung Bezug (var)
    /// </summary>
    public static ObisIdentifier L1ReactivePowerConsume => new(new byte[] { 0, 23, 4, 0 });

    /// <summary>
    /// Phase1 Blindleistung Einspeisung (var)
    /// </summary>
    public static ObisIdentifier L1ReactivePowerSell => new(new byte[] { 0, 24, 4, 0 });

    /// <summary>
    /// Phase1 Scheinleistung Bezug (VA)
    /// </summary>
    public static ObisIdentifier L1ApparentPowerConsume => new(new byte[] { 0, 29, 4, 0 });

    /// <summary>
    /// Phase1 Scheinleistung Einspeisung (VA)
    /// </summary>
    public static ObisIdentifier L1ApparentPowerSell => new(new byte[] { 0, 30, 4, 0 });

    /// <summary>
    /// Phase1 Strom (amp)
    /// </summary>
    public static ObisIdentifier L1Ampere => new(new byte[] { 0, 31, 4, 0 });

    /// <summary>
    /// Phase1 Spannung (V)
    /// </summary>
    public static ObisIdentifier L1Voltage => new(new byte[] { 0, 32, 4, 0 });


    /// <summary>
    /// Phase2 Wirkleistung Bezug (Wh)
    /// </summary>
    public static ObisIdentifier L2PowerConsume => new(new byte[] { 0, 41, 4, 0 });

    /// <summary>
    /// Phase2 Wirkleistung Einspeisung (Wh)
    /// </summary>
    public static ObisIdentifier L2PowerSell => new(new byte[] { 0, 42, 4, 0 });

    /// <summary>
    /// Phase2 Blindleistung Bezug (var)
    /// </summary>
    public static ObisIdentifier L2ReactivePowerConsume => new(new byte[] { 0, 43, 4, 0 });

    /// <summary>
    /// Phase2 Blindleistung Einspeisung (var)
    /// </summary>
    public static ObisIdentifier L2ReactivePowerSell => new(new byte[] { 0, 44, 4, 0 });

    /// <summary>
    /// Phase2 Scheinleistung Bezug (VA)
    /// </summary>
    public static ObisIdentifier L2ApparentPowerConsume => new(new byte[] { 0, 49, 4, 0 });

    /// <summary>
    /// Phase2 Scheinleistung Einspeisung (VA)
    /// </summary>
    public static ObisIdentifier L2ApparentPowerSell => new(new byte[] { 0, 50, 4, 0 });

    /// <summary>
    /// Phase2 Strom (amp)
    /// </summary>
    public static ObisIdentifier L2Ampere => new(new byte[] { 0, 51, 4, 0 });

    /// <summary>
    /// Phase2 Spannung (V)
    /// </summary>
    public static ObisIdentifier L2Voltage => new(new byte[] { 0, 52, 4, 0 });


    /// <summary>
    /// Phase3 Wirkleistung Bezug (Wh)
    /// </summary>
    public static ObisIdentifier L3PowerConsume => new(new byte[] { 0, 61, 4, 0 });
    /// <summary>
    /// Phase3 Wirkleistung Einspeisung (Wh)
    /// </summary>
    public static ObisIdentifier L3PowerSell => new(new byte[] { 0, 62, 4, 0 });

    /// <summary>
    /// Phase3 Blindleistung Bezug (var)
    /// </summary>
    public static ObisIdentifier L3ReactivePowerConsume => new(new byte[] { 0, 63, 4, 0 });

    /// <summary>
    /// Phase3 Blindleistung Einspeisung (var)
    /// </summary>
    public static ObisIdentifier L3ReactivePowerSell => new(new byte[] { 0, 64, 4, 0 });

    /// <summary>
    /// Phase3 Scheinleistung Bezug (VA)
    /// </summary>
    public static ObisIdentifier L3ApparentPowerConsume => new(new byte[] { 0, 69, 4, 0 });

    /// <summary>
    /// Phase3 Scheinleistung Einspeisung (VA)
    /// </summary>
    public static ObisIdentifier L3ApparentPowerSell => new(new byte[] { 0, 70, 4, 0 });

    /// <summary>
    /// Phase3 Strom (amp)
    /// </summary>
    public static ObisIdentifier L3Ampere => new(new byte[] { 0, 71, 4, 0 });

    /// <summary>
    /// Phase3 Spannung (V)
    /// </summary>
    public static ObisIdentifier L3Voltage => new(new byte[] { 0, 72, 4, 0 });
}
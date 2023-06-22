using System;
using System.Diagnostics;

namespace SMA;

[DebuggerDisplay("{Channel}:{Index}.{MeasurementType}.{Tariff}")]
public record struct ObisIdentifier
{
    private readonly string? friendlyName;

    internal ObisIdentifier(ReadOnlySpan<byte> obisId, string? friendlyName = null)
	{
		if (obisId.Length != 4) throw new ArgumentException("Invalid data provided! Identifier must be 4 byte!", nameof(obisId));

		this.friendlyName = string.IsNullOrWhiteSpace(friendlyName) ? null:friendlyName;

		Channel = obisId[0];
        Index = obisId[1];
        MeasurementType = obisId[2];
        Tariff = obisId[3];
    }


	public byte Channel { get; set; }
	public byte Index { get; set; }
	public byte MeasurementType { get; set; }
	public byte Tariff { get; set; }

    public readonly string FriendlyName => friendlyName ?? ToString();
    public readonly ushort DataLength => MeasurementType == 4 ? (ushort)4 : (ushort)8;

    public override readonly string ToString() => $"{Channel}:{Index}.{MeasurementType}.{Tariff}";
}


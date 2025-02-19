namespace MassTransit.GrpcTransport.Integration
{
    using System;
    using Contracts;
    using Google.Protobuf;
    using Google.Protobuf.WellKnownTypes;
    using Metadata;


    public static class TransportMessageExtensions
    {
        public static Node Initialize(this Node node, IGrpcNode hostNode)
        {
            node.Address = hostNode.NodeAddress.ToString();
            node.Version = "1.0-alpha";
            node.SessionId = ToUuid(hostNode.SessionId);

            node.Host.Add(nameof(HostMetadataCache.Host.Assembly), HostMetadataCache.Host.Assembly);
            node.Host.Add(nameof(HostMetadataCache.Host.AssemblyVersion), HostMetadataCache.Host.AssemblyVersion);
            node.Host.Add(nameof(HostMetadataCache.Host.FrameworkVersion), HostMetadataCache.Host.FrameworkVersion);
            node.Host.Add(nameof(HostMetadataCache.Host.MachineName), HostMetadataCache.Host.MachineName);
            node.Host.Add(nameof(HostMetadataCache.Host.ProcessId), HostMetadataCache.Host.ProcessId.ToString());
            node.Host.Add(nameof(HostMetadataCache.Host.ProcessName), HostMetadataCache.Host.ProcessName);
            node.Host.Add(nameof(HostMetadataCache.Host.MassTransitVersion), HostMetadataCache.Host.MassTransitVersion);
            node.Host.Add(nameof(HostMetadataCache.Host.OperatingSystemVersion), HostMetadataCache.Host.OperatingSystemVersion);

            if (hostNode != null)
                node.Topology.AddRange(hostNode.GetTopology());

            return node;
        }

        public static NullableString ToNullableString(this string value)
        {
            return value == null ? new NullableString() : new NullableString {Value = value};
        }

        public static string ToStringValue(this NullableString value)
        {
            return value.StringCase == NullableString.StringOneofCase.None
                ? default
                : value.Value;
        }

        public static Guid? ToGuid(this NullableUuid value)
        {
            return value.UuidCase == NullableUuid.UuidOneofCase.None
                ? default
                : new Guid(value.Value.Value.ToByteArray());
        }

        public static Uuid ToUuid(this Guid value)
        {
            return new Uuid {Value = ByteString.CopyFrom(value.ToByteArray())};
        }

        public static NullableUuid ToUuid(this Guid? value)
        {
            return value.HasValue
                ? new NullableUuid {Value = new Uuid {Value = ByteString.CopyFrom(value.Value.ToByteArray())}}
                : new NullableUuid();
        }

        public static NullableString ToNullableString(this Uri value)
        {
            return value != null
                ? new NullableString {Value = value.ToString()}
                : new NullableString();
        }

        public static DateTime? ToDateTime(this NullableTimestamp value)
        {
            return value.TimestampCase == NullableTimestamp.TimestampOneofCase.None
                ? default
                : value.Value.ToDateTime();
        }

        public static NullableTimestamp ToFutureDateTime(this TimeSpan? value)
        {
            return value.HasValue
                ? new NullableTimestamp {Value = Timestamp.FromDateTime(DateTime.UtcNow + value.Value)}
                : new NullableTimestamp();
        }

        public static Guid ToGuid(this Uuid value)
        {
            return new Guid(value.Value.ToByteArray());
        }

        public static Uri ToUri(this NullableString value)
        {
            try
            {
                return value.StringCase == NullableString.StringOneofCase.None
                    ? default
                    : new Uri(value.Value);
            }
            catch (FormatException)
            {
                return default;
            }
        }
    }
}

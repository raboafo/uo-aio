#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UOAIO.ShardRuntime;

public static class ClientBootstrapSerializer
{
    public static string Serialize(ClientBootstrapDefinition bootstrap)
    {
        ClientBootstrapEnvelope envelope = ToEnvelope(bootstrap);
        using MemoryStream stream = new();
        DataContractJsonSerializer serializer = CreateSerializer();
        serializer.WriteObject(stream, envelope);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static ClientBootstrapDefinition Deserialize(string json)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(json ?? string.Empty));
        DataContractJsonSerializer serializer = CreateSerializer();
        ClientBootstrapEnvelope? envelope = serializer.ReadObject(stream) as ClientBootstrapEnvelope;
        return FromEnvelope(envelope);
    }

    private static DataContractJsonSerializer CreateSerializer()
    {
        return new DataContractJsonSerializer(
            typeof(ClientBootstrapEnvelope),
            new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true
            });
    }

    private static ClientBootstrapEnvelope ToEnvelope(ClientBootstrapDefinition bootstrap)
    {
        if (bootstrap is null)
        {
            throw new ArgumentNullException(nameof(bootstrap));
        }

        if (bootstrap.Shard is null)
        {
            throw new InvalidOperationException("Client bootstrap is missing shard details.");
        }

        return new ClientBootstrapEnvelope
        {
            SchemaVersion = bootstrap.SchemaVersion,
            CreatedAtUtc = bootstrap.CreatedAtUtc,
            Shard = ToEnvelope(bootstrap.Shard)
        };
    }

    private static ShardDefinitionEnvelope ToEnvelope(ShardDefinition shard)
    {
        return new ShardDefinitionEnvelope
        {
            Id = shard.Id,
            Name = shard.Name,
            Description = shard.Description,
            Host = shard.Host,
            Account = shard.Account,
            Password = shard.Password,
            UOClientVersion = shard.UOClientVersion?.ToString() ?? string.Empty,
            ServerIp = shard.ServerIP?.ToString() ?? string.Empty,
            Port = shard.ServerPort,
            Metadata = new Dictionary<string, string>(shard.Metadata ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase)
        };
    }

    private static ClientBootstrapDefinition FromEnvelope(ClientBootstrapEnvelope? envelope)
    {
        if (envelope?.Shard is null)
        {
            throw new InvalidOperationException("Unable to deserialize client bootstrap definition.");
        }

        return new ClientBootstrapDefinition
        {
            SchemaVersion = envelope.SchemaVersion,
            CreatedAtUtc = envelope.CreatedAtUtc,
            Shard = FromEnvelope(envelope.Shard)
        };
    }

    private static ShardDefinition FromEnvelope(ShardDefinitionEnvelope envelope)
    {
        IPAddress? serverIp = null;
        if (!string.IsNullOrWhiteSpace(envelope.ServerIp))
        {
            serverIp = IPAddress.Parse(envelope.ServerIp);
        }

        Version? clientVersion = null;
        if (!string.IsNullOrWhiteSpace(envelope.UOClientVersion))
        {
            clientVersion = Version.Parse(envelope.UOClientVersion);
        }

        return new ShardDefinition
        {
            Id = envelope.Id ?? string.Empty,
            Name = envelope.Name ?? string.Empty,
            Description = envelope.Description ?? string.Empty,
            Host = envelope.Host ?? string.Empty,
            Account = envelope.Account ?? string.Empty,
            Password = envelope.Password ?? string.Empty,
            UOClientVersion = clientVersion,
            ServerIP = serverIp,
            ServerPort = envelope.Port,
            Metadata = envelope.Metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };
    }
}

public sealed class ClientBootstrapPipeTransport
{
    public static string CreatePipeName()
    {
        return $"uoaio-bootstrap-{Guid.NewGuid():N}";
    }

    public async Task WriteAsync(string pipeName, ClientBootstrapDefinition bootstrap, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pipeName))
        {
            throw new ArgumentException("A pipe name is required.", nameof(pipeName));
        }

        string payload = ClientBootstrapSerializer.Serialize(bootstrap);
        using NamedPipeServerStream pipe = new(
            pipeName,
            PipeDirection.Out,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        await pipe.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        await pipe.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public ClientBootstrapDefinition Read(string pipeName, int connectTimeoutMilliseconds = 15000)
    {
        if (string.IsNullOrWhiteSpace(pipeName))
        {
            throw new ArgumentException("A pipe name is required.", nameof(pipeName));
        }

        using NamedPipeClientStream pipe = new(".", pipeName, PipeDirection.In);
        pipe.Connect(connectTimeoutMilliseconds);
        using StreamReader reader = new(pipe, Encoding.UTF8, true);
        string json = reader.ReadToEnd();
        return ClientBootstrapSerializer.Deserialize(json);
    }
}

[DataContract]
internal sealed class ClientBootstrapEnvelope
{
    [DataMember(Name = "schemaVersion")]
    public int SchemaVersion { get; set; }

    [DataMember(Name = "createdAtUtc")]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [DataMember(Name = "shard")]
    public ShardDefinitionEnvelope Shard { get; set; } = new();
}

[DataContract]
internal sealed class ShardDefinitionEnvelope
{
    [DataMember(Name = "id")]
    public string Id { get; set; } = string.Empty;

    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "description")]
    public string Description { get; set; } = string.Empty;

    [DataMember(Name = "host")]
    public string Host { get; set; } = string.Empty;

    [DataMember(Name = "account")]
    public string Account { get; set; } = string.Empty;

    [DataMember(Name = "password")]
    public string Password { get; set; } = string.Empty;

    [DataMember(Name = "uoClientVersion")]
    public string UOClientVersion { get; set; } = string.Empty;

    [DataMember(Name = "serverIp")]
    public string ServerIp { get; set; } = string.Empty;

    [DataMember(Name = "port")]
    public int Port { get; set; }

    [DataMember(Name = "metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

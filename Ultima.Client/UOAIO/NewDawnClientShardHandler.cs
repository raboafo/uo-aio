using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UOAIO.ShardRuntime;

namespace UOAIO;

internal sealed class NewDawnClientShardHandler : IClientBootstrapHandler
{
    private ClientBootstrapDefinition _clientDef;
    private ShardDefinition _shard;
    private string _token;
    private string _newDawnClientVersion;

    public string ShardId => "uo-new-dawn";

    public void InitializeBootstrap(ClientBootstrapDefinition clientDef)
    {
        _clientDef = clientDef;
        _shard = clientDef.Shard;
        _token = _shard.GetMetadata("jwt");
        _newDawnClientVersion = _shard.GetMetadata("new_dawn_client_version");

        PacketHandlers.Register(0x8C, 11, ReceiveServerRelay);
        PacketHandlers.Register(0xC9, 3, NewDawnClientVersion);
    }

    public void SendFirstLogin()
    {
        Network.Send(new PLoginSeed(0x839D1E0A, _shard.ClientVersion));
        Network.Send(new PAccount(_shard.Account,_shard.Password, _token));
    }

    private class PLoginSeed : Packet
    {
        public PLoginSeed(uint seed, Version version)
            : base(239, 21)
        {
            base.m_Encode = false;
            base.m_Stream.Write(seed);
            base.m_Stream.Write(version.Major);
            base.m_Stream.Write(version.Minor);
            base.m_Stream.Write(version.Build);
            base.m_Stream.Write(version.Revision);
        }
    }

    private class PAccount : Packet
    {
        public PAccount(string account, string password, string token) : base(128, 1062)
        {
            base.m_Encode = false;
            base.m_Stream.Write(account, 30);
            base.m_Stream.Write(password, 30);
            base.m_Stream.Write(token);
            base.m_Stream.Write((ushort)0xFF);
            int padding = 1000 - token.Length - 1;
            padding = Math.Max(0, padding);
            base.m_Stream.Write(new byte[padding]);
        }
    }

    private class PGameLogin : Packet
    {
        public PGameLogin(uint authId, string account, string password, string jwt)
            : base(145, 1065)
        {
            base.m_Encode = false;
            base.m_Stream.Write(authId);
            base.m_Stream.Write(account, 30);
            base.m_Stream.Write(password, 30);
            base.m_Stream.Write(jwt, 1000);
        }
    }

    private class PNewDawnClientVersion : Packet
    {
        public PNewDawnClientVersion(string version, string key)
            : base(201)
        {
            string text = GenerateHmac(version, key);
            base.m_Stream.Write(text, text.Length + 1);
        }
    }

    private void ReceiveServerRelay(PacketReader pvSrc)
    {
        pvSrc.ReadBytes(4);
        pvSrc.ReadUInt16();
        uint authId = pvSrc.ReadUInt32();

        GameCrypto gameCrypto = new GameCrypto(authId);
        Debug.Trace($"Server relay: connecting to {_shard.ServerIP}:{_shard.ServerPort}");
        if (!Network.Connect(gameCrypto, new IPEndPoint(_shard.ServerIP, _shard.ServerPort)))
        {
            throw new InvalidOperationException("Failed to connect.");
        }

        Network.Send(new PGameSeed(authId));
        Network.Send(new PGameLogin(authId, _shard.Account, _shard.Password, _token));
        Network.Context._crypto = gameCrypto;
        Network.Flush();
    }

    private static string GenerateHmac(string message, string key)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(key.ToLowerInvariant());
        byte[] bytes2 = Encoding.UTF8.GetBytes(message);
        using HMACSHA256 hMACSHA = new HMACSHA256(bytes);
        return Convert.ToBase64String(hMACSHA.ComputeHash(bytes2));
    }

    private void NewDawnClientVersion(PacketReader pvSrc)
    {
        pvSrc.ReadUInt16();
        Network.Send(new PNewDawnClientVersion(_newDawnClientVersion, _shard.Account));
    }
}

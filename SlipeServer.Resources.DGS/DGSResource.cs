﻿using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Interpreters.Meta;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace SlipeServer.Resources.DGS;

internal class DGSResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new();
    private readonly Dictionary<LuaValue, LuaValue> DGSRecordedFiles = new();
    private readonly DGSVersion version;
    private Task downloadDGSTask;

    internal DGSResource(MtaServer server, DGSVersion version)
        : base(server, server.GetRequiredService<RootElement>(), "dgs")
    {
        this.version = version;

        downloadDGSTask = Task.Run(DownloadDGS);
        //Root.SetData("DGSI_FileInfo", DGSRecordedFiles, DataSyncType.Broadcast);
        server.PlayerJoined += Server_PlayerJoined;
    }

    private async Task DownloadDGS()
    {
        var versionString = version switch
        {
            DGSVersion.Release_3_518 => "3.518",
            _ => throw new NotImplementedException()
        };
        var downloadPath = $"https://github.com/thisdp/dgs/archive/refs/tags/{versionString}.zip";

        var client = new HttpClient();
        var response = await client.GetAsync(downloadPath);
        response.EnsureSuccessStatusCode();

        using var zip = new ZipArchive(response.Content.ReadAsStream(), ZipArchiveMode.Read);
        var metaXmlEntry = zip.GetEntry($"dgs-{versionString}/meta.xml");

        StreamReader streamReader = new StreamReader(metaXmlEntry.Open());
        MetaXml? metaXml = (MetaXml?)new XmlSerializer(typeof(MetaXml)).Deserialize(streamReader);

        using SHA256 sha256Hash = SHA256.Create();
        foreach (MetaXmlFile item in metaXml.Value.files)
        {
            var entry = zip.GetEntry($"dgs-{versionString}/{item.Source}");
            using StreamReader sr = new StreamReader(entry.Open());
            var data = Encoding.Default.GetBytes(sr.ReadToEnd());
            Files.Add(ResourceFileFactory.FromBytes(data, item.Source, ResourceFileType.ClientFile));
            AdditionalFiles.Add(item.Source, data);
            string hash = GetHash(sha256Hash, data);
            DGSRecordedFiles.Add(item.Source, new LuaValue(new LuaValue[] { hash, data.Length }));
        }

        foreach (MetaXmlScript item in metaXml.Value.scripts.Where((MetaXmlScript x) => x.Type == "client"))
        {
            var entry = zip.GetEntry($"dgs-{versionString}/{item.Source}");
            using StreamReader sr = new StreamReader(entry.Open());
            var data = Encoding.Default.GetBytes(sr.ReadToEnd());
            Files.Add(ResourceFileFactory.FromBytes(data, item.Source, ResourceFileType.ClientScript));
            AdditionalFiles.Add(item.Source, data);
        }

        Exports.AddRange(metaXml.Value.exports
            .Where(x => x.Type == "client")
            .Select(x => x.Function));
    }

    private void Server_PlayerJoined(Player obj)
    {
        downloadDGSTask.Wait();
        Root.SetData("DGSI_FileInfo", DGSRecordedFiles, DataSyncType.Broadcast);
    }

    private static string GetHash(HashAlgorithm hashAlgorithm, byte[] input)
    {
        byte[] data = hashAlgorithm.ComputeHash(input);

        var sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString();
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using UOAIO;
using Veritas;

namespace UOAIO.Profiles;

[DataContract]
internal sealed class PreferencesDocument
{
    [DataMember(Name = "renderSettings")]
    public RenderSettingsDocument RenderSettings { get; set; } = new();

    [DataMember(Name = "audio")]
    public AudioPreferencesDocument Audio { get; set; } = new();

    [DataMember(Name = "speechHues")]
    public SpeechHuesDocument SpeechHues { get; set; } = new();

    [DataMember(Name = "notorietyHues")]
    public NotorietyHuesDocument NotorietyHues { get; set; } = new();

    [DataMember(Name = "options")]
    public OptionsDocument Options { get; set; } = new();

    [DataMember(Name = "scavenger")]
    public ScavengerDocument Scavenger { get; set; } = new();

    [DataMember(Name = "layout")]
    public ScreenLayoutDocument Layout { get; set; } = new();
}

[DataContract]
internal sealed class RenderSettingsDocument
{
    [DataMember(Name = "terrainQuality")]
    public int TerrainQuality { get; set; } = 1;

    [DataMember(Name = "smoothingMode")]
    public int SmoothingMode { get; set; } = 1;

    [DataMember(Name = "gameplayWindow")]
    public int GameplayWindow { get; set; } = 1;

    [DataMember(Name = "smoothCharacters")]
    public bool SmoothCharacters { get; set; } = true;

    [DataMember(Name = "animatedCharacters")]
    public bool AnimatedCharacters { get; set; } = true;

    [DataMember(Name = "itemShadows")]
    public bool ItemShadows { get; set; } = true;

    [DataMember(Name = "characterShadows")]
    public bool CharacterShadows { get; set; } = true;
}

[DataContract]
internal sealed class AudioPreferencesDocument
{
    [DataMember(Name = "footsteps")]
    public VolumeDocument Footsteps { get; set; } = new();

    [DataMember(Name = "sound")]
    public VolumeDocument Sound { get; set; } = new();

    [DataMember(Name = "music")]
    public VolumeDocument Music { get; set; } = new();
}

[DataContract]
internal sealed class VolumeDocument
{
    [DataMember(Name = "scale")]
    public int Scale { get; set; } = 10000;

    [DataMember(Name = "mute")]
    public bool Mute { get; set; }
}

[DataContract]
internal sealed class SpeechHuesDocument
{
    [DataMember(Name = "regular")]
    public int Regular { get; set; } = 96;

    [DataMember(Name = "yell")]
    public int Yell { get; set; } = 96;

    [DataMember(Name = "emote")]
    public int Emote { get; set; } = 96;

    [DataMember(Name = "whisper")]
    public int Whisper { get; set; } = 96;
}

[DataContract]
internal sealed class NotorietyHuesDocument
{
    [DataMember(Name = "innocent")]
    public int Innocent { get; set; } = 89;

    [DataMember(Name = "ally")]
    public int Ally { get; set; } = 63;

    [DataMember(Name = "attackable")]
    public int Attackable { get; set; } = 1303;

    [DataMember(Name = "criminal")]
    public int Criminal { get; set; } = 946;

    [DataMember(Name = "enemy")]
    public int Enemy { get; set; } = 144;

    [DataMember(Name = "murderer")]
    public int Murderer { get; set; } = 34;

    [DataMember(Name = "vendor")]
    public int Vendor { get; set; } = 53;
}

[DataContract]
internal sealed class OptionsDocument
{
    [DataMember(Name = "flags")]
    public int Flags { get; set; } = (int)OptionFlag.Default;

    [DataMember(Name = "notoQuery")]
    public int NotoQuery { get; set; } = (int)NotoQueryType.On;

    [DataMember(Name = "houseLevel")]
    public int HouseLevel { get; set; } = 1;
}

[DataContract]
internal sealed class ScavengerDocument
{
    [DataMember(Name = "options")]
    public int Options { get; set; } = (int)ScavengerOptions.Default;

    [DataMember(Name = "items")]
    public ItemRefDocument[] Items { get; set; } = Array.Empty<ItemRefDocument>();
}

[DataContract]
internal sealed class ItemRefDocument
{
    [DataMember(Name = "serial")]
    public int Serial { get; set; }

    [DataMember(Name = "itemId")]
    public int ItemId { get; set; }
}

[DataContract]
internal sealed class ScreenLayoutDocument
{
    [DataMember(Name = "gameBounds")]
    public RectangleDocument GameBounds { get; set; } = new();

    [DataMember(Name = "screenBounds")]
    public RectangleDocument ScreenBounds { get; set; } = new();

    [DataMember(Name = "maximized")]
    public bool Maximized { get; set; }

    [DataMember(Name = "fullSize")]
    public SizeDocument FullSize { get; set; } = new();

    [DataMember(Name = "fullscreen")]
    public bool Fullscreen { get; set; }

    [DataMember(Name = "gumps")]
    public GumpLayoutDocument[] Gumps { get; set; } = Array.Empty<GumpLayoutDocument>();
}

[DataContract]
internal sealed class RectangleDocument
{
    [DataMember(Name = "x")]
    public int X { get; set; }

    [DataMember(Name = "y")]
    public int Y { get; set; }

    [DataMember(Name = "width")]
    public int Width { get; set; }

    [DataMember(Name = "height")]
    public int Height { get; set; }
}

[DataContract]
internal sealed class SizeDocument
{
    [DataMember(Name = "width")]
    public int Width { get; set; }

    [DataMember(Name = "height")]
    public int Height { get; set; }
}

[DataContract]
internal sealed class GumpLayoutDocument
{
    [DataMember(Name = "kind")]
    public string Kind { get; set; } = string.Empty;

    [DataMember(Name = "x")]
    public int X { get; set; }

    [DataMember(Name = "y")]
    public int Y { get; set; }

    [DataMember(Name = "id")]
    public int Id { get; set; }
}

internal sealed class ServerRuntimeState : PersistableObject
{
    public static readonly PersistableType TypeCode = new("serverState", Construct, IgnoreList.TypeCode, TravelAgent.TypeCode);

    private IgnoreList _ignoreList = new();
    private TravelAgent _travelAgent = new();

    public override PersistableType TypeID => TypeCode;

    public IgnoreList IgnoreList => _ignoreList;

    public TravelAgent TravelAgent => _travelAgent;

    private static PersistableObject Construct()
    {
        return new ServerRuntimeState();
    }

    public ServerRuntimeState()
    {
    }

    public ServerRuntimeState(Server server)
    {
        _ignoreList = server.IgnoreList ?? new IgnoreList();
        _travelAgent = server.TravelAgent ?? new TravelAgent();
    }

    protected override void SerializeChildren(PersistanceWriter op)
    {
        _ignoreList.Serialize(op);
        _travelAgent.Serialize(op);
    }

    protected override void DeserializeChildren(PersistanceReader ip)
    {
        while (ip.HasChild)
        {
            object child = ip.GetChild();
            if (child is IgnoreList ignoreList)
            {
                _ignoreList = ignoreList;
            }
            else if (child is TravelAgent travelAgent)
            {
                _travelAgent = travelAgent;
            }
        }
    }
}

internal sealed class CharacterRuntimeState : PersistableObject
{
    public static readonly PersistableType TypeCode = new("characterState", Construct, GuildRoster.TypeCode, Player.TypeCode);

    private GuildRoster _guildRoster = new();
    private Player _player;

    public override PersistableType TypeID => TypeCode;

    public GuildRoster GuildRoster => _guildRoster;

    public Player Player => _player;

    private static PersistableObject Construct()
    {
        return new CharacterRuntimeState();
    }

    public CharacterRuntimeState()
    {
    }

    public CharacterRuntimeState(Profile profile, Player player)
    {
        _guildRoster = profile.GuildRoster ?? new GuildRoster();
        _player = player;
    }

    protected override void SerializeChildren(PersistanceWriter op)
    {
        _guildRoster.Serialize(op);
        _player?.Serialize(op);
    }

    protected override void DeserializeChildren(PersistanceReader ip)
    {
        while (ip.HasChild)
        {
            object child = ip.GetChild();
            if (child is GuildRoster guildRoster)
            {
                _guildRoster = guildRoster;
            }
            else if (child is Player player)
            {
                _player = player;
            }
        }
    }
}

internal static class RuntimeProfileStores
{
    private static readonly DataContractJsonSerializer PreferencesSerializer = new(typeof(PreferencesDocument));

    public static Preferences CreatePreferencesOrDefault()
    {
        Preferences preferences = new();
        if (!ClientRuntimeEnvironment.TryCharacterDataPath("preferences.json", out string path) || !File.Exists(path))
        {
            return preferences;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            PreferencesDocument document = PreferencesSerializer.ReadObject(stream) as PreferencesDocument;
            if (document != null)
            {
                Apply(preferences, document);
            }
        }
        catch
        {
            return new Preferences();
        }

        return preferences;
    }

    public static void SavePreferences(Preferences preferences)
    {
        if (preferences == null || !ClientRuntimeEnvironment.TryCharacterDataPath("preferences.json", out string path))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using FileStream stream = new(path, FileMode.Create, FileAccess.Write, FileShare.None);
        PreferencesSerializer.WriteObject(stream, Capture(preferences));
    }

    public static ServerRuntimeState LoadServerState()
    {
        string path = ClientRuntimeEnvironment.AccountDataPath("server-state.xml");
        return LoadXml<ServerRuntimeState>(path);
    }

    public static void SaveServerState(Server server)
    {
        if (server == null)
        {
            return;
        }

        string path = ClientRuntimeEnvironment.AccountDataPath("server-state.xml");
        SaveXml(new ServerRuntimeState(server), path);
    }

    public static CharacterRuntimeState LoadCharacterState()
    {
        if (!ClientRuntimeEnvironment.TryCharacterDataPath("character-state.xml", out string path))
        {
            return null;
        }

        return LoadXml<CharacterRuntimeState>(path);
    }

    public static void SaveCharacterState(Profile profile, Player player)
    {
        if (profile == null || player == null || !ClientRuntimeEnvironment.TryCharacterDataPath("character-state.xml", out string path))
        {
            return;
        }

        SaveXml(new CharacterRuntimeState(profile, player), path);
    }

    private static T LoadXml<T>(string path) where T : PersistableObject
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        XmlPersistanceReader reader = new(stream);
        T instance = Activator.CreateInstance<T>();
        reader.ReadDocument(instance);
        reader.Close();
        return instance;
    }

    private static void SaveXml(PersistableObject persistableObject, string path)
    {
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        XmlPersistanceWriter writer = new(path);
        writer.WriteDocument(persistableObject);
        writer.Close();
    }

    private static PreferencesDocument Capture(Preferences preferences)
    {
        PreferencesDocument document = new()
        {
            RenderSettings = new RenderSettingsDocument
            {
                TerrainQuality = preferences.RenderSettings.TerrainQuality,
                SmoothingMode = preferences.RenderSettings.SmoothingMode,
                GameplayWindow = preferences.RenderSettings.GameplayWindow,
                SmoothCharacters = preferences.RenderSettings.SmoothCharacters,
                AnimatedCharacters = preferences.RenderSettings.AnimatedCharacters,
                ItemShadows = preferences.RenderSettings.ItemShadows,
                CharacterShadows = preferences.RenderSettings.CharacterShadows
            },
            Audio = new AudioPreferencesDocument
            {
                Footsteps = Capture(preferences.Footsteps.Volume),
                Sound = Capture(preferences.Sound.Volume),
                Music = Capture(preferences.Music.Volume)
            },
            SpeechHues = new SpeechHuesDocument
            {
                Regular = preferences.SpeechHues.Regular,
                Yell = preferences.SpeechHues.Yell,
                Emote = preferences.SpeechHues.Emote,
                Whisper = preferences.SpeechHues.Whisper
            },
            NotorietyHues = new NotorietyHuesDocument
            {
                Innocent = preferences.NotorietyHues.Innocent,
                Ally = preferences.NotorietyHues.Ally,
                Attackable = preferences.NotorietyHues.Attackable,
                Criminal = preferences.NotorietyHues.Criminal,
                Enemy = preferences.NotorietyHues.Enemy,
                Murderer = preferences.NotorietyHues.Murderer,
                Vendor = preferences.NotorietyHues.Vendor
            },
            Options = new OptionsDocument
            {
                Flags = GetFlags(preferences.Options),
                NotoQuery = (int)preferences.Options.NotorietyQuery,
                HouseLevel = preferences.Options.HouseLevel
            },
            Scavenger = new ScavengerDocument
            {
                Options = preferences.Scavenger.OptionsValue,
                Items = Capture(preferences.Scavenger.Items)
            },
            Layout = Capture(preferences.Layout)
        };

        return document;
    }

    private static void Apply(Preferences preferences, PreferencesDocument document)
    {
        preferences.RenderSettings.TerrainQuality = document.RenderSettings.TerrainQuality;
        preferences.RenderSettings.SmoothingMode = document.RenderSettings.SmoothingMode;
        preferences.RenderSettings.GameplayWindow = document.RenderSettings.GameplayWindow;
        preferences.RenderSettings.SmoothCharacters = document.RenderSettings.SmoothCharacters;
        preferences.RenderSettings.AnimatedCharacters = document.RenderSettings.AnimatedCharacters;
        preferences.RenderSettings.ItemShadows = document.RenderSettings.ItemShadows;
        preferences.RenderSettings.CharacterShadows = document.RenderSettings.CharacterShadows;

        Apply(preferences.Footsteps.Volume, document.Audio.Footsteps);
        Apply(preferences.Sound.Volume, document.Audio.Sound);
        Apply(preferences.Music.Volume, document.Audio.Music);

        preferences.SpeechHues.Regular = document.SpeechHues.Regular;
        preferences.SpeechHues.Yell = document.SpeechHues.Yell;
        preferences.SpeechHues.Emote = document.SpeechHues.Emote;
        preferences.SpeechHues.Whisper = document.SpeechHues.Whisper;

        preferences.NotorietyHues.Innocent = document.NotorietyHues.Innocent;
        preferences.NotorietyHues.Ally = document.NotorietyHues.Ally;
        preferences.NotorietyHues.Attackable = document.NotorietyHues.Attackable;
        preferences.NotorietyHues.Criminal = document.NotorietyHues.Criminal;
        preferences.NotorietyHues.Enemy = document.NotorietyHues.Enemy;
        preferences.NotorietyHues.Murderer = document.NotorietyHues.Murderer;
        preferences.NotorietyHues.Vendor = document.NotorietyHues.Vendor;

        preferences.Options.ApplyState((OptionFlag)document.Options.Flags, (NotoQueryType)document.Options.NotoQuery, document.Options.HouseLevel);
        preferences.Scavenger.ApplyState(document.Scavenger.Options, RestoreItems(document.Scavenger.Items));
        Apply(preferences.Layout, document.Layout);
    }

    private static VolumeDocument Capture(Volume volume)
    {
        return new VolumeDocument
        {
            Scale = volume.Scale,
            Mute = volume.Mute
        };
    }

    private static void Apply(Volume volume, VolumeDocument document)
    {
        volume.Scale = document.Scale;
        volume.Mute = document.Mute;
    }

    private static ScreenLayoutDocument Capture(ScreenLayout layout)
    {
        List<GumpLayoutDocument> gumps = new List<GumpLayoutDocument>();
        foreach (GumpLayout gumpLayout in layout.Gumps)
        {
            if (gumpLayout is SpellIconLayout spellIconLayout)
            {
                gumps.Add(new GumpLayoutDocument
                {
                    Kind = "spell",
                    X = spellIconLayout.Offset.X,
                    Y = spellIconLayout.Offset.Y,
                    Id = spellIconLayout.SpellID
                });
            }
            else if (gumpLayout is SkillIconLayout skillIconLayout)
            {
                gumps.Add(new GumpLayoutDocument
                {
                    Kind = "skill",
                    X = skillIconLayout.Offset.X,
                    Y = skillIconLayout.Offset.Y,
                    Id = skillIconLayout.SkillID
                });
            }
        }

        return new ScreenLayoutDocument
        {
            GameBounds = Capture(layout.GameBounds),
            ScreenBounds = Capture(layout.ScreenBounds),
            Maximized = layout.Maximized,
            FullSize = Capture(layout.FullSize),
            Fullscreen = layout.Fullscreen,
            Gumps = gumps.ToArray()
        };
    }

    private static void Apply(ScreenLayout layout, ScreenLayoutDocument document)
    {
        layout.GameBounds = Restore(document.GameBounds);
        layout.ScreenBounds = Restore(document.ScreenBounds);
        layout.Maximized = document.Maximized;
        layout.FullSize = Restore(document.FullSize);
        layout.Fullscreen = document.Fullscreen;

        GumpLayoutCollection gumps = new();
        for (int i = 0; i < document.Gumps.Length; i++)
        {
            GumpLayoutDocument source = document.Gumps[i];
            if (string.Equals(source.Kind, "spell", StringComparison.OrdinalIgnoreCase))
            {
                SpellIconLayout layoutItem = new();
                layoutItem.Offset = new Point(source.X, source.Y);
                typeof(SpellIconLayout).GetField("m_SpellID", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(layoutItem, source.Id);
                gumps.Add(layoutItem);
            }
            else if (string.Equals(source.Kind, "skill", StringComparison.OrdinalIgnoreCase))
            {
                SkillIconLayout layoutItem = new();
                layoutItem.Offset = new Point(source.X, source.Y);
                typeof(SkillIconLayout).GetField("m_SkillID", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(layoutItem, source.Id);
                gumps.Add(layoutItem);
            }
        }

        layout.Gumps = gumps;
    }

    private static RectangleDocument Capture(Rectangle rectangle)
    {
        return new RectangleDocument
        {
            X = rectangle.X,
            Y = rectangle.Y,
            Width = rectangle.Width,
            Height = rectangle.Height
        };
    }

    private static Rectangle Restore(RectangleDocument document)
    {
        return new Rectangle(document.X, document.Y, document.Width, document.Height);
    }

    private static SizeDocument Capture(Size size)
    {
        return new SizeDocument
        {
            Width = size.Width,
            Height = size.Height
        };
    }

    private static Size Restore(SizeDocument document)
    {
        return new Size(document.Width, document.Height);
    }

    private static ItemRefDocument[] Capture(ItemRefCollection items)
    {
        List<ItemRefDocument> documents = new List<ItemRefDocument>();
        foreach (ItemRef item in items)
        {
            documents.Add(new ItemRefDocument
            {
                Serial = item.Serial,
                ItemId = item.ItemID
            });
        }

        return documents.ToArray();
    }

    private static ItemRef[] RestoreItems(ItemRefDocument[] items)
    {
        if (items == null || items.Length == 0)
        {
            return Array.Empty<ItemRef>();
        }

        ItemRef[] restored = new ItemRef[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            ItemRef item = new ItemRef(items[i].ItemId);
            item.Serial = items[i].Serial;
            restored[i] = item;
        }

        return restored;
    }

    private static int GetFlags(Options options)
    {
        int flags = 0;
        foreach (OptionFlag flag in Enum.GetValues(typeof(OptionFlag)))
        {
            if (flag == OptionFlag.None || flag == OptionFlag.Default)
            {
                continue;
            }

            if (options[flag])
            {
                flags |= (int)flag;
            }
        }

        return flags;
    }
}

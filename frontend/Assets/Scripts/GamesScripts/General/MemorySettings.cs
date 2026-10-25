using System;
using System.Linq;
using UnityEngine;

/// <summary>Account-scoped preferences. A running round snapshots them in CardFaces.</summary>
public enum DeckDisplay { Cards, Words, Decode, Random, Images }

public static class MemorySettings
{
    public static bool Practice {get{return PlayerPrefs.GetInt(Key("practice"),0)==1;}set{PlayerPrefs.SetInt(Key("practice"),value?1:0);PlayerPrefs.Save();}}
    public static DeckDisplay DeckMode {
        get { return (DeckDisplay)Mathf.Clamp(PlayerPrefs.GetInt(Key("deckMode"),0),0,4); }
        set { PlayerPrefs.SetInt(Key("deckMode"),(int)value); PlayerPrefs.Save(); }
    }
    private static string Key(string field) { return "memory.settings.v1." + GameManager.AchievementUserId + "." + field; }
    public static int Digits
    {
        get { return PlayerPrefs.GetInt(Key("digits"), 2) == 3 ? 3 : 2; }
        set { PlayerPrefs.SetInt(Key("digits"), value == 3 ? 3 : 2); PlayerPrefs.Save(); }
    }
    public static bool ShowImageNumbers
    {
        get { return PlayerPrefs.GetInt(Key("imageNumbers"), 0) == 1; }
        set { PlayerPrefs.SetInt(Key("imageNumbers"), value ? 1 : 0); PlayerPrefs.Save(); }
    }
    public static bool ShowWordNumbers
    {
        get { return PlayerPrefs.GetInt(Key("wordNumbers"), 0) == 1; }
        set { PlayerPrefs.SetInt(Key("wordNumbers"), value ? 1 : 0); PlayerPrefs.Save(); }
    }
}

public static class PracticeMode {
    // Competitive entry points always override the saved training preference.
    public static bool Untimed {get{return MemorySettings.Practice&&!SprintRuntime.Active&&!Assets.Script.Constants.ChallengeConstants.s_isChallenge;}}
}

public static class MajorWordCatalog
{
    private static string[] words;
    public const int Count = 1000;
    public static string Word(int code)
    {
        if (code < 0 || code >= Count) throw new ArgumentOutOfRangeException("code");
        if (words == null)
        {
            var asset = Resources.Load<TextAsset>("MajorSystem1000");
            if (asset == null) throw new InvalidOperationException("Missing 000-999 word list.");
            var parsed = new string[Count];
            foreach (var line in asset.text.Split(new[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries))
            {
                int n;
                if (line.Length < 5 || !int.TryParse(line.Substring(0,3), out n) || n < 0 || n >= Count || parsed[n] != null)
                    throw new InvalidOperationException("Invalid or duplicate Major word row.");
                parsed[n] = line.Substring(4).Trim();
            }
            for (int i=0; i<Count; i++) if (String.IsNullOrEmpty(parsed[i])) throw new InvalidOperationException("Missing Major word: " + i);
            words = parsed;
        }
        return words[code];
    }
}

public static class DeckCatalog
{
    public struct Entry { public string Suit, Rank, Code, Word; }
    public const int Count = 52;
    private static Entry[] entries;
    public static Entry Get(int index)
    {
        if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException("index");
        if (entries == null)
        {
            var asset = Resources.Load<TextAsset>("DeckWords");
            if (asset == null) throw new InvalidOperationException("Missing deck word list.");
            var lines = asset.text.Split(new[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != Count) throw new InvalidOperationException("Expected 52 deck entries.");
            entries = new Entry[Count];
            for (int i=0; i<Count; i++)
            {
                var cells=lines[i].Split('|');
                if (cells.Length != 4) { entries=null; throw new InvalidOperationException("Invalid deck entry."); }
                entries[i]=new Entry { Suit=cells[0], Rank=cells[1], Code=cells[2], Word=cells[3] };
            }
        }
        return entries[index];
    }
    public static string Decode(int index) {
        var e=Get(index);
        string prefix=e.Suit=="clubs"?"K":e.Suit=="diamonds"?"D":e.Suit=="spades"?"S":"H";
        string rank=e.Rank=="A"?"1":e.Rank=="10"?"0":e.Rank=="J"?"11":e.Rank=="Q"?"12":e.Rank=="K"?"13":e.Rank;
        const string letters="STNMRLJKFP";
        string sounds=String.Join("+",System.Linq.Enumerable.Select(rank,c=>letters[c-'0'].ToString()).ToArray());
        if(e.Suit=="hearts" && e.Rank=="K")return "HK: T+M = 13";
        return (e.Suit=="clubs"?"C":e.Suit=="diamonds"?"D":e.Suit=="hearts"?"H":"S")+e.Rank+": "+prefix+"+"+sounds+" = "+e.Code;
    }
    public static Sprite Mnemonic(int index) { var e=Get(index);return Resources.Load<Sprite>("DeckMnemonics/"+e.Suit+"_"+e.Rank); }
    public static Sprite Face(int index)
    {
        var e=Get(index);
        var sprite=Resources.Load<Sprite>("PlayingCards/"+e.Suit+"_"+e.Rank);
        if (sprite==null) throw new InvalidOperationException("Missing playing card: "+e.Suit+e.Rank);
        MajorSystemImages.SetVisibleSize(sprite, new Vector2(.72f,.96f));
        return sprite;
    }
    public static Sprite CompactFace(int index)
    {
        var e=Get(index);
        return Resources.Load<Sprite>("PlayingCardsCompact/"+e.Suit+"_"+e.Rank);
    }
}

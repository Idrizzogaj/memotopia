using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using UnityEngine;

public enum CardKind { Image, Number, Word, PlayingCard }

/// <summary>
/// What a card shows in Pairs, Flash and Boxes: the Major System picture, its number (00-99) or a word.
/// The player chooses in the content bar on the levels screen; Words, Numbers and Random unlock with the
/// Memory Athlete badge, Help (number + sound letters on the pictures) with the Major System tip.
/// Number and word faces are drawn at run time into ordinary sprites, so the games treat them like pictures.
/// </summary>
public static class CardFaces
{
    [Flags]
    public enum Choice { None = 0, Images = 1, Help = 2, Words = 4, Numbers = 8, Random = 16, Deck = 32 }

    public const string HelpKey = "tip-major-system";
    private const string DigitLetters = "STNMRLJKFP"; // Major System sounds: 0=S 1=T 2=N 3=M 4=R 5=L 6=J 7=K 8=F 9=P
    private const int Size = 256;

    private static readonly Color PurpleInk = new Color32(123, 31, 255, 255);
    private static readonly Color DarkInk = new Color32(43, 43, 58, 255);
    private static readonly Color GreyInk = new Color32(110, 110, 125, 255);

    // ---------- the player's choice (saved per account)

    private static string PrefsKey { get { return "content.choice.v1." + GameManager.AchievementUserId; } }

    public static bool AdvancedUnlocked { get { return MemoryAthlete.Unlocked; } }
    public static bool HelpUnlocked { get { return GameManager.completedAchievements.Contains(HelpKey); } }

    public static Choice Selected
    {
        get
        {
            int raw = (int)Choice.Images;
            try { raw = PlayerPrefs.GetInt(PrefsKey, raw); } catch (Exception) { }
            return Sanitize((Choice)raw);
        }
        set
        {
            try { PlayerPrefs.SetInt(PrefsKey, (int)Sanitize(value)); PlayerPrefs.Save(); } catch (Exception) { }
        }
    }

    public static Choice Sanitize(Choice c)
    {
        if (!AdvancedUnlocked) c &= ~(Choice.Words | Choice.Numbers | Choice.Random);
        if (!HelpUnlocked) c &= ~Choice.Help;
        if ((c & Choice.Deck) != 0) return Choice.Deck;
        // There are 100 images, not 1,000. Never relabel an image as a different three-digit code.
        if (MemorySettings.Digits == 3 && AdvancedUnlocked)
        {
            if ((c & Choice.Images) != 0) c = (c & ~Choice.Images) | Choice.Words;
        }
        if ((c & (Choice.Images | Choice.Words | Choice.Numbers | Choice.Random)) == 0)
            c |= MemorySettings.Digits == 3 && AdvancedUnlocked ? Choice.Words : Choice.Images;
        return c;
    }

    public static string Describe(Choice c)
    {
        if ((c & Choice.Deck) != 0) return "The Deck (52 cards)";
        var parts = new List<string>();
        if ((c & Choice.Random) != 0) parts.Add("Random mix");
        else
        {
            if ((c & Choice.Images) != 0) parts.Add("Images");
            if ((c & Choice.Words) != 0) parts.Add("Words");
            if ((c & Choice.Numbers) != 0) parts.Add("Numbers");
        }
        if ((c & Choice.Help) != 0) parts.Add("with Help");
        return string.Join(" + ", parts.ToArray());
    }

    // ---------- one game round

    private static bool active, help, imageNumbers, wordNumbers, deck, sprint;
    private static readonly int[] sprintDeckIndex=new int[MajorSystemImages.Count+1];
    private static int digits = 2;
    private static DeckDisplay deckMode;
    private static readonly DeckDisplay[] deckFaces = new DeckDisplay[MajorSystemImages.Count + 1];
    private static readonly int[] numberOf = new int[MajorSystemImages.Count + 1];
    private static readonly int[] wordCode = new int[MajorSystemImages.Count + 1];
    public static int ContentCount { get { return active && deck ? DeckCatalog.Count : MajorSystemImages.Count; } }
    public static string NumberText(int id) { return numberOf[id].ToString(digits == 3 ? "D3" : "D2"); }
    public static int WordCode(int id) { return wordCode[id]; }
    private static readonly CardKind[] kinds = new CardKind[MajorSystemImages.Count + 1];
    private static readonly string[] wordOf = new string[MajorSystemImages.Count + 1];
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
    private static string[] imageWords;

    /// <summary>Call when a game round is set up. Challenges always use pictures, for now.</summary>
    public static void BeginSession()
    {
        if(SprintRuntime.Active){BeginSprintSession(SprintRuntime.Current.Battle.tier,SprintRuntime.Current.StageSeed);return;}
        sprint=false;
        ClearCache();
        var c = ChallengeConstants.s_isChallenge ? Choice.Images : Selected;
        bool challenge = ChallengeConstants.s_isChallenge;
        deck = (c & Choice.Deck) != 0 && !challenge;
        digits = !challenge && AdvancedUnlocked ? MemorySettings.Digits : 2;
        imageNumbers = !challenge && MemorySettings.ShowImageNumbers;
        wordNumbers = !challenge && MemorySettings.ShowWordNumbers;
        help = (c & Choice.Help) != 0 && !challenge;
        if (deck)
        {
            deckMode=MemorySettings.DeckMode;
            for (int id=1; id<=DeckCatalog.Count; id++) {
                deckFaces[id]=deckMode==DeckDisplay.Random ? new[]{DeckDisplay.Cards,DeckDisplay.Images,DeckDisplay.Decode,DeckDisplay.Words}[UnityEngine.Random.Range(0,4)] : deckMode;
                kinds[id]=deckFaces[id]==DeckDisplay.Cards ? CardKind.PlayingCard : deckFaces[id]==DeckDisplay.Words ? CardKind.Word : CardKind.Image;
                wordOf[id]=DeckCatalog.Get(id-1).Word;
            }
            active=true;
            return;
        }
        var allowed = new List<CardKind>();
        if ((c & Choice.Random) != 0)
        {
            if (digits == 2) allowed.Add(CardKind.Image);
            if (AdvancedUnlocked) { allowed.Add(CardKind.Word); allowed.Add(CardKind.Number); }
        }
        else
        {
            if ((c & Choice.Images) != 0) allowed.Add(CardKind.Image);
            if ((c & Choice.Words) != 0) allowed.Add(CardKind.Word);
            if ((c & Choice.Numbers) != 0) allowed.Add(CardKind.Number);
        }
        if (allowed.Count == 0) allowed.Add(CardKind.Image);

        var pool = new int[MajorWordCatalog.Count];
        for (int i=0; i<pool.Length; i++) pool[i]=i;
        for (int i=pool.Length-1; i>0; i--)
        {
            int j=UnityEngine.Random.Range(0,i+1);
            int tmp=pool[i]; pool[i]=pool[j]; pool[j]=tmp;
        }
        for (int id=1; id<=MajorSystemImages.Count; id++)
        {
            kinds[id]=allowed[UnityEngine.Random.Range(0,allowed.Count)];
            numberOf[id]=digits==3 ? pool[id-1] : id-1;
            wordCode[id]=numberOf[id];
            wordOf[id]=digits==3 ? MajorWordCatalog.Word(wordCode[id]) : ImageWord(id);
        }
        active = true;
    }

    public static void BeginSprintSession(int tier,int seed) {
        ClearCache();sprint=active=true;deck=help=imageNumbers=wordNumbers=false;digits=2;
        int count=ChallengeSprintRules.KindCount(tier);
        var rng=new SprintRandom(seed);
        var types=System.Linq.Enumerable.Range(0,MajorSystemImages.Count).Select(i=>i%count).ToArray();rng.Shuffle(types);
        var cards=System.Linq.Enumerable.Range(0,52).ToArray();rng.Shuffle(cards);int card=0;
        var numbers=System.Linq.Enumerable.Range(0,100).ToArray();rng.Shuffle(numbers);
        for(int id=1;id<=MajorSystemImages.Count;id++){
            kinds[id]=new[]{CardKind.Image,CardKind.Word,CardKind.Number,CardKind.PlayingCard}[types[id-1]];
            numberOf[id]=numbers[id-1];wordCode[id]=id-1;wordOf[id]=ImageWord(id);
            if(kinds[id]==CardKind.PlayingCard)sprintDeckIndex[id]=cards[card++];
        }
    }
    public static void EndSession() { active = false; deck = sprint = false; ClearCache(); }

    private static void ClearCache()
    {
        foreach (var sprite in cache.Values)
        {
            if (sprite == null) continue;
            MajorSystemImages.ForgetVisibleSize(sprite);
            if (sprite.texture != null) DestroyGenerated(sprite.texture);
            DestroyGenerated(sprite);
        }
        cache.Clear();
    }
    internal static void DestroyGenerated(UnityEngine.Object obj)
    {
        if (Application.isPlaying) UnityEngine.Object.Destroy(obj);
        else UnityEngine.Object.DestroyImmediate(obj);
    }

    public static CardKind KindOf(int id) { return active && id >= 1 && id <= MajorSystemImages.Count ? kinds[id] : CardKind.Image; }

    // ---------- faces

    public static Sprite Face(int id)
    {
        if (!active || id < 1 || id > MajorSystemImages.Count) return MajorSystemImages.Load(id);
        try
        {
            switch (kinds[id])
            {
                case CardKind.PlayingCard: return DeckCatalog.Face(sprint?sprintDeckIndex[id]:id-1);
                case CardKind.Number: return NumberCard(id);
                case CardKind.Word: return deck ? DeckWordCard(id) : WordCard(id);
                default: if(deck)return DeckImage(id); return help || imageNumbers ? HelpImage(id) : MajorSystemImages.Load(id);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Card face fell back to the picture: " + e.Message);
            return MajorSystemImages.Load(id);
        }
    }

    /// <summary>Pairs stores cards as "MajorSystem/NN" resource paths.</summary>
    public static Sprite FaceForPath(string path)
    {
        const string prefix = "MajorSystem/";
        int n;
        if (path != null && path.StartsWith(prefix) && int.TryParse(path.Substring(prefix.Length), out n)) return Face(n + 1);
        return MajorSystemImages.LoadPath(path);
    }

    /// <summary>Text only, wide and transparent: used where the picture area is small and wide (Flash tiles).</summary>
    public static Sprite WideFace(int id)
    {
        if(active && sprint && kinds[id]==CardKind.PlayingCard)return DeckCatalog.CompactFace(sprintDeckIndex[id]);
        if (active && deck && id >= 1 && id <= DeckCatalog.Count && kinds[id]==CardKind.PlayingCard) return DeckCatalog.CompactFace(id - 1);
        if (!active || id < 1 || id > MajorSystemImages.Count || kinds[id] == CardKind.Image || kinds[id] == CardKind.PlayingCard) return Face(id);
        try
        {
            bool number = kinds[id] == CardKind.Number;
            string text = number ? NumberText(id) : wordOf[id];
            string sub = deck ? (deckMode==DeckDisplay.Decode ? DeckLabel(id) : null) : (!number && wordNumbers ? wordCode[id].ToString(digits==3 ? "D3" : "D2") : null);
            string key = "wide:" + text + ":" + sub;
            Sprite sprite;
            if (cache.TryGetValue(key, out sprite)) return sprite;
            const int S = 512, H = 154; // 512 x 154 strip cut out of the middle of a square rendering
            float[] cov = TextRaster.Coverage(text, S, 0.96f, sub == null ? 0.29f : 0.17f, 0.5f, sub == null ? 0.5f : 0.57f, true);
            float[] subCov = sub == null ? null : TextRaster.Coverage(sub, S, 0.35f, 0.08f, 0.5f, 0.42f, true);
            var px = new Color32[S * H];
            Color ink = number ? PurpleInk : DarkInk;
            int offset = (S - H) / 2;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < S; x++)
                {
                    float a = cov[(y + offset) * S + x];
                    if (a > 0.003f) px[y * S + x] = new Color32((byte)(ink.r * 255f), (byte)(ink.g * 255f), (byte)(ink.b * 255f), (byte)(a * 255f));
                    if (subCov != null && subCov[(y + offset) * S + x] > .003f)
                        px[y * S + x] = new Color32(123,31,255,(byte)(subCov[(y + offset) * S + x] * 255f));
                }
            sprite = MakeSprite(px, S, H, 100f);
            MajorSystemImages.SetVisibleSize(sprite, Vector2.one);
            cache[key] = sprite;
            return sprite;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Wide card face fell back: " + e.Message);
            return Face(id);
        }
    }

    public static Sprite SmallFace(int id) { return active && (deck || sprint) ? WideFace(id) : Face(id); }
    private static string DeckLabel(int id) {
        var e=DeckCatalog.Get(id-1);
        return DeckCatalog.Decode(id-1);
    }
    private static Sprite DeckWordCard(int id) {
        string sub=deckMode==DeckDisplay.Decode ? DeckLabel(id) : null;
        string key="deck:"+id+":"+sub;
        Sprite sprite;
        if(cache.TryGetValue(key,out sprite))return sprite;
        sprite=BuildCard(wordOf[id],sub,DarkInk,GreyInk,false);
        cache[key]=sprite;return sprite;
    }

    private static Sprite DeckImage(int id) {
        var source=DeckCatalog.Mnemonic(id-1);
        if(source==null)throw new InvalidOperationException("Missing deck illustration: "+id);
        if(deckFaces[id]!=DeckDisplay.Decode)return source;
        string key="deck-image-help:"+id;
        Sprite result;if(cache.TryGetValue(key,out result))return result;
        int w=source.texture.width,h=source.texture.height;
        var pixels=ReadPixels(source.texture);
        FillRounded(pixels,w,h,.02f,.01f,.98f,.24f,.04f,Color.white,Color.white);
        Blend(pixels,TextRaster.Coverage(DeckCatalog.Decode(id-1),w,.92f,.09f,.5f,.12f,true),PurpleInk);
        result=MakeSprite(pixels,w,h,source.pixelsPerUnit);
        MajorSystemImages.SetVisibleSize(result,Vector2.one);cache[key]=result;return result;
    }

    private static Sprite NumberCard(int id)
    {
        string number = NumberText(id);
        string key = "n" + number + (help ? "h" : "");
        Sprite sprite;
        if (cache.TryGetValue(key, out sprite)) return sprite;
        sprite = BuildCard(number, help ? (digits == 3 ? MajorWordCatalog.Word(numberOf[id]) : ImageWord(id)) : null, PurpleInk, GreyInk, true);
        cache[key] = sprite;
        return sprite;
    }

    private static Sprite WordCard(int id)
    {
        string word = wordOf[id];
        string key = "w" + word;
        Sprite sprite;
        if (cache.TryGetValue(key, out sprite)) return sprite;
        sprite = BuildCard(word, wordNumbers ? wordCode[id].ToString(digits==3 ? "D3" : "D2") : null, DarkInk, GreyInk, false);
        cache[key] = sprite;
        return sprite;
    }

    private static Sprite HelpImage(int id)
    {
        string key = "h" + id;
        Sprite sprite;
        if (cache.TryGetValue(key, out sprite)) return sprite;
        Sprite source = MajorSystemImages.Load(id);
        Texture2D src = source.texture;
        int w = src.width, h = src.height;
        Color32[] px = ReadPixels(src);

        // small label in the bottom right corner: the number and its sound letters
        string number = (id - 1).ToString("D2");
        string letters = help ? SoundLetters(id) : "";
        FillRounded(px, w, h, 0.50f, 0.02f, 0.98f, 0.32f, 0.07f, new Color(1f, 1f, 1f, 0.92f), new Color(0.80f, 0.74f, 1f, 1f));
        Blend(px, TextRaster.Coverage(number, w, 0.30f, 0.15f, 0.74f, 0.22f, true), PurpleInk);
        Blend(px, TextRaster.Coverage(letters, w, 0.28f, 0.075f, 0.74f, 0.095f, true), DarkInk);

        sprite = MakeSprite(px, w, h, source.pixelsPerUnit);
        MajorSystemImages.SetVisibleSize(sprite, MajorSystemImages.VisibleSize(source));
        cache[key] = sprite;
        return sprite;
    }

    private static Sprite BuildCard(string main, string sub, Color mainInk, Color subInk, bool big)
    {
        var px = new Color32[Size * Size];
        FillRounded(px, Size, Size, 0.04f, 0.04f, 0.96f, 0.96f, 0.14f, Color.white, new Color(0.80f, 0.74f, 1f, 1f));
        float mainFit = big ? (sub != null ? 0.40f : 0.52f) : 0.30f;
        Blend(px, TextRaster.Coverage(main, Size, 0.78f, mainFit, 0.5f, sub != null ? 0.60f : 0.50f, true), mainInk);
        if (sub != null) Blend(px, TextRaster.Coverage(sub, Size, 0.70f, 0.14f, 0.5f, 0.20f, true), subInk);
        var sprite = MakeSprite(px, Size, Size, 100f);
        MajorSystemImages.SetVisibleSize(sprite, new Vector2(0.88f, 0.88f));
        return sprite;
    }

    // ---------- drawing helpers

    /// <summary>
    /// Reads the pixels of a texture that may be marked non-readable (the picture sprites are), by copying it
    /// through a temporary render texture.
    /// </summary>
    public static Color32[] ReadPixels(Texture2D source)
    {
        if (source.isReadable) return source.GetPixels32();
        var target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, target);
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(target);
        Color32[] pixels = copy.GetPixels32();
        DestroyGenerated(copy);
        return pixels;
    }

    private static Sprite MakeSprite(Color32[] px, int w, int h, float pixelsPerUnit)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels32(px);
        tex.Apply(false);
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    /// <summary>Alpha-blends a rounded rectangle (fractions of the texture) with a border over the pixels.</summary>
    private static void FillRounded(Color32[] px, int w, int h, float x0, float y0, float x1, float y1, float radiusFraction, Color fill, Color border)
    {
        float left = x0 * w, right = x1 * w, bottom = y0 * h, top = y1 * h;
        float r = radiusFraction * Mathf.Min(w, h);
        float cx = (left + right) * 0.5f, cy = (bottom + top) * 0.5f;
        float hx = (right - left) * 0.5f - r, hy = (top - bottom) * 0.5f - r;
        float borderWidth = Mathf.Max(2f, Mathf.Min(w, h) * 0.02f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x + 0.5f - cx) - hx, 0f), dy = Mathf.Max(Mathf.Abs(y + 0.5f - cy) - hy, 0f);
                float d = Mathf.Sqrt(dx * dx + dy * dy) - r; // < 0 inside
                float inside = Mathf.Clamp01(0.5f - d);
                if (inside <= 0f) continue;
                float edge = Mathf.Clamp01((d + borderWidth) * 0.5f + 0.5f); // 1 at the border, 0 in the middle
                Color c = Color.Lerp(fill, border, edge);
                c.a *= inside;
                px[y * w + x] = Over(px[y * w + x], c, 1f);
            }
    }

    private static Color32 Over(Color32 dst, Color src, float coverage)
    {
        float a = src.a * coverage;
        if (a <= 0.003f) return dst;
        float da = dst.a / 255f;
        float outA = a + da * (1f - a);
        float r = (src.r * a + dst.r / 255f * da * (1f - a)) / outA;
        float g = (src.g * a + dst.g / 255f * da * (1f - a)) / outA;
        float b = (src.b * a + dst.b / 255f * da * (1f - a)) / outA;
        return new Color32((byte)(r * 255f + 0.5f), (byte)(g * 255f + 0.5f), (byte)(b * 255f + 0.5f), (byte)(outA * 255f + 0.5f));
    }

    private static void Blend(Color32[] px, float[] coverage, Color ink)
    {
        for (int i = 0; i < px.Length && i < coverage.Length; i++)
            if (coverage[i] > 0.003f) px[i] = Over(px[i], ink, coverage[i]);
    }

    // ---------- data

    private static string[] soundLetters;

    /// <summary>The consonant sounds of the picture's word as spelled in it, for example "T D" for TOAD (11).</summary>
    public static string SoundLetters(int id)
    {
        if (soundLetters == null)
        {
            soundLetters = new string[MajorSystemImages.Count + 1];
            var asset = Resources.Load<TextAsset>("MajorSystemSounds");
            if (asset != null)
                foreach (var line in asset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int n;
                    if (line.Length > 3 && int.TryParse(line.Substring(0, 2), out n) && n >= 0 && n < MajorSystemImages.Count)
                        soundLetters[n + 1] = line.Substring(3).Trim();
                }
        }
        if (id >= 1 && id <= MajorSystemImages.Count && !string.IsNullOrEmpty(soundLetters[id])) return soundLetters[id];
        string number = (id - 1).ToString("D2");
        return DigitLetters[number[0] - '0'] + " " + DigitLetters[number[1] - '0'];
    }

    /// <summary>The Major System word of a picture, for example "SIGN" for 02.</summary>
    public static string ImageWord(int id)
    {
        if (imageWords == null)
        {
            imageWords = new string[MajorSystemImages.Count + 1];
            var asset = Resources.Load<TextAsset>("MajorSystemNames");
            if (asset != null)
                foreach (var line in asset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int n;
                    if (line.Length > 3 && int.TryParse(line.Substring(0, 2), out n) && n >= 0 && n < MajorSystemImages.Count)
                        imageWords[n + 1] = line.Substring(3).Trim();
                }
        }
        return id >= 1 && id <= MajorSystemImages.Count ? (imageWords[id] ?? "") : "";
    }
}

/// <summary>Draws text with Unity's built-in font into a coverage map, using a hidden TextMesh and camera.</summary>
internal static class TextRaster
{
    private const int Layer = 31;
    private static GameObject root;
    private static Camera cam;
    private static TextMesh mesh;
    private static MeshRenderer meshRenderer;

    private static void EnsureRig()
    {
        if (root != null) return;
#if UNITY_2022_2_OR_NEWER
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        root = new GameObject("~TextRaster");
        root.hideFlags = HideFlags.HideAndDontSave;
        root.transform.position = new Vector3(0f, -50000f, 0f);
        var camObject = new GameObject("cam");
        camObject.hideFlags = HideFlags.HideAndDontSave;
        camObject.transform.SetParent(root.transform, false);
        camObject.transform.localPosition = new Vector3(0f, 0f, -10f);
        camObject.layer = Layer;
        cam = camObject.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 0.5f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 30f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.cullingMask = 1 << Layer;
        cam.allowMSAA = false;
        cam.enabled = false;
        var textObject = new GameObject("text");
        textObject.hideFlags = HideFlags.HideAndDontSave;
        textObject.transform.SetParent(root.transform, false);
        textObject.layer = Layer;
        mesh = textObject.AddComponent<TextMesh>();
        mesh.font = font;
        mesh.fontSize = 120;
        mesh.characterSize = 0.01f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = Color.white;
        meshRenderer = textObject.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = font.material;
    }

    /// <summary>
    /// Returns size*size coverage (0..1, row 0 at the bottom). The text is scaled to fit inside
    /// fitW x fitH (fractions of the square) and centred at (cx, cy) (fractions, origin bottom left).
    /// </summary>
    public static float[] Coverage(string text, int size, float fitW, float fitH, float cx, float cy, bool bold)
    {
        EnsureRig();
        mesh.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        mesh.text = text;
        mesh.transform.localScale = Vector3.one;
        mesh.transform.localPosition = Vector3.zero;
        var target = RenderTexture.GetTemporary(size, size, 16, RenderTextureFormat.ARGB32);
        cam.targetTexture = target;
        cam.Render(); // builds the mesh so that its bounds are valid
        Bounds bounds = meshRenderer.bounds;
        float bw = Mathf.Max(bounds.size.x, 0.0001f), bh = Mathf.Max(bounds.size.y, 0.0001f);
        float scale = Mathf.Min(fitW / bw, fitH / bh);
        Vector3 centre = bounds.center - root.transform.position;
        mesh.transform.localScale = Vector3.one * scale;
        mesh.transform.localPosition = new Vector3((cx - 0.5f) - centre.x * scale, (cy - 0.5f) - centre.y * scale, 0f);
        cam.Render();

        var previous = RenderTexture.active;
        RenderTexture.active = target;
        var readback = new Texture2D(size, size, TextureFormat.RGBA32, false);
        readback.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        RenderTexture.active = previous;
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(target);
        Color32[] pixels = readback.GetPixels32();
        CardFaces.DestroyGenerated(readback);

        var coverage = new float[size * size];
        for (int i = 0; i < coverage.Length; i++) coverage[i] = pixels[i].r / 255f;
        return coverage;
    }
}

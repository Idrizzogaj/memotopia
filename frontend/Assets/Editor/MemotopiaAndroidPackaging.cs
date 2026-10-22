using System.IO;
using UnityEditor.Android;

// Unity 2022 uses AGP 7.4.2. Android's documented fallback for AGP < 8.5.1
// is compressed native libraries, which Android extracts on installation.
// ELF segment alignment is checked independently in the finished AAB.
public sealed class MemotopiaAndroidPackaging : IPostGenerateGradleAndroidProject
{
    public int callbackOrder { get { return 10000; } }

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string root = Directory.GetParent(path).FullName;
        ConfigureLauncherIcon(path, root);
        // Kotlin 1.9 libraries require D8/R8 >= 8.0.27; Unity's AGP 7.4.2 embeds an older version.
        string settingsFile = Path.Combine(root, "settings.gradle");
        string settings = File.ReadAllText(settingsFile);
        if (!settings.Contains("com.android.tools:r8:8.0.40"))
        {
            const string header = "pluginManagement {";
            if (!settings.Contains(header)) throw new System.Exception("Missing Gradle pluginManagement block.");
            settings = settings.Replace(header, header + "\n    buildscript {\n        repositories { google(); mavenCentral() }\n        dependencies { classpath 'com.android.tools:r8:8.0.40' }\n    }\n");
            File.WriteAllText(settingsFile, settings);
        }
        const string marker = "// Memotopia: compressed native libraries for 16 KB devices";
        foreach (string module in new[] { path, Path.Combine(root, "launcher") })
        {
            string file = Path.Combine(module, "build.gradle");
            string content = File.ReadAllText(file);
            if (!content.Contains(marker))
                File.AppendAllText(file, "\n" + marker + "\nandroid { packagingOptions { jniLibs { useLegacyPackaging true } } }\n");
        }
        string properties = Path.Combine(root, "gradle.properties");
        string text = File.ReadAllText(properties);
        text = System.Text.RegularExpressions.Regex.Replace(text,
            @"(?m)^android\.bundle\.enableUncompressedNativeLibs=.*\r?\n?", "");
        File.WriteAllText(properties, text + "\nandroid.bundle.enableUncompressedNativeLibs=false\n");
    }
    private static void ConfigureLauncherIcon(string path, string root)
    {
        string source = Path.Combine(UnityEngine.Application.dataPath, "Resources/icons/Untitled-9-13.png");
        if (!File.Exists(source)) throw new System.Exception("Memotopia launcher artwork is missing.");
        string res = Path.Combine(path, "src/main/res");
        Directory.CreateDirectory(Path.Combine(res, "mipmap-nodpi"));
        Directory.CreateDirectory(Path.Combine(res, "drawable-nodpi"));
        Directory.CreateDirectory(Path.Combine(res, "mipmap-anydpi-v26"));
        File.Copy(source, Path.Combine(res, "mipmap-nodpi/memotopia_launcher.png"), true);
        File.Copy(source, Path.Combine(res, "drawable-nodpi/memotopia_launcher_art.png"), true);
        File.WriteAllText(Path.Combine(res, "mipmap-anydpi-v26/memotopia_launcher.xml"),
            "<adaptive-icon xmlns:android=\"http://schemas.android.com/apk/res/android\">" +
            "<background android:drawable=\"@android:color/white\"/>" +
            "<foreground><inset android:drawable=\"@drawable/memotopia_launcher_art\" android:inset=\"18%\"/></foreground>" +
            "</adaptive-icon>");
        foreach (string module in new[] { path, Path.Combine(root, "launcher") })
        {
            string manifest = Path.Combine(module, "src/main/AndroidManifest.xml");
            if (!File.Exists(manifest)) continue;
            var document = System.Xml.Linq.XDocument.Load(manifest);
            System.Xml.Linq.XNamespace android = "http://schemas.android.com/apk/res/android";
            var application = document.Root.Element("application");
            if (application == null) continue;
            application.SetAttributeValue(android + "icon", "@mipmap/memotopia_launcher");
            application.SetAttributeValue(android + "roundIcon", "@mipmap/memotopia_launcher");
            document.Save(manifest);
        }
    }}

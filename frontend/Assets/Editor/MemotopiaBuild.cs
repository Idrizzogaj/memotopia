using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.Linq;
using System.IO;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class MemotopiaBuild
{
#if UNITY_IOS
    public static void BuildIOS()
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.retention.memotopia");
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.appleDeveloperTeamID = "E838WX6KF8";
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        PlayerSettings.bundleVersion = "1.0.39";
        PlayerSettings.iOS.buildNumber = "2026091707";

        MajorSystemValidation.Validate();
        AchievementValidation.Validate();
        FlowValidation.Validate();

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && !s.path.EndsWith("/SplashScene.unity"))
            .Select(s => s.path)
            .ToArray();

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = Path.GetFullPath(Path.Combine(Application.dataPath, "../../ios-build/IzFreshBuild"));
        options.target = BuildTarget.iOS;
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception("iOS build failed: " + report.summary.result);
        }

        ConfigureXcodeExport(options.locationPathName);
    }
#endif

    /// Debug-signed APK for local sideload testing (adb install). Not for Play Store release —
    /// that needs a real upload keystore, which does not exist yet for this app.
    /// Run with "-buildTarget Android" on the command line so script compilation already
    /// has the right platform defines before this method runs.
    public static void BuildAndroid()
    {
        // The bundled OpenJDK module failed to install (blocked by a Windows elevation
        // prompt). Unity 2019.3's Android tooling needs JDK 8 specifically (newer JDKs
        // lack lib/tools.jar and aren't recognized), so point at a standalone JDK 8.
        EditorPrefs.SetString("JdkPath", @"C:\Users\IdrZog_MEM\.jdk8\jdk8u504-b01");

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.zogajretention.memotopia");
        PlayerSettings.bundleVersion = "1.0.39";
        // The project's real release keystore only exists on the Mac. Use Unity's
        // auto-generated debug keystore for this local sideload/test build instead.
        PlayerSettings.Android.useCustomKeystore = false;

        MajorSystemValidation.Validate();
        AchievementValidation.Validate();
        FlowValidation.Validate();

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && !s.path.EndsWith("/SplashScene.unity"))
            .Select(s => s.path)
            .ToArray();

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = Path.GetFullPath(Path.Combine(Application.dataPath, "../../android-build/Memotopia.apk"));
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception("Android build failed: " + report.summary.result);
        }
    }

    /// Signed .aab for the Play Console closed-test upload. Keystore path/passwords come from
    /// process environment variables (never hardcoded/committed) — set them before invoking Unity:
    /// MEMOTOPIA_ANDROID_KEYSTORE_PATH, MEMOTOPIA_ANDROID_KEY_ALIAS,
    /// MEMOTOPIA_ANDROID_STORE_PASS, MEMOTOPIA_ANDROID_KEY_PASS.
    public static void BuildAndroidRelease()
    {
        EditorPrefs.SetString("JdkPath", @"C:\Users\IdrZog_MEM\.jdk8\jdk8u504-b01");

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.zogajretention.memotopia");
        PlayerSettings.bundleVersion = "1.0.39";
        PlayerSettings.Android.bundleVersionCode = 1;
        // Play's automatic Store protection requires minSdkVersion >= 21 (Android 5.0).
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = System.Environment.GetEnvironmentVariable("MEMOTOPIA_ANDROID_KEYSTORE_PATH");
        PlayerSettings.Android.keystorePass = System.Environment.GetEnvironmentVariable("MEMOTOPIA_ANDROID_STORE_PASS");
        PlayerSettings.Android.keyaliasName = System.Environment.GetEnvironmentVariable("MEMOTOPIA_ANDROID_KEY_ALIAS");
        PlayerSettings.Android.keyaliasPass = System.Environment.GetEnvironmentVariable("MEMOTOPIA_ANDROID_KEY_PASS");

        if (string.IsNullOrEmpty(PlayerSettings.Android.keystoreName) || string.IsNullOrEmpty(PlayerSettings.Android.keystorePass))
        {
            throw new System.Exception("Android signing environment variables are not set.");
        }

        EditorUserBuildSettings.buildAppBundle = true;

        MajorSystemValidation.Validate();
        AchievementValidation.Validate();
        FlowValidation.Validate();

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && !s.path.EndsWith("/SplashScene.unity"))
            .Select(s => s.path)
            .ToArray();

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = Path.GetFullPath(Path.Combine(Application.dataPath, "../../android-build/Memotopia.aab"));
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception("Android release build failed: " + report.summary.result);
        }
    }

#if UNITY_IOS
    public static void ConfigureXcodeExport(string path)
    {
        string projectPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);
        foreach (string target in new[] { project.GetUnityMainTargetGuid(), project.GetUnityFrameworkTargetGuid() })
        {
            // Unity 2019 emits a 32-bit flag that current Xcode rejects for arm64.
            project.UpdateBuildProperty(target, "OTHER_CFLAGS", null, new[] { "-mno-thumb" });
            // Old IL2CPP delegate code fails under current Clang's strict aliasing.
            project.SetBuildProperty(target, "GCC_STRICT_ALIASING", "NO");
            project.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            project.SetBuildProperty(target, "IPHONEOS_DEPLOYMENT_TARGET", "15.0");
        }
        project.WriteToFile(projectPath);

        string symbolsPath = Path.Combine(path, "process_symbols.sh");
        string symbolsScript = File.ReadAllText(symbolsPath);
        const string marker = "# Skip symbol processing when no dSYM was generated.";
        if (!symbolsScript.Contains(marker))
        {
            string guard = marker + "\n" +
                "if [ ! -d \"$DWARF_DSYM_FOLDER_PATH/$DWARF_DSYM_FILE_NAME\" ]; then\n" +
                "    exit 0\nfi\n";
            File.WriteAllText(symbolsPath, symbolsScript.Replace("#!/bin/sh\n", "#!/bin/sh\n" + guard));
        }
    }
#endif
}

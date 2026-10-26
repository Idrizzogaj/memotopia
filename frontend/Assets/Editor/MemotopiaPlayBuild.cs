#if UNITY_ANDROID
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MemotopiaPlayBuild
{
    public static void Build()
    {
        string output = Required("MEMOTOPIA_AAB_OUTPUT");
        int versionCode = int.Parse(Required("MEMOTOPIA_VERSION_CODE"));
        if (versionCode <= 0 || versionCode > 2100000000)
            throw new Exception("Invalid Play version code.");
        string android = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/AndroidPlayer");
        AndroidExternalToolsSettings.jdkRootPath = Path.Combine(android, "OpenJDK");
        AndroidExternalToolsSettings.sdkRootPath = Path.Combine(android, "SDK");
        AndroidExternalToolsSettings.ndkRootPath = Path.Combine(android, "NDK");
        if (!File.Exists(Path.Combine(android, "SDK/platforms/android-36/android.jar")))
            throw new Exception("Android SDK API 36 is missing.");

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.zogajretention.memotopia");
        PlayerSettings.bundleVersion = "2.1.0";
        PlayerSettings.Android.bundleVersionCode = versionCode;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = true;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.allowDebugging = false;

        ReactionAssetImporter.Repair();
        SaysNativeValidation.Validate();
        MajorSystemValidation.Validate();
        ChainValidation.ValidateLogic();
        ChallengeSprintValidation.Run();
        AchievementValidation.Validate();
        FlowValidation.Validate();

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && !s.path.EndsWith("/SplashScene.unity"))
            .Select(s => s.path).ToArray();
        if (scenes.Length == 0) throw new Exception("No build scenes.");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        try
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = Required("MEMOTOPIA_ANDROID_KEYSTORE_PATH");
            PlayerSettings.Android.keystorePass = Credential("Store password");
            PlayerSettings.Android.keyaliasName = "memotopia";
            PlayerSettings.Android.keyaliasPass = Credential("Key password");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });
            string summary = "result=" + report.summary.result + "\n" +
                "package=" + PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) + "\n" +
                "versionName=" + PlayerSettings.bundleVersion + "\nversionCode=" + versionCode + "\n" +
                "unity=" + Application.unityVersion + "\ntargetSdk=36\nminSdk=24\narchitecture=ARM64\n" +
                "errors=" + report.summary.totalErrors + "\nwarnings=" + report.summary.totalWarnings + "\n" +
                "duration=" + report.summary.totalTime + "\n";
            File.WriteAllText(output + ".build-summary.txt", summary);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Play AAB build failed: " + report.summary.result);
            Debug.Log("MEMOTOPIA_AAB_BUILD_SUCCEEDED: " + output);
        }
        finally
        {
            PlayerSettings.Android.keystorePass = "";
            PlayerSettings.Android.keyaliasPass = "";
            AssetDatabase.SaveAssets();
        }
    }

    private static string Credential(string name)
    {
        foreach (string line in File.ReadAllLines(Required("MEMOTOPIA_CREDENTIALS_FILE")))
        {
            var match = System.Text.RegularExpressions.Regex.Match(line,
                @"^\s*" + System.Text.RegularExpressions.Regex.Escape(name) + @"\s*[:=]\s*(.+?)\s*$");
            if (match.Success) return match.Groups[1].Value;
        }
        throw new Exception("Local signing credential field is missing.");
    }

    private static string Required(string name)
    {
        string value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(value)) throw new Exception("Missing build variable: " + name);
        return value;
    }
}

#endif

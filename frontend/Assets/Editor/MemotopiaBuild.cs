using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.Linq;
using System.IO;
using UnityEditor.iOS.Xcode;

public class MemotopiaBuild
{
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
}

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace BRANDForUnity
{
    public static class BRANDUtils
    {
        public static void BuildProject()
        {
            string[] args = Environment.GetCommandLineArgs();
            string sceneName = GetArgValue(args, "-buildScene");
            string outputPath = GetArgValue(args, "-buildOutput");
            string buildTargetStr = GetArgValue(args, "-buildTarget");

            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(outputPath) || string.IsNullOrEmpty(buildTargetStr))
            {
                Debug.LogError("Missing required arguments: -buildScene, -buildOutput, or -buildTarget");
                return;
            }

            BuildTarget buildTarget;
            if (!Enum.TryParse(buildTargetStr, out buildTarget))
            {
                Debug.LogError($"Invalid build target: {buildTargetStr}");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = new[] { $"Assets/Scenes/{sceneName}.unity" },
                target = buildTarget,
                locationPathName = outputPath,
            };

            BuildPipeline.BuildPlayer(options);
        }

        private static string GetArgValue(string[] args, string argName)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == argName)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPPUtility
{
    public static class UnrealUtility
    {
        static public async Task<bool> IsUnrealSolutionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await VS.GetServiceAsync<DTE, DTE2>();

            string[] unrealProjectNames = { "Engine", "Games" };

            int nameCount = 0;

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                if (unrealProjectNames.Contains(project.Name))
                {
                    nameCount++;
                }
            }
            return nameCount >= unrealProjectNames.Length;
        }


        static public string GetShortestIncludePath(TextDocument textDocument)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var path = textDocument.Parent.FullName;

            string[] lastPaths = { "Source", "Public", "Classes" };

            var paths = path.Split('\\');

            for (int i = paths.Length - 1; i >= 0; i--)
            {
                var currentPath = paths[i];

                if (lastPaths.Contains(currentPath))
                {

                    int resultPathStartIndex = i + 1;


                    if (CPPUtilityOption.Instance.IsSkipGameModuleFolder)
                    {
                        // 最後がSourceのとき
                        if (currentPath == "Source")
                        {
                            var projectName = textDocument.Parent.ProjectItem?.ContainingProject?.Name;

                            // プロジェクトがUEから始まらないとき
                            if (projectName != null && !projectName.StartsWith("UE"))
                            {

                                // パスにPluginsが無いとき
                                if (!paths.Contains("Plugins"))
                                {
                                    // ゲームモジュールだと判断して、フォルダを一つ飛ばす
                                    resultPathStartIndex += 1;
                                }
                            }
                        }
                    }

                    string result = "";
                    for (int j = resultPathStartIndex; j < paths.Length; j++)
                    {
                        result += paths[j];

                        if (j != paths.Length - 1)
                        {
                            result += '/';
                        }
                    }
                    return result;
                }
            }

            return path;
        }
    }
}

using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
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


        static public string GetShortestIncludePath(string path)
        {
            string[] lastPaths = { "Source", "Public", "Classes" };

            var paths = path.Split('\\');

            for (int i = paths.Length - 1; i >= 0; i--)
            {
                if (lastPaths.Contains(paths[i]))
                {
                    string result = "";
                    for (int j = i + 1; j < paths.Length; j++)
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

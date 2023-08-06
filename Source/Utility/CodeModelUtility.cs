using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;
using System;
using System.Collections.Generic;

namespace CPPUtility
{
    internal static class CodeModelUtility
    {
        public static string GetFilePath(this ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projItemGuid = new Guid(projectItem.Kind);

            bool isPhysicalFile = (projItemGuid == VSConstants.GUID_ItemType_PhysicalFile);
            try
            {
                try
                {
                    return projectItem.FileNames[(short)(isPhysicalFile ? 0 : 1)];
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    return projectItem.FileNames[(short)(isPhysicalFile ? 1 : 0)];
                }
            }
            catch
            {
                return "";
            }
        }

        public static List<ProjectItem> GetProjectItemsRecursively(ProjectItems items)
        {
            ThreadHelper.ThrowIfNotOnUIThread();


            var ret = new List<EnvDTE.ProjectItem>();
            if (items == null) return ret;

            foreach (ProjectItem item in items)
            {
                ret.Add(item);
                ret.AddRange(GetProjectItemsRecursively(item.ProjectItems));

            }
            return ret;
        }
        public static List<CodeElement> CodeElementsRecursively(CodeElements elements)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var ret = new List<CodeElement>();
            if (elements == null) return ret;

            foreach (CodeElement codeElement in elements)
            {
                ret.Add(codeElement);

                ret.AddRange(CodeElementsRecursively(codeElement.Children));
            }

            return ret;
        }

        public static EditPoint GetHeaderFunctionStartPoint(CodeFunction codeFunction)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var vcCodeFunction = codeFunction as VCCodeFunction;
            return vcCodeFunction.StartPointOf[vsCMPart.vsCMPartHeader, vsCMWhere.vsCMWhereDeclaration].CreateEditPoint();
        }
    }

}

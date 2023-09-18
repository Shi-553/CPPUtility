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
        public static HashSet<CodeElement> GetCodeElementsChildrenRecursively(CodeElements elements)
        {
            var ret = new HashSet<CodeElement>();

            GetCodeElementsChildrenRecursivelyImpl(elements, ret);

            return ret;
        }
        static void GetCodeElementsChildrenRecursivelyImpl(CodeElements elements,HashSet<CodeElement> codeElements)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (elements == null) 
                return ;

            foreach (CodeElement codeElement in elements)
            {
                if (codeElements.Add(codeElement))
                {
                    GetCodeElementsChildrenRecursivelyImpl(codeElement.Children, codeElements);
                }
            }
        }

        public static EditPoint GetHeaderFunctionStartPoint(CodeFunction codeFunction)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var vcCodeFunction = codeFunction as VCCodeFunction;
            return vcCodeFunction.StartPointOf[vsCMPart.vsCMPartHeader, vsCMWhere.vsCMWhereDeclaration].CreateEditPoint();
        }
    }

}

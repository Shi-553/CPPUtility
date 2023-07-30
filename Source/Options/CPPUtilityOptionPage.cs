using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace CPPUtility
{
    [ComVisible(true)]
    [Guid(PackageGuids.CPPUtilityOptionPageString)]
    internal class CPPUtilityOptionPage: UIElementDialogPage
    {
        protected override UIElement Child
        {
            get
            {
                var page = new CPPUtilityOptions
                {
                    cppUtilityOptionsPage = this
                };
                page.Initialize();
                return page;
            }
        }
    }
}

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
        CPPUtilityOptions page;
        protected override UIElement Child
        {
            get
            {
                page = new CPPUtilityOptions
                {
                    cppUtilityOptionsPage = this
                };
                page.Initialize();
                return page;
            }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            CPPUtilityOption.Instance.Save();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
           
            CPPUtilityOption.Instance.Load();
            page.OnClosed();
        }
    }
}

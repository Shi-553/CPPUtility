using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static CPPUtility.BasicLiteralFormatter;
using static CPPUtility.DocumentLiteralFormatter;
using static CPPUtility.CPPFunctionCommentLiteralFormatter;

namespace CPPUtility
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class CPPUtilityOptions : BaseOptionPage<CPPUtilityOption> { }
    }

    public class CPPUtilityOption : BaseOptionModel<CPPUtilityOption>
    {
        [Category("Generate Comment")]
        [DisplayName("Document Top Comment Snippet")]
        [Description("")]
        public string DocumentTopCommentSnippet { get; set; } = $"//----------------\n// @file {FILENAME_LITERAL}\n// @desc {EDIT_POINT_LITERAL}\n// @auther {EDIT_POINT_LITERAL}\n// @date {DATE_LITERAL}\n//----------------\n";



        [Category("Generate Comment")]
        [DisplayName("Use Create Header Function Comment?")]
        [Description("")]
        public bool IsUseCreateHeaderFunctionComment { get; set; } = true;


        [Category("Generate Comment")]
        [DisplayName("Use Generate CPP Function Comment?")]
        [Description("")]
        public bool IsUseGenerateCPPFunctionComment { get; set; } = true;

        [Category("Generate Comment")]
        [DisplayName("CPP Function Comment Snippet")]
        [Description("")]
        public string CPPFunctionCommentSnippet { get; set; } = $"//----------------\n// {COMMENT_LITERAL}\n//----------------\n";




    }
}

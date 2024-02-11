using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static CPPUtility.BasicLiteralFormatter;
using static CPPUtility.DocumentLiteralFormatter;
using static CPPUtility.CPPFunctionCommentLiteralFormatter;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace CPPUtility
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class CPPUtilityOptions : BaseOptionPage<CPPUtilityOption> { }
    }


    public class CPPUtilityOption : BaseOptionModel<CPPUtilityOption>
    {
        // コメント生成


        public string DocumentTopCommentSnippet { get; set; } = $"//----------------\n// @file {FILENAME_LITERAL}\n// @desc {EDIT_POINT_LITERAL}\n// @auther {EDIT_POINT_LITERAL}\n// @date {DATE_LITERAL}\n//----------------\n";



        public bool IsUseCreateHeaderFunctionComment { get; set; } = true;

        public bool IsUseGenerateCPPFunctionComment { get; set; } = true;

        public string CPPFunctionCommentSnippet { get; set; } = $"//----------------\n// {COMMENT_LITERAL}\n//----------------\n";




        // 変数のフォーマット

        public VariableDelimiterType VariableDelimiterType { get; set; } = VariableDelimiterType.UpperCase;
        public string VariableFormattedTestRegexText { get; set; } = @"^[mag]";


        public ObservableCollection<VariableFormatInfo> VariableFormatInfos { get; set; } = new ObservableCollection<VariableFormatInfo>();




        // インクルードの追加


        public bool IsSkipGameModuleFolder { get; set; } = true;




        // シリアライズ・デシリアライズ


        static JsonSerializerSettings GetSettings()
        {
            var settings = new JsonSerializerSettings
            {
                // 見やすいようにインデントで整形
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };

            settings.Converters.Add(new StringEnumConverter { });
            return settings;
        }

        protected override object DeserializeValue(string serializedData, Type type, string propertyName)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject(serializedData, type, GetSettings());
                return obj;
            }
            catch
            {
                return base.DeserializeValue(serializedData, type, propertyName);
            }
        }
        protected override string SerializeValue(object value, Type type, string propertyName)
        {
            try
            {
                var str = JsonConvert.SerializeObject(value, type, GetSettings());
                return str;
            }
            catch
            {
                return base.SerializeValue(value, type, propertyName);
            }
        }
    }
}

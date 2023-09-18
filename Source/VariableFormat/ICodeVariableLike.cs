using EnvDTE;
using Microsoft.VisualStudio.VCCodeModel;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace CPPUtility
{
    // CodeVariable と CodeParameter に共通のインターフェースがないので無理矢理共通化した
    public interface ICodeVariableLike : CodeElement
    {
        VCCodeElement CodeElement { get; }

        CodeTypeRef Type { get; set; }

        CodeAttribute AddAttribute(string Name, string Value, object Position);

        bool IsSelf(object pOther);

        object Parent { get; }


        bool IsConstant { get; set; }

        CodeElements Attributes { get; }

        string DocComment { get; set; }

        string DisplayName { get; }

        bool IsCaseSensitive { get; }

        bool IsReadOnly { get; }

        bool IsZombie { get; }

        object Picture { get; }

        Project Project { get; }

        VCCodeModel CodeModel { get; }

        bool IsInjected { get; }

        string File { get; }

        string get_Location(vsCMWhere Where = vsCMWhere.vsCMWhereDefault);
        TextPoint get_StartPointOf(vsCMPart Part, vsCMWhere Where = vsCMWhere.vsCMWhereDefault);
        TextPoint get_EndPointOf(vsCMPart Part, vsCMWhere Where = vsCMWhere.vsCMWhereDefault);

        string TypeString { get; set; }


        bool IsVolatile { get; set; }
    }

    public static class CodeVariableLikeFactory
    {
        public static ICodeVariableLike CreateCodeVariableLike(CodeVariable codeVariable)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (codeVariable == null)
            {
                return null;
            }

            return new CodeVariableLike_Variable() { CodeVariable = codeVariable as VCCodeVariable };
        }
        public static ICodeVariableLike CreateCodeVariableLike(CodeParameter codeParameter)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (codeParameter == null)
            {
                return null;
            }

            return new CodeVariableLike_Parameter() { CodeParameter = codeParameter as VCCodeParameter };
        }


        class CodeVariableLike_Parameter : ICodeVariableLike,CodeParameter, VCCodeParameter
        {
            public VCCodeParameter CodeParameter { get; set; }
            public VCCodeElement CodeElement => CodeParameter as VCCodeElement;

            public TextPoint GetStartPoint(vsCMPart Part = vsCMPart.vsCMPartWholeWithAttributes)
            {
                return CodeParameter.GetStartPoint(Part);
            }

            public TextPoint GetEndPoint(vsCMPart Part = vsCMPart.vsCMPartWholeWithAttributes)
            {
                return CodeParameter.GetEndPoint(Part);
            }

            public CodeAttribute AddAttribute(string Name, string Value, object Position)
            {
                return CodeParameter.AddAttribute(Name, Value, Position);
            }

            public bool IsSelf(object pOther)
            {
                return CodeParameter.IsSelf(pOther);
            }

            public DTE DTE => CodeParameter.DTE;

            public CodeElements Collection => CodeParameter.Collection;

            public string Name { get => CodeParameter.Name; set => CodeParameter.Name = value; }

            public string FullName => CodeParameter.FullName;

            public ProjectItem ProjectItem => CodeParameter.ProjectItem;

            public vsCMElement Kind => CodeParameter.Kind;

            public bool IsCodeType => CodeParameter.IsCodeType;

            public vsCMInfoLocation InfoLocation => CodeParameter.InfoLocation;

            public CodeElements Children => CodeParameter.Children;

            public string Language => CodeParameter.Language;

            public TextPoint StartPoint => CodeParameter.StartPoint;

            public TextPoint EndPoint => CodeParameter.EndPoint;

            public object ExtenderNames => CodeParameter.ExtenderNames;

            public object get_Extender(string ExtenderName)
            {
                return CodeParameter.Extender[ExtenderName];
            }

            public string ExtenderCATID => CodeParameter.ExtenderCATID;

            public CodeElement Parent => CodeParameter.Parent;
            object ICodeVariableLike.Parent => CodeParameter.Parent;

            public CodeTypeRef Type { get => CodeParameter.Type; set => CodeParameter.Type = value; }

            public CodeElements Attributes => CodeParameter.Attributes;

            public string DocComment { get => CodeParameter.DocComment; set => CodeParameter.DocComment = value; }

            public string DisplayName => CodeParameter.DisplayName;

            public bool IsCaseSensitive => CodeParameter.IsCaseSensitive;

            public bool IsReadOnly => CodeParameter.IsReadOnly;

            public bool IsZombie => CodeParameter.IsZombie;

            public object Picture => CodeParameter.Picture;

            public Project Project => CodeParameter.Project;

            public VCCodeModel CodeModel => CodeParameter.CodeModel;

            public bool IsInjected => CodeParameter.IsInjected;

            public string File => CodeParameter.File;

            public string get_Location(vsCMWhere Where = vsCMWhere.vsCMWhereDefault)
            {
                return CodeParameter.Location[Where];
            }

            public TextPoint get_StartPointOf(vsCMPart Part, vsCMWhere Where = vsCMWhere.vsCMWhereDefault)
            {
                return CodeParameter.StartPointOf[Part, Where];
            }

            public TextPoint get_EndPointOf(vsCMPart Part, vsCMWhere Where = vsCMWhere.vsCMWhereDefault)
            {
                return CodeParameter.EndPointOf[Part, Where];
            }

            public object DefaultExpression { get => CodeParameter.DefaultExpression; set => CodeParameter.DefaultExpression = value; }
            public string TypeString { get => CodeParameter.TypeString; set => CodeParameter.TypeString = value; }

            public int Index => CodeParameter.Index;

            public bool IsConstant { get => CodeParameter.IsConstant; set => CodeParameter.IsConstant = value; }
            public bool IsVolatile { get => CodeParameter.IsVolatile; set => CodeParameter.IsVolatile = value; }
        }

        class CodeVariableLike_Variable : ICodeVariableLike, CodeVariable, VCCodeVariable
        {
            public VCCodeVariable CodeVariable { get; set; }
            public VCCodeElement CodeElement => CodeVariable as VCCodeElement;

            public TextPoint GetStartPoint(vsCMPart Part = vsCMPart.vsCMPartWholeWithAttributes)
            {
                return CodeVariable.GetStartPoint(Part);
            }

            public TextPoint GetEndPoint(vsCMPart Part = vsCMPart.vsCMPartWholeWithAttributes)
            {
                return CodeVariable.GetEndPoint(Part);
            }

            public CodeAttribute AddAttribute(string Name, string Value, object Position)
            {
                return CodeVariable.AddAttribute(Name, Value, Position);
            }

            public bool IsSelf(object pOther)
            {
                return CodeVariable.IsSelf(pOther);
            }

            public DTE DTE => CodeVariable.DTE;

            public CodeElements Collection => CodeVariable.Collection;

            public string Name { get => CodeVariable.Name; set => CodeVariable.Name = value; }

            public string FullName => CodeVariable.FullName;

            public ProjectItem ProjectItem => CodeVariable.ProjectItem;

            public vsCMElement Kind => CodeVariable.Kind;

            public bool IsCodeType => CodeVariable.IsCodeType;

            public vsCMInfoLocation InfoLocation => CodeVariable.InfoLocation;

            public CodeElements Children => CodeVariable.Children;

            public string Language => CodeVariable.Language;

            public TextPoint StartPoint => CodeVariable.StartPoint;

            public TextPoint EndPoint => CodeVariable.EndPoint;

            public object ExtenderNames => CodeVariable.ExtenderNames;

            public object get_Extender(string ExtenderName)
            {
                return CodeVariable.Extender[ExtenderName];
            }

            public string ExtenderCATID => CodeVariable.ExtenderCATID;

            public object Parent => CodeVariable.Parent;

            public object InitExpression { get => CodeVariable.InitExpression; set => CodeVariable.InitExpression = value; }

            public string get_Prototype(int Flags = 0)
            {
                return CodeVariable.Prototype[Flags];
            }

            public CodeTypeRef Type { get => CodeVariable.Type; set => CodeVariable.Type = value; }
            public vsCMAccess Access { get => CodeVariable.Access; set => CodeVariable.Access = value; }
            public bool IsConstant { get => CodeVariable.IsConstant; set => CodeVariable.IsConstant = value; }

            public CodeElements Attributes => CodeVariable.Attributes;

            public string DocComment { get => CodeVariable.DocComment; set => CodeVariable.DocComment = value; }
            public string Comment { get => CodeVariable.Comment; set => CodeVariable.Comment = value; }
            public bool IsShared { get => CodeVariable.IsShared; set => CodeVariable.IsShared = value; }

            public string DisplayName => CodeVariable.DisplayName;

            public bool IsCaseSensitive => CodeVariable.IsCaseSensitive;

            public bool IsReadOnly => CodeVariable.IsReadOnly;

            public bool IsZombie => CodeVariable.IsZombie;

            public object Picture => CodeVariable.Picture;

            public Project Project => CodeVariable.Project;

            public VCCodeModel CodeModel => CodeVariable.CodeModel;

            public bool IsInjected => CodeVariable.IsInjected;

            public string File => CodeVariable.File;

            public string get_Location(vsCMWhere Where = vsCMWhere.vsCMWhereDefault)
            {
                return CodeVariable.Location[Where];
            }

            public TextPoint get_StartPointOf(vsCMPart Part, vsCMWhere Where = vsCMWhere.vsCMWhereDefault)
            {
                return CodeVariable.StartPointOf[Part, Where];
            }

            public TextPoint get_EndPointOf(vsCMPart Part, vsCMWhere Where = vsCMWhere.vsCMWhereDefault)
            {
                return CodeVariable.EndPointOf[Part, Where];
            }

            public string TypeString { get => CodeVariable.TypeString; set => CodeVariable.TypeString = value; }

            public EnvDTE.CodeNamespace Namespace => CodeVariable.Namespace;

            public string DeclarationText { get => CodeVariable.DeclarationText; set => CodeVariable.DeclarationText = value; }

            public CodeElements References => CodeVariable.References;

            public bool IsVolatile { get => CodeVariable.IsVolatile; set => CodeVariable.IsVolatile = value; }
        }

    }
}

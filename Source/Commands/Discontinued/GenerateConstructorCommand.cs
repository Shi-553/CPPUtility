using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;
using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CPPUtility.Commands
{
    [Command(PackageIds.GenerateConstructorCommand)]
    internal sealed class GenerateConstructorCommand : BaseCommand<GenerateConstructorCommand>
    {

        static readonly vsCMTypeRef[] ValTypes = new vsCMTypeRef[]
        {
                        vsCMTypeRef.vsCMTypeRefByte,
                        vsCMTypeRef.vsCMTypeRefChar,
                        vsCMTypeRef.vsCMTypeRefShort,
                        vsCMTypeRef.vsCMTypeRefInt,
                        vsCMTypeRef.vsCMTypeRefLong,
                        vsCMTypeRef.vsCMTypeRefFloat,
                        vsCMTypeRef.vsCMTypeRefDouble,
                        vsCMTypeRef.vsCMTypeRefDecimal,
                        vsCMTypeRef.vsCMTypeRefBool,
                        vsCMTypeRef.vsCMTypeRefPointer,

                        (vsCMTypeRef)vsCMTypeRef2.vsCMTypeRefUnsignedChar,
                        (vsCMTypeRef)vsCMTypeRef2.vsCMTypeRefUnsignedShort,
                        (vsCMTypeRef)vsCMTypeRef2.vsCMTypeRefUnsignedInt,
                        (vsCMTypeRef)vsCMTypeRef2.vsCMTypeRefUnsignedLong,
                        (vsCMTypeRef)vsCMTypeRef2.vsCMTypeRefReference,
                        (vsCMTypeRef)vsCMTypeRef2.vsCMTypeRefMCBoxedReference,
                        (vsCMTypeRef)vsCMTypeRef2.vsCMTypeRefSByte
        };

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await VS.GetServiceAsync<DTE, DTE2>();

            var editPoint = (dte?.ActiveDocument?.Selection as EnvDTE.TextSelection)?.ActivePoint.CreateEditPoint();

            var prevEditPoint = editPoint;

            while (true)
            {
                if (editPoint == null)
                {
                    return;
                }

                if (editPoint.CodeElement[vsCMElement.vsCMElementStruct] is CodeStruct s)
                {
                    dte.UndoContext.Open($"Create '{s.Name}' struct Constructor");
                    try
                    {
                        var func = s.AddFunction(s.Name, vsCMFunction.vsCMFunctionConstructor, null, -1);
                        AddArgsAndInitializer(func, s.Members);

                    }
                    finally
                    {
                        dte.UndoContext.Close();
                    }
                    return;
                }

                if (editPoint.CodeElement[vsCMElement.vsCMElementClass] is CodeClass c)
                {
                    dte.UndoContext.Open($"Create '{c.Name}' class Constructor");
                    try
                    {
                        var func = c.AddFunction(c.Name, vsCMFunction.vsCMFunctionConstructor, null, -1);
                        AddArgsAndInitializer(func, c.Members);
                    }
                    finally
                    {
                        dte.UndoContext.Close();
                    }
                    return;
                }

                foreach (vsCMElement kind in Enum.GetValues(typeof(vsCMElement)))
                {
                    try
                    {
                        if (editPoint.CodeElement[kind] is CodeElement element)
                        {
                            var parent = element.GetType().InvokeMember("Parent", System.Reflection.BindingFlags.GetProperty, null, element, null);
                            if (parent != null)
                            {
                                editPoint = (parent as CodeElement).GetStartPoint(vsCMPart.vsCMPartName).CreateEditPoint();
                                break;
                            }
                        }
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException)) { }

                }

                if (prevEditPoint == editPoint)
                    return;
                prevEditPoint = editPoint;
            }
        }

        void AddArgsAndInitializer(CodeFunction func, CodeElements members)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!(func is VCCodeFunction vcFunc))
            {
                return;
            }

            //CodeElements bases = null;
            //if (vcFunc.Parent is VCCodeClass cl)
            //{
            //    bases = cl.Bases;
            //}
            //if (vcFunc.Parent is VCCodeStruct cs)
            //{
            //    bases = cs.Bases;
            //}
            //if (bases != null)
            //{
            //    foreach (var b in bases)
            //    {
            //        if (b is VCCodeBase codeBase)
            //        {
            //            foreach (var child in codeBase.Children)
            //            {
            //                if (child is VCCodeFunction baseFunction)
            //                {
            //                    if (baseFunction.FunctionKind != vsCMFunction.vsCMFunctionConstructor)
            //                    {
            //                        continue;
            //                    }

            //                    baseFunction.Parameters
            //                    vcFunc.AddParameter(name, type, -1);
            //                    vcFunc.AddInitializer($"{val.Name}({val.Name})");

            //                }
            //            }
            //        }
            //    }
            //}

            foreach (CodeElement item in members)
            {
                if (item.Kind == vsCMElement.vsCMElementVariable)
                {
                    var val = (VCCodeVariable)item;
                    var type = val.Type.AsString;
                    var name = val.Name;


                    if (!ValTypes.Contains(val.Type.TypeKind))
                    {
                        type = $"const {type}&";
                    }

                    vcFunc.AddParameter(name, type, -1);

                    vcFunc.AddInitializer($"{val.Name}({val.Name})");
                }
            }
        }
    }
}

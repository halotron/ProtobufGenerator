/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell;
using VSLangProj80;

namespace Knacka.Se.ProtobufGenerator
{
    /// <summary>
    /// This is the generator class. 
    /// When setting the 'Custom Tool' property of a C#, VB, or J# project item to "ProtobufGenerator", 
    /// the GenerateCode function will get called and will return the contents of the generated file 
    /// to the project system
    /// </summary>
    [ComVisible(true)]
    [Guid("52FD1149-33FA-4DD3-AC44-AB655C27671C")]
    [CodeGeneratorRegistration(typeof(ProtobufGeneratorGRPC), "ProtobufGeneratorGRPC - Generate C# from proto files", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
    [CodeGeneratorRegistration(typeof(ProtobufGeneratorGRPC), "ProtobufGeneratorGRPC - Generate C# from proto files", TempNetSdkProjectGuid, GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(ProtobufGeneratorGRPC))]
    public class ProtobufGeneratorGRPC : ProtobufGeneratorBase
    {
#pragma warning disable 0414
        //The name of this generator (use for 'Custom Tool' property of project item)
        internal static string name = "ProtobufGeneratorGRPC";
#pragma warning restore 0414

        protected override bool GenerateGRPC => true;

    }
}
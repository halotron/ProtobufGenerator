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
    public abstract class ProtobufGeneratorBase : BaseCodeGeneratorWithSite
    {
        protected const string TempNetSdkProjectGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";

        protected static string _protocPath;
        protected static string _grpcPath;

        protected abstract bool GenerateGRPC { get; }

        /// <summary>
        /// Function that builds the contents of the generated file based on the contents of the input file
        /// </summary>
        /// <param name="inputFileContent">Content of the input file</param>
        /// <returns>Generated file as a byte array</returns>
        protected override byte[] GenerateCode(string inputFileContent)
        {
            var vsItem = this.GetVSProjectItem();
            var name = vsItem?.ProjectItem?.Name;
            var path = vsItem?.ProjectItem?.Document?.Path;

            if (_protocPath == null)
            {
                IHaveProtoc protocFinder = new HaveProtoc(path, GenerateGRPC);
                if (protocFinder.HaveFoundProtoc)
                {
                    _protocPath = protocFinder.ProtocPath;
                    _grpcPath = protocFinder.GrpcPath;
                }
            }

            if (string.IsNullOrEmpty(_protocPath))
            {
                this.GeneratorError(4, "Protoc.exe not found. Please read the documentation for ProtobufGenerator", 1, 1);
                return null;
            }

            ICanGenerateFromProto generator = new GenerateFromProto(_protocPath, _grpcPath);

            try
            {
                var inputFile = Path.GetFileName(InputFilePath) 
                    ?? (Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".proto");

                if (this.CodeGeneratorProgress != null)
                {
                    this.CodeGeneratorProgress.Progress(50, 100);
                }
                var res = generator.GenerateCsharpFromProto(inputFileContent, path, inputFile);
                if (this.CodeGeneratorProgress != null)
                {
                    this.CodeGeneratorProgress.Progress(100, 100);
                }
                return res;
            }
            catch (Exception e)
            {
                this.GeneratorError(4, e.ToString(), 1, 1);
                //Returning null signifies that generation has failed
                return null;
            }
        }


    }
}
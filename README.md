# ProtobufGenerator
A plugin for Visual Studio that will automatically transform protobuf proto-files into generated C# code.

How to use it:
1. Install the ProtobufGenerator VSIX file (just download and double click). https://marketplace.visualstudio.com/items?itemName=jonasjakobsson.ProtobufGeneratorvisualstudio
2. Add "ProtobufGenerator" as Custom Tool for any proto file in your solution.
3. If everything is OK, a C# file will be created as a child of the proto file after any changes.
If not, see below.

PREREQUISITE:
Protoc.exe, contained in the nuget package Google.Protobuf.Tools, needs to be somewhere on your system. NOT necessarily in the project you are working with.

It searches for it in these locations in order:
1. Is protoc.exe in your system path.
2. Is Protoc.exe in your packages folder under your profile directory?
  In my case that is c:\Users\jonas\\.nuget\packages\Google.Protobuf.Tools....
3. Then it searches in the current folder of the proto-file being generated from. If it finds a packages-folder it will search for the Google.Protobuf.Tools folder and find the protoc.exe there.
4. It will search in parent directories of the generated file for the packages folder. So if your packages folder is c:\src\packages it will find the Google.Protobuf.Tools there if the file being generated is in c:\src\some\distant\folder\hello.proto


Currently supports Visual Studio 2017 and might work with 2013 and 2015 as well. Try it and report on your success.

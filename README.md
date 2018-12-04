# ProtobufGenerator
A plugin for Visual Studio that will automatically transform protobuf proto-files into generated C# code.
Currently it does not support a proto file with declarations from a different file. Everything needed in the proto file needs to be in the proto file.

GRPC is supported as well. See below.

How to use it:
1. Install the ProtobufGenerator VSIX file from here: https://marketplace.visualstudio.com/items?itemName=jonasjakobsson.ProtobufGeneratorvisualstudio
2. Add "ProtobufGenerator" as Custom Tool for any proto file in your solution. (look in properties for the file)
3. If everything is OK, a C# file will be created as a child of the proto file after any changes.
If not, see below.

PREREQUISITE:
Protoc.exe, contained in the nuget package Google.Protobuf.Tools, needs to be somewhere on your system. NOT necessarily in the project you are working with.
But the easiest option might be to just install Google.Protobuf.Tools in your project.

It searches for protoc.exe in these locations in order:
1. Is protoc.exe in your system path.
2. Is Protoc.exe in your packages folder under your profile directory?
  In my case that is c:\Users\jonas\\.nuget\packages\Google.Protobuf.Tools....
3. Then it searches in the current folder of the proto-file being generated from. If it finds a packages-folder it will search for Google.Protobuf.Tools inside of that.
4. It will search in parent directories of the generated file for the packages folder. So if your packages folder is c:\src\packages it will find the Google.Protobuf.Tools there if the file being generated is in c:\src\some\distant\folder\hello.proto


GRPC support:
Use "ProtobufGeneratorGRPC" instead of "ProtobufGenerator".
Same story: Grpc needs to be somewhere...

Logging:
outputs logging and the complete command that is executed to the general output pane in visual studio, or to the debug pane, if for some reason output pane is not available.

Protobuf Argument Support :boom: Waring this may have unintended consequences :boom:
Support for extra protoc cli arguments is provided on a per file basis by placing a comment of the form // ProtobufGenerator-Arg:__arg_name__:__arg_value__. This will generate the following input to protoc --arg_name arg_value. Arguments may be defined multiple times which may be helpful in the case of proto path (note all import paths are relative to file being transformed).
Example
```
/// ProtobufGenerator-Arg:proto_path:../packages/Google.Protobuf.Tools.3.6.1/tools/
/// ProtobufGenerator-Arg:proto_path:../OtherLibrary/

syntax = "proto3";

import "google/protobuf/timestamp.proto";
import "SharedProtos.proto";
```
The above example generates the following parameters to be passed to protoc --proto_path ../packages/Google.Protobuf.Tools.3.6.1/tools/ --proto_path ../OtherLibrary

Currently supports Visual Studio 2017 and might work with 2013 and 2015 as well. Try it and report on your success.

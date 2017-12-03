# ProtobufGenerator
A plugin to Visual Studio that will automatically transform protobuf proto-files into generated C# code.

How to use it:
1. Install the ProtobufGenerator VSIX file (just download and double click).
2. Add "ProtobufGenerator" as Custom Tool for any proto file in your solution.
3. If everything is OK, a C# file will be created as a child of the proto file after any changes.


Currently supports Visual Studio 2017. If you want support for another version, please make a pull request. It should not be very hard to modify to work with other versions.

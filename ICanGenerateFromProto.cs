using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knacka.Se.ProtobufGenerator
{
    public interface ICanGenerateFromProto
    {
        byte[] GenerateCsharpFromProto(string protoContent, string protoDirPath);
    }
}

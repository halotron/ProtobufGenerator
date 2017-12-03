namespace Knacka.Se.ProtobufGenerator
{
    public interface IHaveProtoc
    {
        bool HaveFoundProtoc { get; }
        string ProtocPath { get; }
    }
}
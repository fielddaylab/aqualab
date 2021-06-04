using BeauUtil;

namespace ProtoAqua.Argumentation
{
    static public class ArgumentationEvents
    {
        static public readonly StringHash32 ArgueValidResponse = "argue:valid-response"; // StringHash32 nodeId
        static public readonly StringHash32 ArgueInvalidResponse = "argue:invalid-response"; // StringHash32 nodeId
    }
}

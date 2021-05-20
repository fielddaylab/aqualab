using BeauData;

namespace Aqua.Profile
{
    public interface IProfileChunk : ISerializedObject
    {
        bool HasChanges();
        void MarkChangesPersisted();
    }
}
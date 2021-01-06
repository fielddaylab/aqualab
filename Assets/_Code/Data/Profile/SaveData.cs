using BeauData;

namespace Aqua.Profile
{
    public class SaveData : ISerializedObject, ISerializedVersion
    {
        public string Id;
        public long LastUpdated;

        public CharacterProfile Character = new CharacterProfile();
        public InventoryData Inventory = new InventoryData();
        public ScriptingData Script = new ScriptingData();
        public BestiaryData Bestiary = new BestiaryData();
        public JobsData Jobs = new JobsData();

        #region ISerializedData

        ushort ISerializedVersion.Version { get { return 4; } }

        void ISerializedObject.Serialize(Serializer ioSerializer)
        {
            ioSerializer.Serialize("id", ref Id);
            ioSerializer.Serialize("lastUpdated", ref LastUpdated, 0L);
            ioSerializer.Object("character", ref Character);

            if (ioSerializer.ObjectVersion >= 2)
            {
                ioSerializer.Object("inventory", ref Inventory);
                ioSerializer.Object("script", ref Script);
            }

            if (ioSerializer.ObjectVersion >= 3)
            {
                ioSerializer.Object("bestiary", ref Bestiary);
            }

            if (ioSerializer.ObjectVersion >= 4)
            {
                ioSerializer.Object("jobs", ref Jobs);
            }
        }

        #endregion // ISerializedData
    }
}
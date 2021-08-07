using System.Collections.Generic;

namespace shared
{
    /**
     * Send from SERVER to CLIENTS to show the dices a client has rolled, and what client has rolled them
     */
    public class DiceBagUpdate: ASerializable
    {
        public DiceBag bag;
        public string owner; //the player who owns the bag
        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(owner);
            pPacket.Write(bag);
        }

        public override void Deserialize(Packet pPacket)
        {
            owner = pPacket.ReadString();
            bag=pPacket.Read<DiceBag>();
        }
    }
}

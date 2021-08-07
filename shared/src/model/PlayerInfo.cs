namespace shared
{
    /**
    Information about the player
     */
    public class PlayerInfo : ASerializable
    {
        public string username;
        public bool heartBeatState;

        public PlayerInfo() { heartBeatState = true; }

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(username);
        }

        public override void Deserialize(Packet pPacket)
        {
            username=pPacket.ReadString();
        }
    }
}

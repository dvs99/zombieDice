namespace shared
{
    /**
     * Send from CLIENT to SERVER to decice what to do in your turn
     */
    public class ActionRequest: ASerializable
    {
        public enum Action { ROLL, ENDTURN };
        public Action action;
        public override void Serialize(Packet pPacket)
        {
            pPacket.Write((int)action);
        }

        public override void Deserialize(Packet pPacket)
        {
            action=(Action)pPacket.ReadInt();
        }
    }
}

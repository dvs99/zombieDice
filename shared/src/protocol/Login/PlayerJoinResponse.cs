namespace shared
{
    /**
     * Send from SERVER to CLIENT to let the client know whether it was allowed to join or not.
     */
    public class PlayerJoinResponse : ASerializable
    {
        public enum RequestResult { ACCEPTED, USEDNAME, GENERICERROR }; //can add different result states, use genericerror to reject a connection without providing feedback
        public RequestResult result;
        public override void Serialize(Packet pPacket)
        {
            pPacket.Write((int)result);
        }

        public override void Deserialize(Packet pPacket)
        {
            result = (RequestResult)pPacket.ReadInt();
        }
    }
}

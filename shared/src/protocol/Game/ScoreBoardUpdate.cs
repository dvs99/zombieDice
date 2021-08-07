using System.Collections.Generic;

namespace shared
{
    /**
     * Send from SERVER to CLIENTS to update the names and scores of players
     */
    public class ScoreBoardUpdate : ASerializable
    {
        public ScoreBoardData scores;
        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(scores);
        }

        public override void Deserialize(Packet pPacket)
        {
            scores = pPacket.Read<ScoreBoardData>();
        }
    }
}

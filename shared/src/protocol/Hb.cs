namespace shared
{
	/**
	 * Send from CLIENT to SEVER to notify that the client is still connected.
	 */
	public class Hb : ASerializable
	{
		public override void Serialize(Packet pPacket)
		{
		}

		public override void Deserialize(Packet pPacket)
		{
		}
		public override string ToString()
		{
			return "Heartbeat";
		}
	}
}

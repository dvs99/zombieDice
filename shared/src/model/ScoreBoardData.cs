using System;

namespace shared
{
	/**
	 * Scoreboard with player names and their scores
	 */
	public class ScoreBoardData : ASerializable
	{
		public int[] scores = new int[10] { -1, -1, -1, -1, -1, -1, -1, -1, - 1, -1 };
		public string[] names = new string[10] { "", "", "", "", "", "", "", "", "", ""};
		
		public override void Serialize(Packet pPacket)
		{
			for (int i = 0; i < scores.Length; i++) pPacket.Write(scores[i]);
			for (int i = 0; i < names.Length; i++) pPacket.Write(names[i]);
		}

		public override void Deserialize(Packet pPacket)
		{
			for (int i = 0; i < scores.Length; i++) scores[i] = pPacket.ReadInt();
			for (int i = 0; i < names.Length; i++) names[i] = pPacket.ReadString();
		}

		public override string ToString()
		{
			return GetType().Name +":"+ "[names]" + string.Join(",", names) + " [scores]"+ string.Join(",",scores);
		}
	}
}


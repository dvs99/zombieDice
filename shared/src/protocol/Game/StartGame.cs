namespace shared
{
    /**
     * Send from SERVER to all CLIENTS to update the scores
     */
    public class StartGame: ASerializable
    {
        public string[] names;
        public string username;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(names.Length);
            foreach (string name in names)
                pPacket.Write(name);

            pPacket.Write(username);
        }

        public override void Deserialize(Packet pPacket)
        {
            int length=pPacket.ReadInt();
            names = new string[length];
            for(int i =0; i<length; i++)
                names[i]=pPacket.ReadString();

            username = pPacket.ReadString();
        }
        public override string ToString()
        {
            return GetType().Name + ":" + "[names]" + string.Join(",", names) + " [username]" + username;
        }
    }
}

namespace shared
{
    /**
    Conteins the data of a dice, incuding the result of the roll and color of the dice
     */
    public class DiceInfo : ASerializable
    {
        public enum State { BRAIN, FOOT, SHOT, UNROLLED };
        public enum Color { GREEN, YELLOW, RED  };

        public Color color;
        public State state;

        public DiceInfo() { state = State.UNROLLED; }

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write((int)color);
            pPacket.Write((int)state);
        }

        public override void Deserialize(Packet pPacket)
        {
            color = (Color)pPacket.ReadInt();
            state = (State)pPacket.ReadInt();
        }
    }
}

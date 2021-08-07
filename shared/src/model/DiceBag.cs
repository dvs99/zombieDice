using System;

namespace shared
{
    /**
    *Holds 13 dice in the right color proportions
    */
    public class DiceBag : ASerializable
    {
        public DiceInfo[] diceBag { get; set; } = new DiceInfo[13];

        public DiceBag()
        {
            for (int i = 0; i < 13; i++)
            {
                diceBag[i] = new DiceInfo();
                if (i < 6)
                    diceBag[i].color = DiceInfo.Color.GREEN;
                else if (i < 10)
                    diceBag[i].color = DiceInfo.Color.YELLOW;
                else
                    diceBag[i].color = DiceInfo.Color.RED;
            }
        }

        public override void Serialize(Packet pPacket)
        {
            for (int i = 0; i < 13; i++)
                pPacket.Write(diceBag[i]);
        }

        public override void Deserialize(Packet pPacket)
        {
            for (int i = 0; i < 13; i++)
                diceBag[i] = pPacket.Read<DiceInfo>();
        }

        public override string ToString()
        {
            return GetType().Name + ":" + "[diceBag]" + string.Join<DiceInfo>(",", diceBag);
        }
    }
}

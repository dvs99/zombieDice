using shared;
using System;

namespace server
{
    /**
     * interact with the dice and dicebags using this class
     */
    public class DiceManager
    {
        private Random r = new Random();
        private float[,] stateChance = new float[3, 3] { { 3, 2, 1 }, { 2, 2, 2 }, { 1, 2, 3 } }; //use: stateChance[Color,State]


        //roll a dice, taking into account what kind of output each dice produces
        public DiceInfo RollDice(DiceInfo dice)
        {
            int faceUp = r.Next(0, 6);
            if (faceUp < stateChance[(int)dice.color, (int)DiceInfo.State.BRAIN])
                dice.state = DiceInfo.State.BRAIN;
            else if (faceUp < (stateChance[(int)dice.color, (int)DiceInfo.State.BRAIN] + stateChance[(int)dice.color, (int)DiceInfo.State.FOOT]))
                dice.state = DiceInfo.State.FOOT;
            else
                dice.state = DiceInfo.State.SHOT;

            return dice;
        }


        //check if you can't roll 3 new dice on a bag
        public bool UnrollableBag(DiceBag bag)
        {
            int c = 0;
            DiceInfo[] diceBag = bag.diceBag;
            bool unrollable = true;
            for (int i = 0; i < 13; i++)
                if (diceBag[i].state == DiceInfo.State.UNROLLED || diceBag[i].state == DiceInfo.State.FOOT)
                    c++;

            return c<3;
        }

        public int GetRandomUnrolledDiceIndexInBag(DiceBag bag) //asumes there is a rollable dice. CHECK BEFORE W/ filledBag
        {

            DiceInfo[] diceBag = bag.diceBag;

            //find unrolled dice
            int diceIndex = r.Next(0, 13);
            while (diceBag[diceIndex].state != DiceInfo.State.UNROLLED)
            {
                diceIndex = r.Next(0, 13);
            }
            return diceIndex;
        }

        //find a dice in the bag
        public DiceInfo GetDiceAtIndexInBag(DiceBag bag, int index)
        {
            DiceInfo[] diceBag =bag.diceBag;
            if (index >= 0 && index < 13)
            {
                Console.WriteLine(diceBag[index]);
                return diceBag[index];
            }
            else return null;
        }

        //counts the amount of brains
        public int CountPoints(DiceBag bag)
        {
            int c=0;
            foreach (DiceInfo dice in bag.diceBag)
                if (dice.state == DiceInfo.State.BRAIN)
                    c++;
            return c;
        }


        //checks if the bag has as least three shots 
        public bool ThreeShotsInBag(DiceBag bag)
        {
            int c = 0;
            foreach (DiceInfo dice in bag.diceBag)
                if (dice.state == DiceInfo.State.SHOT)
                    c++;
            return c>=3;
        }
    }
}

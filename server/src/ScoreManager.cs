using shared;
using System;

namespace server
{
    /**
     * you should only interact with the scoreboard using this class
     */
    public class ScoreManager
    {
        public ScoreBoardData scoreBoard { get; private set; } = new ScoreBoardData();
        int nameIndexForAdding=0;

        public void AddName (string name)
        {
            if (nameIndexForAdding < scoreBoard.names.Length)
            {
                scoreBoard.scores[nameIndexForAdding] = 0;
                scoreBoard.names[nameIndexForAdding++] = name;
            }
        }

        public bool AddPoints(int playerIndex, int points)
        {
            scoreBoard.scores[playerIndex] +=points;
            return scoreBoard.scores[playerIndex] >= 13;
        }

        public string[] GetNames()
        {
            return scoreBoard.names;
        }

        //checks if a player exists @index
        public bool IsPlayerAlive (int indexOfPlayer)
        {
            return !(scoreBoard.names[indexOfPlayer]=="");
        }

        //find a player by username and remove it
        public void RemovePlayerWithName(string username)
        {
            for (int i = 0; i < scoreBoard.names.Length; i++)
                if (scoreBoard.names[i] == username)
                {
                    scoreBoard.names[i] = "";
                    scoreBoard.scores[i] = -1;
                    return;
                }
        }

        //find index of a player by username

        public int indexOfPlayer(string username)
        {
            for (int i = 0; i < scoreBoard.names.Length; i++)
                if (scoreBoard.names[i] == username)
                {
                    return i;
                }
            return -1;
        }
    }
}

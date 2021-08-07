using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using shared;
using TMPro;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    public enum EndGame { FALSE, WON, LOST};
    private int scoreBoardSize;

    private TextMeshProUGUI[] nameArray;
    private TextMeshProUGUI[] scoreArray;
    private Vector2 initialSize;

    private void Awake()
    {
        initialSize = new Vector2(GetComponent<RectTransform>().sizeDelta.x, GetComponent<RectTransform>().sizeDelta.y);

        scoreBoardSize = transform.childCount;
        nameArray = new TextMeshProUGUI[scoreBoardSize];
        scoreArray = new TextMeshProUGUI[scoreBoardSize];
        
        for (int i=0; i < scoreBoardSize;  i++)
        {
            nameArray[i] = transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();
            scoreArray[i] = transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>();
        }
    }

    //Updates the scoreboard to reflect the given  data. If it detects the game session has ended it returns wether the player won or not
    public EndGame SetData(ScoreBoardData pScoreBoardData, string username)
    {
        EndGame endGame = EndGame.FALSE;
        int c=0;
        for (int i = 0; i < scoreBoardSize; i++)
        {
            //ignore unexisting or removed players
            if (pScoreBoardData.names[i] == "")
            {
                c++;
                if (nameArray[i].transform.parent.gameObject.activeSelf == true)
                {
                    nameArray[i].transform.parent.gameObject.SetActive(false);
                    GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, GetComponent<RectTransform>().sizeDelta.y - nameArray[i].transform.parent.GetComponent<RectTransform>().sizeDelta.y - GetComponent<VerticalLayoutGroup>().spacing);
                }
            }

            nameArray[i].text = pScoreBoardData.names[i];
            scoreArray[i].text = pScoreBoardData.scores[i].ToString();

            if (nameArray[i].text == username)
            {
                nameArray[i].color = new Color(0.165f, 1f, 0.337f);
                scoreArray[i].color = new Color(0.165f, 1f, 0.337f);
            }

            if (pScoreBoardData.scores[i] >= 13)
            {
                if (nameArray[i].text == username)
                    endGame = EndGame.WON;
                else
                    endGame = EndGame.LOST;
            }
        }
        if (c == 9)
            endGame = EndGame.WON;

        return endGame;
    }

    //changes turn in ui and returns true if the turn changed
    public bool SetTurn(string turnUsername) 
    {
        bool changed = false;
        for (int i = 0; i < scoreBoardSize; i++)
        {
            if (nameArray[i].text == turnUsername)
            {
                if (!nameArray[i].transform.GetChild(0).gameObject.activeSelf)
                {
                    nameArray[i].transform.GetChild(0).gameObject.SetActive(true);
                    changed = true;
                }
            }
            else
            {
                nameArray[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        return changed;
    }

    //Updates the scoreboard to reflect the given data for the first time.
    public void CreateData(string[] names, string username)
    {
        for (int i = 0; i < scoreBoardSize; i++)
        {
            if (names[i] == "")
            {
                nameArray[i].transform.parent.gameObject.SetActive(false);
                GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, GetComponent<RectTransform>().sizeDelta.y - nameArray[i].transform.parent.GetComponent<RectTransform>().sizeDelta.y- GetComponent<VerticalLayoutGroup>().spacing);
            }

            nameArray[i].text = names[i];
            scoreArray[i].text = "0";
            if (nameArray[i].text == username)
            {
                nameArray[i].color = new Color(0.165f, 1f, 0.337f);
                scoreArray[i].color = new Color(0.165f, 1f, 0.337f);
            }
        }
    }

    public void ReSet()
    {
        scoreBoardSize = transform.childCount;
        for (int i = 0; i < scoreBoardSize; i++)
        {
            nameArray[i].transform.parent.gameObject.SetActive(true);
            nameArray[i].color = new Color(0.984f, 0.176f, 0.176f);
            scoreArray[i].color = new Color(0.984f, 0.176f, 0.176f);
        }

        GetComponent<RectTransform>().sizeDelta = initialSize;
    }
}

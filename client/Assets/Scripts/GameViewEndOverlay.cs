using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameViewEndOverlay : MonoBehaviour
{
    [SerializeField]
    private Transform winHolder;
    [SerializeField]
    private Transform myScoreBoard;
    [SerializeField]
    private Button leave;



    public void Set(bool won, ScoreBoard scoreBoard, Button.ButtonClickedEvent leaveOnClickEvent)
    {
        if (won)
            winHolder.GetChild(0).gameObject.SetActive(true);
        else
            winHolder.GetChild(1).gameObject.SetActive(true);

        for (int i = 0; i < myScoreBoard.transform.childCount; i++)
        {
            myScoreBoard.transform.GetChild(i).gameObject.SetActive(scoreBoard.transform.GetChild(i).gameObject.activeSelf);
            myScoreBoard.transform.GetChild(i).GetChild(0).GetChild(0).gameObject.SetActive(scoreBoard.transform.GetChild(i).GetChild(0).GetChild(0).gameObject.activeSelf);
            myScoreBoard.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = scoreBoard.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text;
            myScoreBoard.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = scoreBoard.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text;
            myScoreBoard.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().color = scoreBoard.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().color;
            myScoreBoard.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color = scoreBoard.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color;
        }

        leave.onClick = leaveOnClickEvent;
    }

    public void ReSet()
    {
        winHolder.GetChild(0).gameObject.SetActive(false);
        winHolder.GetChild(1).gameObject.SetActive(false);

        for (int i = 0; i < myScoreBoard.transform.childCount; i++)
        {
            myScoreBoard.transform.GetChild(i).gameObject.SetActive(true);
            myScoreBoard.transform.GetChild(i).GetChild(0).GetChild(0).gameObject.SetActive(false);
            myScoreBoard.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            myScoreBoard.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            myScoreBoard.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0);
            myScoreBoard.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0);
        }
    }
}

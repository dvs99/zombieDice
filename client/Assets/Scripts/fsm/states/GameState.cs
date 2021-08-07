using shared;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * This is where we 'play' a game.
 */
public class GameState : ApplicationStateWithView<GameView>
{
    int timeForTurn = 90;
    private int timeLeft;
    private string username;
    private bool myTurn;
    private bool endButton;
    private bool rollButton;
    private int shots = 0;
    private int brains = 0;
    private int needToBeStored = 3;
    private bool firstUpdate = true;

    private Transform storageTransforms;

    DiceInfo[] oldBag;
    List<int> reRollIndeces = new List<int>();
    List<int> reRollPos = new List<int>();

    List<Dice> rollingDice = new List<Dice>();


    private void Awake()
    {
        storageTransforms = view.storageParent.transform;
    }

    //set teh right starting values and button methods
    public override void EnterState()
    {
        base.EnterState();
        view.buttonEnd.onClick.AddListener(endTurn);
        view.buttonRoll.onClick.AddListener(rollAgain);
        view.buttonLeave.onClick.AddListener(leaveGame);
        view.buttonEnd.gameObject.SetActive(false);
        view.itemsLeftText.gameObject.SetActive(true);
    }


    //reset some values and exit
    public override void ExitState()
    {
        view.scoreboard.ReSet();
        if (view.endOverlay.gameObject.activeSelf)
        {
            view.endOverlay.ReSet();
            view.endOverlay.gameObject.SetActive(false);
        }

        foreach (Transform t in storageTransforms)
        {
            if (t.childCount > 0)
                Destroy(t.GetChild(0).gameObject);
        }

        foreach (Transform t in view.slots)
        {
            if (t.childCount > 0)
                Destroy(t.GetChild(0).gameObject);
        }

        view.itemsLeftText.gameObject.SetActive(false);
        view.buttonEnd.onClick.RemoveAllListeners();
        view.buttonRoll.onClick.RemoveAllListeners();
        view.buttonLeave.onClick.RemoveAllListeners();
        StopAllCoroutines();
        timeLeft = timeForTurn;
        view.timeUI.transform.GetChild(0).GetComponent<Image>().fillAmount = timeLeft / (float)timeForTurn;
        view.timeUI.transform.GetChild(0).GetComponent<Image>().color = Color.Lerp(new Color(1, 0.1f, 0.1f), new Color(0, 1, 0), timeLeft / (float)timeForTurn);
        view.timeUI.text = timeForTurn.ToString();
        base.ExitState();
    }

    private void Update()
    {
        receiveAndProcessNetworkMessages();

        //store dice in order so they don't cross paths
        int c = 0;
        foreach (Transform t in view.slots) 
            if (t.childCount > 0 && t.GetChild(0).GetComponent<Dice>().toBeStored)
                c++;

        if (c == needToBeStored)
            foreach (Transform t in view.slots)
                if (t.GetChild(0).GetComponent<Dice>().getStopState() != DiceInfo.State.FOOT)
                t.GetChild(0).GetComponent<Dice>().StartStoring();

        //logig for the buttons and texts to do their thing when all the dice are done moving
        if (myTurn)
        {
            List<Dice> notRolling = new List<Dice>();
            foreach (Dice d in rollingDice)
                if (d.Finished)
                    notRolling.Add(d);

            foreach (Dice d in notRolling)
            {
                rollingDice.Remove(d);
                if (rollingDice.Count == 0)
                {
                    if (shots >= 3)//turn ended (lost) 
                    {
                        view.infoText.text = "You got shot 3 times";
                        endButton = true;
                        rollButton = false;
                    }
                    else if (shots + brains > 10)// turn ended(no enough dice)
                    {
                        view.infoText.text = "Not enough dice to roll";
                        endButton = true;
                        rollButton = false;
                    }
                    else
                        view.infoText.text = "It is your turn";

                    showButtons();
                }
            }
        }
    }

    protected override void handleNetworkMessage(ASerializable pMessage)
    {
        if (!view.endOverlay.gameObject.activeSelf)
        {

            if (pMessage is StartGame) handleStartGame(pMessage as StartGame);
            else if (pMessage is ScoreBoardUpdate) handleScoreBoardUpdate(pMessage as ScoreBoardUpdate);
            else if (pMessage is DiceBagUpdate) handleDiceBagUpdate(pMessage as DiceBagUpdate);
        }
        if (pMessage is RoomJoinedEvent) handleRoomJoinedEvent(pMessage as RoomJoinedEvent);
    }

    //update the scores
    private void handleScoreBoardUpdate(ScoreBoardUpdate pUpdate)
    {
        ScoreBoard.EndGame result = view.scoreboard.SetData(pUpdate.scores, username);
        showResults(result);
    }

    //if the server throws the player back to the lobby follow the order
    private void handleRoomJoinedEvent(RoomJoinedEvent pJ)
    {
        if (pJ.room == RoomJoinedEvent.Room.LOBBY_ROOM)
        {
            fsm.ChangeState<LobbyState>();
        }
    }

    //game started
    private void handleStartGame(StartGame start)
    {
        view.scoreboard.CreateData(start.names, start.username);
        username = start.username;
        oldBag = new DiceBag().diceBag;
        firstUpdate = true;
        ResetTimer();
    }

    //update the dice with the new bag provided by the server
    private void handleDiceBagUpdate(DiceBagUpdate pUpdate)
    {

        //check if its the same turn or a new one 
        if (view.scoreboard.SetTurn(pUpdate.owner)||firstUpdate)
        {
            firstUpdate = false;
            ResetTimer();
            reRollIndeces.Clear();
            reRollPos.Clear();
            foreach (Transform t in storageTransforms)
            {
                if (t.childCount > 0)
                    Destroy(t.GetChild(0).gameObject);
                t.DetachChildren();
            }

            if(pUpdate.owner == username)
            {
                myTurn = true;
                view.buttonRoll.gameObject.SetActive(true);
                view.infoText.text = "It is your turn";
            }
            else
            {
                myTurn = false;
                view.buttonEnd.gameObject.SetActive(false);
                view.buttonRoll.gameObject.SetActive(false);
                if (pUpdate.owner.Length>15)
                    view.infoText.text = "It is " + pUpdate.owner.Substring(0,15) + "'s turn";
                else
                    view.infoText.text = "It is " + pUpdate.owner + "'s turn";
            }
        }
        int green = 0;
        int red = 0;
        int yellow = 0;

        needToBeStored = 3;

        //removes old dice
        foreach (Transform t in view.slots)
        {
            if (t.childCount > 0)
                Destroy(t.GetChild(0).gameObject);
            t.DetachChildren();
        }
        List<int> nextReRollIndices = new List<int>();
        List<int> nextReRollPos = new List<int>();

        //rerolls dice that are foot
        for (int i=0; i<reRollIndeces.Count ; i++)
        {
            Dice d = Instantiate(view.dicePrefab, view.slots[reRollPos[i]]).GetComponent<Dice>();
            d.storageTransforms = storageTransforms;
            d.SetColor(pUpdate.bag.diceBag[reRollIndeces[i]].color);
            d.SetStopState(pUpdate.bag.diceBag[reRollIndeces[i]].state);
            if (myTurn)
                rollingDice.Add(d);

            if (pUpdate.bag.diceBag[reRollIndeces[i]].state == DiceInfo.State.FOOT)
            {
                nextReRollIndices.Add(reRollIndeces[i]);
                nextReRollPos.Add(reRollPos[i]);
                needToBeStored--;
            }
        }

        reRollIndeces= nextReRollIndices;
        reRollPos=nextReRollPos;

        shots = 0;
        brains = 0;

        for (int i =0; i<13; i++)
        {
            // count shots so we can inform the player if he lost
            if (pUpdate.bag.diceBag[i].state == DiceInfo.State.SHOT)
                shots++;

            // count brains so we can know if the player can still roll more dice
            if (pUpdate.bag.diceBag[i].state == DiceInfo.State.BRAIN)
                brains++;

            //dice left
            if (pUpdate.bag.diceBag[i].state == DiceInfo.State.UNROLLED)
            {
                if (pUpdate.bag.diceBag[i].color == DiceInfo.Color.GREEN) green++;
                else if (pUpdate.bag.diceBag[i].color == DiceInfo.Color.RED) red++;
                else if (pUpdate.bag.diceBag[i].color == DiceInfo.Color.YELLOW) yellow++;
            }
            //new rolled dice
            else if (oldBag[i].state == DiceInfo.State.UNROLLED)
            {
                if(myTurn)
                    endButton=true;

                int pos = 0;
                foreach (Transform t in view.slots)
                {
                    if (t.childCount == 0)
                    {
                        Dice d = Instantiate(view.dicePrefab, t).GetComponent<Dice>();
                        d.storageTransforms = storageTransforms;
                        d.SetColor(pUpdate.bag.diceBag[i].color);
                        d.SetStopState(pUpdate.bag.diceBag[i].state);
                        if (myTurn)
                            rollingDice.Add(d);
                        break;
                    }
                    pos++; 
                }

                if (pUpdate.bag.diceBag[i].state == DiceInfo.State.FOOT)
                {
                    reRollIndeces.Add(i);
                    reRollPos.Add(pos);
                    needToBeStored--;
                }
            }
            oldBag[i] = pUpdate.bag.diceBag[i];
        }


        view.itemsLeftText.text = "Dice left in bag:\nGreen x" + green + "\nYellow x" + yellow + "\nRed x" + red;
    }

    //hides buttos storing theis state
    private void hideButtons()
    {
        rollButton = view.buttonRoll.gameObject.activeSelf;
        endButton = view.buttonEnd.gameObject.activeSelf;
        view.buttonEnd.gameObject.SetActive(false);
        view.buttonRoll.gameObject.SetActive(false);
    }

    //rests buttons to their stored states
    private void showButtons()
    {
        view.buttonEnd.gameObject.SetActive(endButton);
        view.buttonRoll.gameObject.SetActive(rollButton);
    }

    // fisplays the final results screen
    private void showResults(ScoreBoard.EndGame r)
    {
        if (ScoreBoard.EndGame.FALSE == r) return;

        view.endOverlay.gameObject.SetActive(true);
        if (ScoreBoard.EndGame.WON == r)
            view.endOverlay.Set(true,view.scoreboard, view.buttonLeave.onClick);

        else if (ScoreBoard.EndGame.LOST == r)
            view.endOverlay.Set(false, view.scoreboard, view.buttonLeave.onClick);
    }

    //requests the end of current turn to server
    private void endTurn()
    {
        ActionRequest request = new ActionRequest();
        request.action = ActionRequest.Action.ENDTURN;
        fsm.channel.SendMessage(request);
        hideButtons();
    }

    //requests next roll to server
    private void rollAgain()
    {
        ActionRequest request = new ActionRequest();
        request.action = ActionRequest.Action.ROLL;
        fsm.channel.SendMessage(request);
        view.infoText.text = "Rolling...";
        hideButtons();
    }

    //request the server to leave the game
    private void leaveGame()
    {
        LeaveRequest request = new LeaveRequest();
        fsm.channel.SendMessage(request);
    }

    //resets the local timer (used to aproximately show the turn timeouts from the server timer)
    private void ResetTimer()
    {
        StopAllCoroutines();
        timeLeft = timeForTurn;
        view.timeUI.text = timeForTurn.ToString();
        StartCoroutine(timeDown());
    }

    //timer, note that this timer is just to give the player some feedback about how much time he has to play.
    //the actual timer is enforced on the server side
    public IEnumerator timeDown()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (timeLeft > 0)
            {
                view.timeUI.text = (--timeLeft).ToString();
                view.timeUI.transform.GetChild(0).GetComponent<Image>().fillAmount = timeLeft / (float)timeForTurn;
                view.timeUI.transform.GetChild(0).GetComponent<Image>().color = Color.Lerp(new Color(1, 0.1f, 0.1f), new Color(0, 1, 0), timeLeft / (float)timeForTurn);
            }
        }
    }
}

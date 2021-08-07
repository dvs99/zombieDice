using shared;
using System;
using System.Collections.Generic;
using System.Timers;

namespace server
{
	/**
	 * This room runs a single Game.
	 */
	class GameRoom : Room
	{
		public bool IsGameInPlay { get; private set; }
		private int turn = 0;

		//score board helper
		private ScoreManager scoreBoardManager = new ScoreManager();

		//dice helper
		private DiceManager diceManager = new DiceManager();

		//posible states for the room
		private enum RoomState { FILLING, PLAYING, ENDING, ERROR }
		private RoomState state = RoomState.FILLING;

		//stores all dice for the active player
		private DiceBag bag = new DiceBag();

		//stores the dice with state==foot so we can reroll them
		private Stack<DiceInfo> mustReRoll = new Stack<DiceInfo>();

		List<string> aux = new List<string>();

		int timeForTurn = 90;
		int timeLeft = 90;
		Timer timer;



		public GameRoom(TCPGameServer pOwner) : base(pOwner)
		{
		}

		public void StartGame(TcpMessageChannel[] playerArray)
		{
			//this if should never happen to be true unless by programmer error
			if (playerArray.Length != _server.MAX_PLAYERS_PER_GAME) throw new Exception("There is not the right amount of players in the game. This shouldn't be happening :(");
			
			//start the game by adding players and setting up the timer
			IsGameInPlay = true;
			foreach (TcpMessageChannel player in playerArray)
				addMember(player);

			timer = new Timer(1000);
			timer.Elapsed += timeDown;
			timer.Start();
		}

		//timer tick action (just counts down seconds)
		private void timeDown(Object source, System.Timers.ElapsedEventArgs e)
		{
			timeLeft--;
		}

		protected override void addMember(TcpMessageChannel pMember)
		{
			base.addMember(pMember);

			//add to temp list
			aux.Add(_server.GetPlayerInfo(pMember).username);

			//notify client he has joined a game room 
			RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
			roomJoinedEvent.room = RoomJoinedEvent.Room.GAME_ROOM;
			pMember.SendMessage(roomJoinedEvent);

			//if full start game
			if (memberCount == _server.MAX_PLAYERS_PER_GAME)
			{
				//shuffle the order of the players
				//add players to the scoreboard
				foreach (string user in Shuffle(aux))
					scoreBoardManager.AddName(user);

				safeForEach(sendStart);

				state = RoomState.PLAYING;

				//send a dice bag with the player that's starting username and just unrolled dice
				DiceBagUpdate update = new DiceBagUpdate();
				update.bag = bag;
				update.owner = scoreBoardManager.GetNames()[turn];
				sendToAll(update);
			}
		}

		//simple brute force shuffling
		//note can be slow for long lists but max length is 10 for ours, doesn't work for empty strings
		public static string[] Shuffle(List<string> values)
		{
			int lenght = values.Count;
			Random r = new Random();
			string[] aux = new string[lenght];

			for (int i = 0; i < lenght; i++)
				aux[i]="";

			for (int i = 0; i < lenght; i++)
			{
				int rn = r.Next(0, lenght);
				while (aux[rn]!="")
				{
					rn = r.Next(0, lenght);
				}
				aux[rn] = values[i];
			}
			return aux;
		}

		private void sendStart(TcpMessageChannel pMember)
		{
			StartGame start = new StartGame();
			start.username = _server.GetPlayerInfo(pMember).username;
			start.names = scoreBoardManager.GetNames();
			pMember.SendMessage(start);
		}

		public override void Update()
		{
			base.Update();
			if (state == RoomState.PLAYING)
			{
				//if the time ends server enforce a normal turn end
				if (timeLeft <= 0)
				{
					EndTurn(true);
				}
				//if the player leaves finish their turn, without regard of how many points they had
				if (!scoreBoardManager.IsPlayerAlive(turn))
				{
					EndTurn(false);
				}
			}
			else if (state == RoomState.ENDING)
			{
				//when time ends after the game finished just send players to the lobby
				if (timeLeft <= 0)
				{
					safeForEach(handleLeaveRequest);

					_server.DestroyRoom(this);
				}
			}
			else if (state == RoomState.ERROR)
			{
				//send everyone to the lobby
				safeForEach(_server.GetLobbyRoom().AddMember);

				//Inform the player about why he has been revmoved from the game
				ChatMessage cMessage = new ChatMessage();
				cMessage.message = "There was an error on your game, please join a new game!";
				sendToAll(cMessage);

				safeForEach(handleLeaveRequest);


				_server.DestroyRoom(this);
			}
		}


		protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
		{
			if (pMessage is ActionRequest && state == RoomState.PLAYING) handleActionRequest(pMessage as ActionRequest, pSender);
			if (pMessage is LeaveRequest) handleLeaveRequest(pSender);
		}

		private void handleLeaveRequest(TcpMessageChannel pSender)
		{
			//send  to the lobby
			_server.GetLobbyRoom().AddMember(pSender);

			ChatMessage cMessage = new ChatMessage();
			cMessage.message = "You are back to the lobby, you can join a new game!";
			pSender.SendMessage(cMessage);

			removeMember(pSender);

		}

		private void handleActionRequest(ActionRequest pRequest, TcpMessageChannel pSender)
		{
			if (scoreBoardManager.indexOfPlayer(_server.GetPlayerInfo(pSender).username) != turn)
				return;

			if (pRequest.action == ActionRequest.Action.ENDTURN)
			{	//if the player end the turn check if we should add the brains from the bag to the score by counting the shots
				if (diceManager.ThreeShotsInBag(bag))
					EndTurn(false);
				else
					EndTurn(true);
			}
			else if (pRequest.action == ActionRequest.Action.ROLL)
			{
				if (diceManager.ThreeShotsInBag(bag)) //server enforce that the client can't keep rolling after bieng shot 3 times in a turn, should never happen with properly behaving clients
				{
					EndTurn(false);
					return;
				}

				if (diceManager.UnrollableBag(bag)) //server enforce an end of turn if there are less than 3 dice to roll available 
				{
					EndTurn(true);
					return;
				}

				// reroll foot dices
				int diceLeft = 3 - (mustReRoll.Count);
				while (mustReRoll.Count > 0)
				{
					DiceInfo dice = diceManager.RollDice(mustReRoll.Pop());
					if (dice.state == DiceInfo.State.FOOT)
						mustReRoll.Push(dice);
				}

				//roll new dices
				while (diceLeft > 0)
				{
					int diceIndex = diceManager.GetRandomUnrolledDiceIndexInBag(bag);
					DiceInfo dice = diceManager.RollDice(diceManager.GetDiceAtIndexInBag(bag,diceIndex));
					if (dice.state == DiceInfo.State.FOOT)
						mustReRoll.Push(dice);
					diceLeft--;
				}

				//send the new dicebag
				DiceBagUpdate update = new DiceBagUpdate();
				update.owner = scoreBoardManager.GetNames()[turn];
				update.bag = bag;
				sendToAll(update);
			}
		}

		private void EndTurn(bool addScore)
		{
			timer.Stop();
			if (addScore) //add the score to the bag and notify the players
			{
				bool ended = scoreBoardManager.AddPoints(turn, diceManager.CountPoints(bag));
				ScoreBoardUpdate boardUpdate = new ScoreBoardUpdate();
				boardUpdate.scores = scoreBoardManager.scoreBoard;
				sendToAll(boardUpdate);

				//win condition: points>=13
				if (ended)
				{
					endGame();
					return;
				}
			}

			//make sure we don't pass the turn to an unexisting player
			if (++turn == 10) turn = 0;
			while (!scoreBoardManager.IsPlayerAlive(turn))
				if (++turn == 10) turn = 0;

			//reset internal collections
			bag = new DiceBag();
			mustReRoll.Clear();

			//notify everyone of the new player and its dicebag
			DiceBagUpdate bagUpdate = new DiceBagUpdate();
			bagUpdate.owner = scoreBoardManager.GetNames()[turn];
			bagUpdate.bag = bag;
			sendToAll(bagUpdate);
			timeLeft = timeForTurn;
			timer.Start();
		}

		protected override void removeMember(TcpMessageChannel pMember)
		{
			if (state != RoomState.ENDING) //remove the leaving player from teh scoreboard and send it to every client
			{
				scoreBoardManager.RemovePlayerWithName(_server.GetPlayerInfo(pMember).username);
				ScoreBoardUpdate boardUpdate = new ScoreBoardUpdate();
				boardUpdate.scores = scoreBoardManager.scoreBoard;
				sendToAll(boardUpdate);
			}

			base.removeMember(pMember);

			//if just one player finish the game
			if (state == RoomState.PLAYING)
			{
				if (memberCount <= 1)
				{
					endGame();
				}
			}
			else if (state != RoomState.ENDING)
				state = RoomState.ERROR;
		}

		//sets the room in terminal state
		private void endGame()
		{
			timer.Stop();
			state = RoomState.ENDING;
			timeLeft = 12;
			timer.Start();
		}
	}
}

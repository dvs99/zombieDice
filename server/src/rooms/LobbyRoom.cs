using shared;
using System.Collections.Generic;

namespace server
{
	/**
	 * The LobbyRoom is a little bit more extensive than the LoginRoom.
	 * In this room clients change their 'ready status'.
	 * If enough people are ready, they are automatically moved to the GameRoom to play a Game (assuming a game is not already in play).
	 */ 
	class LobbyRoom : SimpleRoom
	{
		//this list keeps tracks of which players are ready to play a game, this is a subset of the people in this room
		private List<TcpMessageChannel> _readyMembers = new List<TcpMessageChannel>();

		public LobbyRoom(TCPGameServer pOwner) : base(pOwner)
		{
		}

		protected override void addMember(TcpMessageChannel pMember)
		{
			base.addMember(pMember);

			//tell the member it has joined the lobby
			RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
			roomJoinedEvent.room = RoomJoinedEvent.Room.LOBBY_ROOM;
			pMember.SendMessage(roomJoinedEvent);

			//print some info in the new client lobby
			ChatMessage cMessage = new ChatMessage();
			cMessage.message = "You have joined the lobby! Your username is '"+_server.GetPlayerInfo(pMember).username+"'.";
			pMember.SendMessage(cMessage);

			//put info about the new client at every chat
			cMessage.message = "User '" + _server.GetPlayerInfo(pMember).username + "' has joined the lobby.";

			//send information to all clients that the lobby count has changed
			sendLobbyUpdateCount();
		}

		/**
		 * Override removeMember so that our ready count and lobby count is updated (and sent to all clients)
		 * anytime we remove a member.
		 */
		protected override void removeMember(TcpMessageChannel pMember)
		{
			base.removeMember(pMember);
			_readyMembers.Remove(pMember);

			sendLobbyUpdateCount();
		}

		protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
		{
			if (pMessage is ChangeReadyStatusRequest) handleReadyNotification(pMessage as ChangeReadyStatusRequest, pSender);
			if (pMessage is ChatMessage) handleChatMessage(pMessage as ChatMessage, pSender);
		}

		private void handleReadyNotification(ChangeReadyStatusRequest pReadyNotification, TcpMessageChannel pSender)
		{
			//if the given client was not marked as ready yet, mark the client as ready
			if (pReadyNotification.ready)
			{
				if (!_readyMembers.Contains(pSender)) _readyMembers.Add(pSender);
			}
			else //if the client is no longer ready, unmark it as ready
			{
				_readyMembers.Remove(pSender);
			}

			//do we have enough people for a game?
			if (_readyMembers.Count >= _server.MAX_PLAYERS_PER_GAME)
			{
				TcpMessageChannel[] playerList = new TcpMessageChannel[_server.MAX_PLAYERS_PER_GAME];
				TcpMessageChannel player;
				for (int i = 0; i < _server.MAX_PLAYERS_PER_GAME; i++)
				{
					player = _readyMembers[0];
					removeMember(player);
					playerList[i]=player;
				}
				_server.NewGameRoom().StartGame(playerList);
			}

			//(un)ready-ing / starting a game changes the lobby/ready count so send out an update
			//to all clients still in the lobby
			sendLobbyUpdateCount();
		}

		private void handleChatMessage(ChatMessage cMessage, TcpMessageChannel pSender)
		{
			//prepend username and send to every server
			cMessage.message = "[" + _server.GetPlayerInfo(pSender).username + "]" + cMessage.message;
			sendToAll(cMessage);
		}

		private void sendLobbyUpdateCount()
		{
			LobbyInfoUpdate lobbyInfoMessage = new LobbyInfoUpdate();
			lobbyInfoMessage.memberCount = memberCount;
			lobbyInfoMessage.readyCount = _readyMembers.Count;
			lobbyInfoMessage.maxGameSize = _server.MAX_PLAYERS_PER_GAME;
			sendToAll(lobbyInfoMessage);
		}
	}
}

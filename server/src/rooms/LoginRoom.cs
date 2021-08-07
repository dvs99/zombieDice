using shared;

namespace server
{
	/**
	 * The LoginRoom is the first room clients 'enter' until the client identifies himself with a PlayerJoinRequest. 
	 * If the client sends the wrong type of request, it will be kicked.
	 *
	 * A connected client that never sends anything will be stuck in here for life,
	 * unless the client disconnects (that will be detected in due time).
	 */ 
	class LoginRoom : SimpleRoom
	{
		public LoginRoom(TCPGameServer pOwner) : base(pOwner)
		{
		}

		protected override void addMember(TcpMessageChannel pMember)
		{
			base.addMember(pMember);

			//notify the client that they are now in the login room, clients can wait for that before doing anything else
			RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
			roomJoinedEvent.room = RoomJoinedEvent.Room.LOGIN_ROOM;
			pMember.SendMessage(roomJoinedEvent);
		}

		protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
		{
			if (pMessage is PlayerJoinRequest)
			{
				handlePlayerJoinRequest(pMessage as PlayerJoinRequest, pSender);
			}
			else //if member send something else than a PlayerJoinRequest
			{
				Log.LogInfo("Declining client, auth request not understood", this);

				//don't provide info back to the member on what it is we expect, just close and remove
				removeAndCloseMember(pSender);
			}
		}

		/**
		 * Tell the client he is accepted and move the client to the lobby room.
		 */
		private void handlePlayerJoinRequest (PlayerJoinRequest pMessage, TcpMessageChannel pSender)
		{
			PlayerJoinResponse playerJoinResponse = new PlayerJoinResponse();
			if (pMessage.name == "") //serveer enforce rule about empty strings
			{
				Log.LogInfo("Join request rejected. Name can't be an empty string.", this);
				playerJoinResponse.result = PlayerJoinResponse.RequestResult.GENERICERROR;
			}
			else if (_server.GetPlayerInfo(x => x.username == pMessage.name).Count > 0) //if a player with that username already exists
			{
				Log.LogInfo("Join request rejected. Name already exists.", this);
				playerJoinResponse.result = PlayerJoinResponse.RequestResult.USEDNAME;
			}
			else //accept the client and move it to the right room
			{
				Log.LogInfo("Moving new client to accepted...", this);

				playerJoinResponse.result = PlayerJoinResponse.RequestResult.ACCEPTED;
				PlayerInfo pI=_server.GetPlayerInfo(pSender);
				pI.username = pMessage.name;
				removeMember(pSender);
				_server.GetLobbyRoom().AddMember(pSender);
			}

			pSender.SendMessage(playerJoinResponse);

		}

	}
}

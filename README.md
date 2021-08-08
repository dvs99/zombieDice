# Zombie Dice
This individual project is an online playable version of the board game Zombie Dice. The game client has been developed using the using Unity game engine in combination
with a game server programmed using C# too so that some code can be shared between them. It has been developed without using Unity's integrated networking
features because its development was aimed at being a useful coding exercise about networked games in general instead of Unity specific. Therefore it has been
fully developed using TCP protocol sockets for client-server and server-client communication. This is
just a programming exercise, no copyright infringement intended. All copyrighted content belongs to its respective copyright owners. 

## How to setup
Download the server from the releases page on this repository and run it (you can change the number of players per game by running it with the amount you want as a parameter).
If not playing in a local network make sure to enable port forwarding on your router and firewall for TCP packets on port 55555. Finally, download the client from the releases page on this repository on every PC you want to play at, run it and connect to your server's IP and you will join the lobby and be able to queue for games.

## Portfolio relevance
I think this project highlights my ability to develop structured and functional object oriented code a somewhat big project that requires a fair amount of planning by myself.
It also showcases that I'm able to produce network related code both server-side and client-side.

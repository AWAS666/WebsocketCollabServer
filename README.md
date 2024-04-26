# WebsocketCollabServer

Used as a collab hub for ai vtubers to collab on twitch, though can likely be used for other applications aswell.
Uses Discord as UI.
Create an account on discord which is used in the websocket auth.
Create a room that anyone with the id can access as websocket communication and subscribe to.

The intend is to be a more lightweight and simpler solution than RMQ, but also lacking more nice to have features.

## Libraries
- [DisCatSharp](https://github.com/Aiko-IT-Systems/DisCatSharp)
- [WebsocketSharp](http://sta.github.io/websocket-sharp/)

## Todo
- [ ] restart, recreate rooms or not
- [ ] client demos in varying programming languages
- [ ] hosted server and discord that goes along with it
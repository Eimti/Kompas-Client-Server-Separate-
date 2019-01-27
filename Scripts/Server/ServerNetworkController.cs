﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerNetworkController : NetworkController {

    private int recievedConnectionID; //the connection id that is recieved with the network transport layer packet

    void Update()
    {
        if (!hosting) return; //self-explanatory
        if (!(Game.mainGame is ServerGame)) return; //this controller should only deal with the networking if the current game is a server game.

        //the "out" arguments in the following method mean that the things are being passed by reference,
        //and will be changed by the function

        recData = NetworkTransport.Receive(out hostID, out recievedConnectionID, out channelID,
            recBuffer, BUFFER_SIZE, out dataSize, out error);
        switch (recData)
        {
            //nothing
            case NetworkEventType.Nothing:
                break;
            //someone has tried to connect to me
            case NetworkEventType.ConnectEvent:
                //yay someone connected to me with the connectionID that was just set
                Debug.Log("Connected!");
                connected = true;
                //add the player. if it's the second player, tell each player the other is here
                if (ServerGame.mainServerGame.AddPlayer(recievedConnectionID) == 2)
                {

                }
                //TODO on second player connect, notify both players of their opponent also TODO change server scene to have correct things attached to main cam

                break;
            //they've actually sent something
            case NetworkEventType.DataEvent:
                ParseRequest(recBuffer, recievedConnectionID);
                break;
            //the person has disconnected
            case NetworkEventType.DisconnectEvent:
                break;
            //i don't know what this does and i don't 
            case NetworkEventType.BroadcastEvent:
                break;
        }

    }


    private void SendPackets(Packet outPacket, Packet outPacketInverted, ServerGame serverGame, int connectionID)
    {
        //if it's not null, send the normal packet to the player that queried you
        if (outPacket != null)
            Send(outPacket, connectionID);


        //if the inverted packet isn't null, send the packet to the correct player
        if (outPacketInverted == null) return;
        //if the one that queried you is player 0,
        if (connectionID == serverGame.Players[0].ConnectionID)
            //send the inverted one to player 1.
            Send(outPacketInverted, serverGame.Players[1].ConnectionID);
        //if the one that queried you is player 1,
        else
            //send the inverted one to player 0.
            Send(outPacketInverted, serverGame.Players[0].ConnectionID);

    }

    private void ParseRequest(byte[] buffer, int connectionID)
    {
        Packet packet = Deserialize(buffer);
        if (packet == null) return;

        Packet outPacket = null;
        Packet outPacketInverted = null;
        int playerIndex = ServerGame.mainServerGame.GetPlayerIndexFromID(connectionID);
        packet.InvertForController(playerIndex);

        //switch between all the possible requests for the server to handle.
        switch (packet.command)
        {
            case Packet.Command.AddToDeck:
                //figure out who's getting the card to their deck
                Player owner = ServerGame.mainServerGame.Players[playerIndex];
                //add the card in, with the cardCount being the card id, then increment the card count
                Card added = owner.deckCtrl.AddCard(packet.args, ServerGame.mainServerGame.cardCount++);
                //let everyone know
                outPacket = new Packet(Packet.Command.AddToDeck, packet.args, added.ID);
                outPacketInverted = new Packet(Packet.Command.AddToEnemyDeck, packet.args, added.ID);
                SendPackets(outPacket, outPacketInverted, ServerGame.mainServerGame, connectionID);
                break;
            case Packet.Command.Play:
                //get the card to play
                Card toPlay = ServerGame.mainServerGame.GetCardFromID(packet.cardID);
                //if it's not a valid place to do, return
                if (!ServerGame.mainServerGame.ValidBoardPlay(toPlay, packet.x, packet.y)) return;
                //play the card here
                ServerGame.mainServerGame.Play(toPlay, packet.x, packet.y);
                //tell everyone to do it
                outPacket = new Packet(Packet.Command.Play, toPlay, packet.x, packet.y);
                outPacketInverted = new Packet(Packet.Command.Play, toPlay, packet.x, packet.y, true);
                SendPackets(outPacket, outPacketInverted, ServerGame.mainServerGame, connectionID);
                break;
            case Packet.Command.Move:
                //get the card to move
                Card toMove = ServerGame.mainServerGame.GetCardFromID(packet.cardID);
                //if it's not a valid place to do, return
                if (!ServerGame.mainServerGame.ValidMove(toMove, packet.x, packet.y)) return;
                //play the card here
                ServerGame.mainServerGame.Move(toMove, packet.x, packet.y);
                //tell everyone to do it
                outPacket = new Packet(Packet.Command.Move, toMove, packet.x, packet.y);
                outPacketInverted = new Packet(Packet.Command.Move, toMove, packet.x, packet.y, true);
                SendPackets(outPacket, outPacketInverted, ServerGame.mainServerGame, connectionID);
                break;
            case Packet.Command.Topdeck:
                Card toTopdeck = ServerGame.mainServerGame.GetCardFromID(packet.cardID);
                //eventually, this won't be necessary, because the player won't initiate this action
                ServerGame.mainServerGame.Topdeck(toTopdeck);
                //and let everyone know
                outPacket = new Packet(Packet.Command.Topdeck, toTopdeck);
                outPacketInverted = outPacket;
                SendPackets(outPacket, outPacketInverted, ServerGame.mainServerGame, connectionID);
                break;
            case Packet.Command.Discard:
                Card toDiscard = ServerGame.mainServerGame.GetCardFromID(packet.cardID);
                //eventually, this won't be necessary, because the player won't initiate this action
                ServerGame.mainServerGame.Discard(toDiscard);
                //and let everyone know
                outPacket = new Packet(Packet.Command.Discard, toDiscard);
                outPacketInverted = outPacket;
                SendPackets(outPacket, outPacketInverted, ServerGame.mainServerGame, connectionID);
                break;
            case Packet.Command.Rehand:
                Card toRehand = ServerGame.mainServerGame.GetCardFromID(packet.cardID);
                //eventually, this won't be necessary, because the player won't initiate this action
                ServerGame.mainServerGame.Rehand(toRehand);
                //and let everyone know
                outPacket = new Packet(Packet.Command.Rehand, toRehand);
                outPacketInverted = outPacket;
                SendPackets(outPacket, outPacketInverted, ServerGame.mainServerGame, connectionID);
                break;
            default:
                Debug.Log("Invalid command " + packet.command + " to server from " + connectionID);
                break;
        }
    }
}

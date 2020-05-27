﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KompasNetworking;
using System.Net.Sockets;

public class ServerPlayer : Player
{
    public ServerPlayer ServerEnemy;
    public ServerGame serverGame;
    public ServerNetworkController ServerNetworkCtrl;
    public ServerNotifier ServerNotifier;

    public override Player Enemy => ServerEnemy;

    public override void SetInfo(TcpClient tcpClient, int index)
    {
        base.SetInfo(tcpClient, index);

        ServerNetworkCtrl.SetInfo(tcpClient);
    }

    //If the player tries to do something, it goes here to check if it's ok, then do it if it is ok.
    #region Player Control Methods
    /// <summary>
    /// x and y here are from playerIndex's perspective
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void TryAugment(AugmentCard aug, int x, int y)
    {
        if (serverGame.ValidAugment(aug, x, y, this)) serverGame.Play(aug, x, y, this);
        else ServerNotifier.NotifyPutBack();
    }

    public void TryPlay(Card card, int x, int y)
    {
        if (serverGame.ValidBoardPlay(card, x, y, this)) serverGame.Play(card, x, y, this);
        else ServerNotifier.NotifyPutBack();
    }

    public void TryMove(Card toMove, int x, int y)
    {
        Debug.Log($"Requested move {toMove?.CardName} to {x}, {y}");
        //if it's not a valid place to do, put the cards back
        if (serverGame.ValidMove(toMove, x, y)) serverGame.MoveOnBoard(toMove, x, y, true);
        else ServerNotifier.NotifyPutBack();
    }

    /// <summary>
    /// If it is a valid action to take, activates the effect, adding it to the stack and suchlike
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="controller"></param>
    public void TryActivateEffect(ServerEffect effect)
    {
        Debug.Log($"Player {index} trying to activate effect of {effect?.Source?.CardName}");
        if (effect.CanBeActivatedBy(this))
        {
            serverGame.EffectsController.PushToStack(effect, this);
            serverGame.EffectsController.CheckForResponse();
        }
    }

    public void TryAttack(CharacterCard attacker, CharacterCard defender)
    {
        ServerNotifier.NotifyBothPutBack();

        if (serverGame.ValidAttack(attacker, defender, this))
            serverGame.Attack(attacker, defender, this);
    }
    #endregion Player Control Methods
}

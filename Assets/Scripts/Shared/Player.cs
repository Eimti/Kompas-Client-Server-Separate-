﻿using KompasCore.Cards;
using KompasCore.Effects;
using KompasCore.GameCore;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public abstract class Player : MonoBehaviour{

    public abstract Player Enemy { get; }

    //game mechanics data
    private int pips = 3;
    public virtual int Pips
    {
        get => pips;
        set => pips = value;
    }
    private GameCard avatar;
    public virtual GameCard Avatar
    {
        get => avatar;
        set => avatar = value;
    }

    //other game data
    public abstract bool Friendly { get; }
    //this is not a property so it can be assigned in the inspector for client players
    public int index;

    //friendly
    public DeckController deckCtrl;
    public DiscardController discardCtrl;
    public HandController handCtrl;
    public AnnihilationController annihilationCtrl;

    //friendly
    public GameObject deckObject;
    public GameObject discardObject;
    public GameObject handObject;

    public TcpClient TcpClient { get; private set; }

    /// <summary>
    /// Whether the player has yet passed priority
    /// </summary>
    public bool passedPriority = false;

    public virtual void SetInfo(TcpClient tcpClient, int index)
    {
        TcpClient = tcpClient;
        this.index = index;
    }

    public Space SubjectiveCoords(Space space) => index == 0 ? space : space.Inverse;

    public override string ToString()
    {
        return $"Player {index}";
    }
}

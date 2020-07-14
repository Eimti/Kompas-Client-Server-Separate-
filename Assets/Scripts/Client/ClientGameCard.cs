﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientGameCard : GameCard
{
    public ClientGame ClientGame { get; protected set; }
    public override Game Game => ClientGame;

    public override CardLocation Location
    {
        get => base.Location;
        set
        {
            base.Location = value;
            ClientGame.clientUICtrl.Leyload = Game.Leyload;
        }
    }

    private ClientPlayer clientController;
    public ClientPlayer ClientController
    {
        get => clientController;
        set
        {
            clientController = value;
            cardCtrl?.SetRotation();
        }
    }
    public override Player Controller
    {
        get => ClientController;
        set => ClientController = value as ClientPlayer;
    }

    public ClientPlayer ClientOwner { get; private set; }
    public override Player Owner => ClientOwner;

    public ClientEffect[] ClientEffects { get; private set; }
    public override IEnumerable<Effect> Effects => ClientEffects;
    public override bool IsAvatar => false;

    public virtual void SetInfo(SerializableCard serializedCard, ClientGame game, ClientPlayer owner, ClientEffect[] effects, int id)
    {
        base.SetInfo(serializedCard, id);
        ClientGame = game;
        ClientController = ClientOwner = owner;
        ClientEffects = effects;
    }
}

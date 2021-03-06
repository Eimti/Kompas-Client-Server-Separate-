﻿using KompasClient.Effects;
using KompasClient.GameCore;
using KompasCore.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KompasCore.Networking
{
    public class HandSizeToStackPacket : Packet
    {
        public int controllerIndex;

        public HandSizeToStackPacket() : base(HandSizeToStack) { }

        public HandSizeToStackPacket(int controllerIndex) : this()
        {
            this.controllerIndex = controllerIndex;
        }

        public override Packet Copy() => new HandSizeToStackPacket(controllerIndex);

        public override Packet GetInversion(bool known = true) => new HandSizeToStackPacket(1 - controllerIndex);
    }
}

namespace KompasClient.Networking
{
    public class HandSizeToStackClientPacket : HandSizeToStackPacket, IClientOrderPacket
    {
        public void Execute(ClientGame clientGame)
        {
            var controller = clientGame.ClientPlayers[controllerIndex];
            clientGame.clientEffectsCtrl.Add(new ClientHandSizeStackable(controller));
        }
    }
}
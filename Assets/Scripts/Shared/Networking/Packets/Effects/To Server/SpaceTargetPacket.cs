﻿using KompasCore.Cards;
using KompasCore.Networking;
using KompasServer.GameCore;
using KompasServer.Effects;
using System.Threading.Tasks;

namespace KompasCore.Networking
{
    public class SpaceTargetPacket : Packet
    {
        public int x;
        public int y;

        public SpaceTargetPacket() : base(SpaceTargetChosen) { }

        public SpaceTargetPacket(int x, int y) : this()
        {
            this.x = x;
            this.y = y;
        }

        public override Packet Copy() => new SpaceTargetPacket(x, y);
    }
}

namespace KompasServer.Networking
{
    public class SpaceTargetServerPacket : SpaceTargetPacket, IServerOrderPacket
    {
        public Task Execute(ServerGame serverGame, ServerPlayer player, ServerAwaiter awaiter)
        {
            if(player.index != 0)
            {
                x = 6 - x;
                y = 6 - y;
            }

            awaiter.SpaceTarget = (x, y);
            return Task.CompletedTask;
        }
    }
}
﻿using KompasClient.GameCore;
using KompasCore.Networking;
using KompasServer.GameCore;

namespace KompasCore.Networking
{
    public class MoveActionPacket : Packet
    {
        public int cardId;
        public int x;
        public int y;

        public MoveActionPacket() : base(MoveAction) { }

        public MoveActionPacket(int cardId, int x, int y) : this()
        {
            this.cardId = cardId;
            this.x = x;
            this.y = y;
        }

        public override Packet Copy() => new MoveActionPacket(cardId, x, y);
    }
}

namespace KompasServer.Networking
{
    public class MoveActionServerPacket : MoveActionPacket, IServerOrderPacket
    {
        public void Execute(ServerGame serverGame, ServerPlayer player)
        {
            if (player.index == 1)
            {
                x = 6 - x;
                y = 6 - y;
            }
            var card = serverGame.GetCardWithID(cardId);
            player.TryMove(card, x, y);
        }
    }
}
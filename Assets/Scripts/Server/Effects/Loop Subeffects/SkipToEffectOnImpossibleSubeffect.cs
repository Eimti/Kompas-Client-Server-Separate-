﻿using System.Threading.Tasks;

namespace KompasServer.Effects
{
    /// <summary>
    /// Resolves a specified subeffect if at any point the effect is declared impossible
    /// </summary>
    public class SkipToEffectOnImpossibleSubeffect : ServerSubeffect
    {
        public int jumpTo;

        public override Task<ResolutionInfo> Resolve()
        {
            ServerEffect.OnImpossible = this;
            return Task.FromResult(ResolutionInfo.Next);
        }

        public override Task<ResolutionInfo> OnImpossible(string why)
        {
            //forget about this effect on impossible, and jump to a new one
            ServerEffect.OnImpossible = null;
            return Task.FromResult(ResolutionInfo.Index(jumpTo));
        }
    }
}

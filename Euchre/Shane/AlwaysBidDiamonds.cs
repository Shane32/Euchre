using System;
using System.Linq;

namespace Euchre.Shane
{
    public class AlwaysBidDiamonds : JumpingPC
    {
        public AlwaysBidDiamonds() : base() { }

        public override Bid GetBid()
        {
            // Always order up diamonds (not alone)
            if (Game.Phase == GamePhase.BidRound1)
            {
                if (Game.RevealedCard.Suit == Suit.Diamonds)
                {
                    return new Bid(false, Suit.Diamonds);
                }
            }
            else if (Game.Phase == GamePhase.BidRound2)
            {
                // In round 2, bid diamonds if it's not the revealed card suit
                if (Game.RevealedCard.Suit != Suit.Diamonds)
                {
                    return new Bid(false, Suit.Diamonds);
                }
            }
            
            // Otherwise, pass
            return null;
        }
    }
}
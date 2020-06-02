using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Euchre.Shane
{
    public class AITrainer : JumpingPC
    {
        public AITrainer() : base("AI Trainer") { }

        private Suit SuitToBid;
        private bool DoBid;
        private bool BidAlone;

        public virtual void SetBid(Suit suit, bool bid, bool alone)
        {
            SuitToBid = suit;
            DoBid = bid;
            BidAlone = alone;
        }

        public virtual float[] GetAIHandStats()
        {
            var variables = new float[34];
            var index = 0;
            var suits = new Suit[4];
            suits[0] = Game.RevealedCard.Suit;
            suits[1] = suits[0] switch
            {
                Suit.Clubs => Suit.Spades,
                Suit.Spades => Suit.Clubs,
                Suit.Hearts => Suit.Diamonds,
                _ => Suit.Hearts
            };
            suits[2] = suits[0] switch
            {
                Suit.Clubs => Suit.Hearts,
                Suit.Spades => Suit.Hearts,
                _ => Suit.Clubs
            };
            suits[3] = suits[0] switch
            {
                Suit.Clubs => Suit.Diamonds,
                Suit.Spades => Suit.Diamonds,
                _ => Suit.Spades,
            };
            for (int suitIndex = 0; suitIndex < 4; suitIndex++) {
                var suit = suits[suitIndex];
                for (int cardIndex = 0; cardIndex < 6; cardIndex++)
                {
                    var card = new Card(cardIndex, suit);
                    var holdingCard = Cards.Any(x => x == card);
                    var flippedUp = RevealedCard == card;
                    variables[index++] = holdingCard ? 1f : 0f;
                    if (suitIndex == 0)
                        variables[index++] = flippedUp ? 1f : 0f;
                }
            }
            var meDealer = Game.Dealer == Me;
            var nextPlayer = Game.GetNextPlayer(Me);
            var dealer2 = Game.Dealer == nextPlayer;
            nextPlayer = Game.GetNextPlayer(nextPlayer);
            var dealer3 = Game.Dealer == nextPlayer;
            nextPlayer = Game.GetNextPlayer(nextPlayer);
            var dealer4 = Game.Dealer == nextPlayer;
            variables[index++] = meDealer ? 1f : 0f;
            variables[index++] = dealer2 ? 1f : 0f;
            variables[index++] = dealer3 ? 1f : 0f;
            variables[index++] = dealer4 ? 1f : 0f;
            return variables;
        }

        public override Bid GetBid()
        {
            if (Game.Phase == GamePhase.BidRound1)
            {
                var trump = Game.RevealedCard.Suit;
                if (trump == SuitToBid && DoBid) return new Bid(BidAlone, trump);
            }
            else
            {
                if (DoBid) return new Bid(BidAlone, SuitToBid);
            }
            return null;
        }
    }
}

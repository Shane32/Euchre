using System;
using System.Collections.Generic;
using System.Text;

namespace Euchre
{
    public abstract class Player
    {
        public readonly string Name;
        protected List<Card> Cards { get; } = new List<Card>(6);
        protected Game Game { get; private set; }

        public Player(string name)
        {
            Name = name;
        }

        public void NewDeal(Game game)
        {
            Game = game;
            Cards.Clear();
            OnNewDeal(game);
        }
        public void GiveCard(Card card)
        {
            Cards.Add(card);
            OnGiveCard(card);
        }

        protected virtual void OnNewDeal(Game game) { }

        protected virtual void OnGiveCard(Card card) { }

        /// <summary>
        /// Return a bid or null if no bid
        /// </summary>
        /// <returns></returns>
        public abstract Bid GetBid();

        /// <summary>
        /// Pick up the specified card into your hand, and then discard one
        /// </summary>
        /// <param name="card"></param>
        /// <returns>The card to discard</returns>
        public abstract Card PickUpCard(Card card);

        /// <summary>
        /// Return a card to play from your hand
        /// </summary>
        /// <returns></returns>
        public abstract Card GetCard();

        /// <summary>
        /// Executes after a trick is played out
        /// </summary>
        public abstract void TrickFinished(Player takenBy, Team teamTakenBy);

        /// <summary>
        /// Executes after a hand is played out
        /// </summary>
        public abstract void HandFinished();

        /// <summary>
        /// Executes after a round has started, before bidding begins. Cards have been dealt and one flipped up
        /// </summary>
        /// <param name="me">A reference to yourself, as stored in the Players and Teams variables</param>
        public abstract void StartRound(Player me);

        /// <summary>
        /// Executes after a player has bid; examine Bid and BiddingPlayer; RevealedCard is still set
        /// </summary>
        public abstract void BiddingFinished();
    }

    public class Bid
    {
        public readonly bool Alone;
        public readonly Suit Suit;

        public Bid(bool alone, Suit suit)
        {
            Alone = alone;
            Suit = suit;
        }
    }
}

using Euchre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visualizer
{
    internal class PlayerMonitor : Player
    {
        private Player player;

        public PlayerMonitor(Player player) : base((player ?? throw new ArgumentNullException(nameof(player))).Name)
        {
            this.player = player;
        }

        protected override void OnNewDeal(Game game) => player.NewDeal(game);

        protected override void OnGiveCard(Card card) => player.GiveCard(card);

        public List<Card> PeekCards() => Cards;

        public override void BiddingFinished() => player.BiddingFinished();

        public override Bid GetBid() => player.GetBid();

        public override Card GetCard()
        {
            var card = player.GetCard();
            Cards.Remove(card);
            return card;
        }

        public override void HandFinished() => player.HandFinished();

        public override Card PickUpCard(Card card)
        {
            Cards.Add(card);
            var discard = player.PickUpCard(card);
            Cards.Remove(discard);
            return discard;
        }

        public override void StartRound(Player me) => player.StartRound(me);

        public override void TrickFinished(Player takenBy, Team teamTakenBy) => player.TrickFinished(takenBy, teamTakenBy);
    }
}

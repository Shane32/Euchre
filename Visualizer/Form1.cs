using Euchre;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visualizer
{
    public partial class Form1 : Form
    {
        private Cards Cards = new Cards();
        private Game Game;
        private List<PlayerMonitor> Players = new List<PlayerMonitor>();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            Players.Clear();
            Players.Add(new PlayerMonitor(new Euchre.Shane.JumpingPC()));
            Players.Add(new PlayerMonitor(new StupidAI()));
            Players.Add(new PlayerMonitor(new Euchre.Shane.JumpingPC()));
            Players.Add(new PlayerMonitor(new StupidAI()));
            Game = new Game(Players[0], Players[2], Players[1], Players[3]);
            //Cards.Initialize();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            /*
             * Required attrition notice for open source playing cards:
             * 
             * Vectorized Playing Cards 3.1
             * https://totalnonsense.com/open-source-vector-playing-cards/
             * Copyright 2011,2019 – Chris Aguilar – conjurenation at gmail dot com
             * Licensed under: LGPL 3.0 – https://www.gnu.org/licenses/lgpl-3.0.html
             * 
             */
            if (Game == null) { base.OnPaint(e); return; }
            var cardSpacingX = 17;
            var cardSpacingY = 35;
            var cardWidth = Cards.CardWidth;
            var cardHeight = Cards.CardHeight;
            var clientRect = this.ClientRectangle;
            clientRect.Y = this.btnStartGame.Top * 2 + this.btnStartGame.Height;
            clientRect.Height -= clientRect.Y + this.btnStartGame.Top;
            clientRect.X = this.btnStartGame.Left;
            clientRect.Width -= clientRect.X * 2;
            var topPlayerPos = new Point(clientRect.X + (clientRect.Width - cardWidth * 2 - cardSpacingX * 5) / 2 + cardWidth + cardSpacingX, clientRect.Y);
            var leftPlayerPos = new Point(clientRect.X, clientRect.Y + (clientRect.Height - cardHeight * 2 - cardSpacingY * 5) / 2 + cardHeight + cardSpacingY);
            var bottomPlayerPos = topPlayerPos;
            bottomPlayerPos.Y = clientRect.Height - cardHeight;
            var rightPlayerPos = leftPlayerPos;
            rightPlayerPos.X = clientRect.Width - cardWidth;
            var topBottomPlayerBidOffsetX = -cardWidth - cardSpacingX;
            var leftRightPlayerBidOffsetY = -cardHeight - cardSpacingY;
            var centerX = clientRect.X + clientRect.Width / 2 - cardWidth / 2;
            var centerY = clientRect.Y + clientRect.Height / 2 - cardHeight / 2;
            var topPlayerCard = new Point(centerX, centerY - cardHeight / 2);
            var leftPlayerCard = new Point(centerX - cardWidth / 2, centerY);
            var rightPlayerCard = new Point(centerX + cardWidth / 2, centerY);
            var bottomPlayerCard = new Point(centerX, centerY + cardHeight / 2);

            for (int playerNum = 0; playerNum < 4; playerNum++)
            {
                var player = Players[playerNum];
                Point cardsPos;
                Point nextCardOffset;
                Point bidOffset;
                switch (playerNum)
                {
                    case 0:
                        cardsPos = bottomPlayerPos;
                        nextCardOffset = new Point(cardSpacingX, 0);
                        bidOffset = new Point(topBottomPlayerBidOffsetX, 0);
                        break;
                    case 1:
                        cardsPos = leftPlayerPos;
                        nextCardOffset = new Point(0, cardSpacingY);
                        bidOffset = new Point(0, leftRightPlayerBidOffsetY);
                        break;
                    case 2:
                        cardsPos = topPlayerPos;
                        nextCardOffset = new Point(cardSpacingX, 0);
                        bidOffset = new Point(topBottomPlayerBidOffsetX, 0);
                        break;
                    case 3:
                        cardsPos = rightPlayerPos;
                        nextCardOffset = new Point(0, cardSpacingY);
                        bidOffset = new Point(0, leftRightPlayerBidOffsetY);
                        break;
                    default:
                        throw new ApplicationException();
                }
                if (Game.Phase != GamePhase.Deal)
                {
                    var cards = player.PeekCards();
                    for (int cardNum = 0; cardNum < cards.Count; cardNum++)
                    {
                        Cards.Draw(e.Graphics, cards[cardNum], cardsPos.X + nextCardOffset.X * cardNum, cardsPos.Y + nextCardOffset.Y * cardNum);
                    }
                }
                if (Game.Dealer == player)
                {
                    if (Game.Phase == GamePhase.BidRound1)
                    {
                        Cards.Draw(e.Graphics, Game.RevealedCard, cardsPos.X + bidOffset.X, cardsPos.Y + bidOffset.Y);
                    }
                    else
                    {
                        Cards.Draw(e.Graphics, null, cardsPos.X + bidOffset.X, cardsPos.Y + bidOffset.Y);
                    }
                }
            }
            if (Game.Phase >= GamePhase.GamePlay)
            {
                foreach (var play in Game.CardsInPlay)
                {
                    Point point;
                    if (play.PlayedBy == Players[0]) point = bottomPlayerCard;
                    else if (play.PlayedBy == Players[1]) point = leftPlayerCard;
                    else if (play.PlayedBy == Players[2]) point = topPlayerCard;
                    else if (play.PlayedBy == Players[3]) point = rightPlayerCard;
                    else throw new InvalidOperationException();
                    Cards.Draw(e.Graphics, play.Card, point.X, point.Y);
                }
            }

            base.OnPaint(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        protected override void OnResize(EventArgs e)
        {
            this.Invalidate();
            base.OnResize(e);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (Game != null)
            {
                Game.Next();
                string str = $"Score: {Game.Teams[0].Score} to {Game.Teams[1].Score}";
                if (Game.Phase >= GamePhase.GamePlay)
                {
                    str += $" === Tricks: {Game.TricksTaken[0]} to {Game.TricksTaken[1]} === Trump: {Game.Bid?.Suit} {(Game.Bid?.Alone ?? false ? "Alone" : "")} ";
                } else
                {
                    str += $" === Bidding now";
                }
                lblStatus.Text = str;
            }
            this.Invalidate();
        }
    }
}

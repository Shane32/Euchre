using Euchre;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Visualizer
{
    class Cards
    {
        public static Bitmap Image = Properties.Resources.CardImages;
        public const int CardWidth = 100;
        public const int CardHeight = 140;
        public void Draw(Graphics graphics, Card card, int x, int y)
        {
            Rectangle srcRect = new Rectangle(0, 0, CardWidth, CardHeight);
            Rectangle destRect = new Rectangle(x, y, CardWidth, CardHeight);
            //9 10 11-J 12-Q 13-K 14-A 15-left 16-right
            if (card == null)
            {
                srcRect.X = CardWidth * 6;
            }
            else
            {
                if (card.Number == 16)
                {
                    card = new Card(11, card.Suit);
                }
                else if (card.Number == 15)
                {
                    switch (card.Suit)
                    {
                        case Suit.Clubs: card = new Card(11, Suit.Spades); break;
                        case Suit.Spades: card = new Card(11, Suit.Clubs); break;
                        case Suit.Hearts: card = new Card(11, Suit.Diamonds); break;
                        case Suit.Diamonds: card = new Card(11, Suit.Hearts); break;
                        default: throw new ArgumentOutOfRangeException(nameof(card.Suit));
                    }
                }
                switch (card.Suit)
                {
                    case Suit.Clubs: srcRect.Y = CardHeight; break;
                    case Suit.Spades: srcRect.Y = 0; break;
                    case Suit.Hearts: srcRect.Y = CardHeight * 2; break;
                    case Suit.Diamonds: srcRect.Y = CardHeight * 3; break;
                    default: throw new ArgumentOutOfRangeException(nameof(card.Suit));
                }
                if (card.Number < 9 || card.Number > 14) throw new ArgumentOutOfRangeException(nameof(card.Number));
                srcRect.X = (card.Number - 9) * CardWidth;
            }
            graphics.DrawImage(Image, destRect, srcRect, GraphicsUnit.Pixel);
        }
        //public void CombineFiles()
        //{
        //    using (var newImage = new Bitmap(7 * CardWidth, 4 * CardHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    using (var g = Graphics.FromImage(newImage))
        //    using (var penBlack = new Pen(Brushes.Black, 1))
        //    using (var penTransparent = new Pen(Brushes.Transparent, 1))
        //    {
        //        for (int suit = 0; suit < 4; suit++)
        //        {
        //            for (int card = 0; card < 7; card++)
        //            {
        //                string filename = "C:\\Users\\Shane\\Downloads\\Vector-Cards-Version-3.1\\Vector Cards (Version 3.1)\\";
        //                if (card < 6)
        //                {
        //                    filename += "FACES (BORDERED)\\STANDARD BORDERED\\";
        //                    switch (card)
        //                    {
        //                        case 0: filename += "9"; break;
        //                        case 1: filename += "10"; break;
        //                        case 2: filename += "j"; break;
        //                        case 3: filename += "q"; break;
        //                        case 4: filename += "k"; break;
        //                        case 5: filename += "a"; break;
        //                        default: throw new InvalidOperationException();
        //                    }
        //                    switch (suit)
        //                    {
        //                        case 0: filename += "s"; break;
        //                        case 1: filename += "c"; break;
        //                        case 2: filename += "h"; break;
        //                        case 3: filename += "d"; break;
        //                        default: throw new InvalidOperationException();
        //                    }
        //                }
        //                else
        //                {
        //                    filename += "BACKS-PIPS-NUMBERS\\back";
        //                }
        //                filename += ".png";
        //                var cardImage = Bitmap.FromFile(filename);
        //                var cardLocation = new Point(card * CardWidth, suit * CardHeight);
        //                g.DrawImage(cardImage, new Rectangle(cardLocation.X, cardLocation.Y, CardWidth, CardHeight), 0, 0, CardWidth, CardHeight, GraphicsUnit.Pixel);
        //                g.DrawLine(penTransparent, cardLocation.X, cardLocation.Y, cardLocation.X + 1, cardLocation.Y);
        //                g.DrawLine(penTransparent, cardLocation.X + CardWidth + 1, cardLocation.Y, cardLocation.X + CardWidth, cardLocation.Y);
        //                g.DrawLine(penTransparent, cardLocation.X, cardLocation.Y + CardHeight - 1, cardLocation.X + 1, cardLocation.Y + CardHeight - 1);
        //                g.DrawLine(penTransparent, cardLocation.X + CardWidth + 1, cardLocation.Y + CardHeight - 1, cardLocation.X + CardWidth, cardLocation.Y + CardHeight - 1);
        //                g.DrawLine(penBlack, cardLocation.X + 1, cardLocation.Y, cardLocation.X + CardWidth - 2, cardLocation.Y);
        //                g.DrawLine(penBlack, cardLocation.X + 1, cardLocation.Y + CardHeight - 1, cardLocation.X + CardWidth - 2, cardLocation.Y + CardHeight - 1);
        //                g.DrawLine(penBlack, cardLocation.X, cardLocation.Y + 1, cardLocation.X, cardLocation.Y + CardHeight - 2);
        //                g.DrawLine(penBlack, cardLocation.X + CardWidth - 1, cardLocation.Y + 1, cardLocation.X + CardWidth - 1, cardLocation.Y + CardHeight - 2);
        //            }
        //        }
        //        newImage.Save("C:\\Users\\Shane\\Downloads\\Vector-Cards-Version-3.1\\Vector Cards (Version 3.1)\\out.png", System.Drawing.Imaging.ImageFormat.Png);
        //    }
        //}
    }
}

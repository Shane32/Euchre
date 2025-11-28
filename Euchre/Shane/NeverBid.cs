using System;

namespace Euchre.Shane
{
    public class NeverBid : JumpingPC
    {
        public NeverBid() : base() { }

        public override Bid GetBid()
        {
            // Never bid
            return null;
        }
    }
}
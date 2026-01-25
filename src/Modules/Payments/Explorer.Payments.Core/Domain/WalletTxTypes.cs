using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Payments.Core.Domain
{
    public static class WalletTxTypes
    {
        public const int AdminTopUp = 1;        
        public const int CheckoutPurchase = 2;
        public const int WelcomeBonusAc = 3;    
        public const int RankRewardAc = 4;     
    }
}

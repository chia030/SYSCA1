using System.Collections.Generic;

namespace SharedModels
{
    public class CreditStandingChangedMessage
    {
        public int CustomerId { get; set; }
        public int PaidAmount { get; set; }
    }
}

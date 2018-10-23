using System.Collections.Generic;
using System.Linq;
using INN.Core.Models;
using INN.Core.Models.DTO;

namespace EPiServer.Reference.Commerce.Site.Features.Inn
{
    public class InnViewModel
    {
        public InnStatusResult InnStatusResult { get; set; }
        public IEnumerable<DeliveryAddress> Addresses { get; set; } = Enumerable.Empty<DeliveryAddress>();
        public DeliveryAddress PreselectedAddress { get; set; }
    }
}
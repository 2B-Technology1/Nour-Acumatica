/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.CL.Services.DataProviders
{
    public class RecipientEmailDataProvider : IRecipientEmailDataProvider
    {
        private readonly PXGraph graph;

        public RecipientEmailDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public string GetRecipientEmail(NotificationRecipient notificationRecipient, int? vendorId)
        {
            switch (notificationRecipient.ContactType)
            {
                case NotificationContactType.Primary:
                    return GetEmailForPrimaryVendor(vendorId);
                case NotificationContactType.Employee:
                    return GetContactEmail(notificationRecipient.ContactID);
                case NotificationContactType.Remittance:
                    return GetEmailForRemittanceContact(vendorId);
                case NotificationContactType.Shipping:
                    return GetEmailForShippingContact(vendorId);
                default:
                    return GetContactEmail(notificationRecipient.ContactID);
            }
        }

        private string GetEmailForRemittanceContact(int? vendorId)
        {
            var locationExtensionAddress = GetLocationExtensionAddress(vendorId);
            var contactId = locationExtensionAddress.VRemitContactID;
            return GetContactEmail(contactId);
        }

        private string GetEmailForShippingContact(int? vendorId)
        {
            var locationExtensionAddress = GetLocationExtensionAddress(vendorId);
            var contactId = locationExtensionAddress.DefContactID;
            return GetContactEmail(contactId);
        }

        private string GetContactEmail(int? contactId)
        {
            return graph.Select<Contact>().SingleOrDefault(c => c.ContactID == contactId)?.EMail;
        }

        private string GetEmailForPrimaryVendor(int? vendorId)
        {
            var locationExtensionAddress = GetLocationExtensionAddress(vendorId);
            return GetContactEmail(locationExtensionAddress.VDefContactID);
        }

        private Location GetLocationExtensionAddress(int? vendorId)
        {
            return SelectFrom<Location>
                .Where<Location.bAccountID.IsEqual<P.AsInt>>.View
                .Select(graph, vendorId);
        }
    }
}
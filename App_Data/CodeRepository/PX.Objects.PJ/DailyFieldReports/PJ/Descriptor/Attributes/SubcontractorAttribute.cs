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

using System;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.PO;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    [PXDBInt]
    [PXDefault]
    [PXUIField(DisplayName = "Vendor ID")]
    public sealed class SubcontractorAttribute : PXDimensionSelectorAttribute
    {
        private static readonly Type[] Fields =
        {
            typeof(Vendor.acctCD),
            typeof(Vendor.acctName),
            typeof(Address.addressLine1),
            typeof(Address.addressLine2),
            typeof(Address.postalCode),
            typeof(Contact.phone1),
            typeof(Address.city),
            typeof(Address.countryID),
            typeof(Location.taxRegistrationID),
            typeof(Vendor.curyID),
            typeof(Contact.attention),
            typeof(Vendor.vendorClassID),
            typeof(Vendor.vStatus)
        };

        private static readonly Type SearchType = typeof(SelectFrom<Vendor>
            .InnerJoin<POLine>.On<POLine.vendorID.IsEqual<Vendor.bAccountID>
                .And<POLine.projectID.IsEqual<DailyFieldReport.projectId.FromCurrent>>>
            .LeftJoin<Address>.On<Address.bAccountID.IsEqual<Vendor.bAccountID>>
            .LeftJoin<Contact>.On<Contact.bAccountID.IsEqual<Vendor.bAccountID>>
            .LeftJoin<Location>.On<Location.bAccountID.IsEqual<Vendor.bAccountID>>
            .AggregateTo<GroupBy<Vendor.bAccountID>>.SearchFor<Vendor.bAccountID>);

        public SubcontractorAttribute()
            : base("VENDOR", SearchType, typeof(Vendor.acctCD), Fields)
        {
            Filterable = true;
            FilterEntity = typeof(Vendor);
            CacheGlobal = true;
        }
    }
}
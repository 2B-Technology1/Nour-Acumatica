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

using PX.Objects.PJ.Common.Descriptor;
using PX.Common.Serialization;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Objects.CR;
using PX.Objects.PM;

namespace PX.Objects.PJ.RequestsForInformation.PM.DAC
{
    [PXSerializable]
    [PXCacheName(CacheNames.ProjectContact)]
    public class ProjectContact : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public int? ProjectContactId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(PMProject.contractID))]
        public int? ProjectId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search<BAccountR.bAccountID,
                Where<BAccountR.status, NotEqual<CustomerStatus.inactive>,
                    And<Where<BAccountR.type, Equal<BAccountType.vendorType>, 
                        Or<BAccountR.type, Equal<BAccountType.customerType>,
                        Or<BAccountR.type, Equal<BAccountType.combinedType>>>>>>>),
            typeof(BAccountR.acctCD),
            typeof(BAccountR.acctName),
            typeof(BAccountR.type),
            SubstituteKey = typeof(BAccountR.acctCD),
            DescriptionField = typeof(BAccountR.acctName))]
        [PXUIField(DisplayName = "Business Account")]
        public int? BusinessAccountId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.Visible, Required = true)]
        [DependsOnField(typeof(businessAccountId), ShouldDisable = false)]
        [PXSelector(typeof(Search<Contact.contactID,
                Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
                    And2<Where<Contact.bAccountID, Equal<Current<businessAccountId>>,
                            Or<Current<businessAccountId>, IsNull>>,
                        And<Contact.isActive, Equal<True>>>>>),
            SubstituteKey = typeof(Contact.contactID),
            DescriptionField = typeof(Contact.displayName),
            Filterable = true,
            DirtyRead = true)]
        public int? ContactId
        {
            get;
            set;
        }

        [PXString]
        [PXUnboundDefault(typeof(Search<Contact.eMail, Where<Contact.contactID, Equal<Current<contactId>>>>))]
        [PXUIField(DisplayName = "Email", IsReadOnly = true)]
        public string Email
        {
            get;
            set;
        }

        [PXString]
        [PXUnboundDefault(typeof(Search<Contact.phone1, Where<Contact.contactID, Equal<Current<contactId>>>>))]
        [PXUIField(DisplayName = "Phone", IsReadOnly = true)]
        public string Phone
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Role")]
        public string Role
        {
            get;
            set;
        }

        public abstract class projectContactId : IBqlField
        {
        }

        public abstract class projectId : PX.Data.BQL.BqlInt.Field<projectId>
        {
        }

        public abstract class businessAccountId : IBqlField
        {
        }

        public abstract class contactId : IBqlField
        {
        }

        public abstract class email : IBqlField
        {
        }

        public abstract class phone : IBqlField
        {
        }

        public abstract class role : IBqlField
        {
        }
    }
}
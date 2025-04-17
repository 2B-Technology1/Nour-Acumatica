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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CN.Common.DAC;
using PX.Objects.EP;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.PM.DAC
{
    [Serializable]
    [PXCacheName("Lien Waiver Recipient")]
    public class LienWaiverRecipient : BaseCache, IBqlTable
    {
        #region Key
        public new class PK : PrimaryKeyOf<LienWaiverRecipient>.By<lienWaiverRecipientId>
        {
	        public static LienWaiverRecipient Find(PXGraph graph, int? lienWaiverRecipientId, PKFindOptions options = PKFindOptions.None) =>
		        FindBy(graph, lienWaiverRecipientId, options);
        }

        public new static class FK
        {
            public class Project : PMProject.PK.ForeignKeyOf<LienWaiverRecipient>.By<projectId> { }
            public class VendorClass : Objects.AP.VendorClass.PK.ForeignKeyOf<LienWaiverRecipient>.By<vendorClassId> { }
        }
        #endregion

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField]
        public virtual bool? Selected
        {
            get;
            set;
        }

        [PXDBIdentity]
        public virtual int? LienWaiverRecipientId
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(PMProject.contractID))]
        [PXParent(typeof(FK.Project))]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Vendor Class")]
        [PXSelector(typeof(SearchFor<VendorClass.vendorClassID>.In<SelectFrom<VendorClass>
            .LeftJoin<EPEmployeeClass>.On<EPEmployeeClass.vendorClassID.IsEqual<VendorClass.vendorClassID>>
            .Where<EPEmployeeClass.vendorClassID.IsNull>>))]
        [PXParent(typeof(FK.VendorClass))]
        public virtual string VendorClassId
        {
            get;
            set;
        }

        [PXDBDecimal(MinValue = 0)]
        [PXDefault]
        [PXUIField(DisplayName = "Minimum Commitment Amount", Required = true)]
        public virtual decimal? MinimumCommitmentAmount
        {
            get;
            set;
        }

        public abstract class selected : BqlBool.Field<selected>
        {
        }

        public abstract class lienWaiverRecipientId : BqlInt.Field<lienWaiverRecipientId>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class vendorClassId : BqlString.Field<vendorClassId>
        {
        }

        public abstract class minimumCommitmentAmount : BqlDecimal.Field<minimumCommitmentAmount>
        {
        }
    }
}

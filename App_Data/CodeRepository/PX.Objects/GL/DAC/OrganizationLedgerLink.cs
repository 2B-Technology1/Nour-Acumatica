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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL.Attributes;

namespace PX.Objects.GL.DAC
{
	/// <summary>
	/// A many-to-many relation link between <see cref="Organization"/> and <see cref="Ledger"/> records. 
	/// </summary>
	[Serializable]
	public class OrganizationLedgerLink: IBqlTable
    {
		#region Keys
		public class PK : PrimaryKeyOf<OrganizationLedgerLink>.By<organizationID, ledgerID>
		{
			public static OrganizationLedgerLink Find(PXGraph graph, int? organizationID, int? ledgerID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, organizationID, ledgerID, options);
		}
		public static class FK
		{
			public class Organization : DAC.Organization.PK.ForeignKeyOf<OrganizationLedgerLink>.By<organizationID> { }
			public class Ledger : GL.Ledger.PK.ForeignKeyOf<OrganizationLedgerLink>.By<ledgerID> { }
		}
		#endregion

		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		// <summary>
		/// The reference to the associated <see cref="Organization"/> record.
		/// </summary>
		[PXParent(typeof(Select<Organization, Where<Organization.organizationID, Equal<Current<OrganizationLedgerLink.organizationID>>>>))]
		[Organization(true, typeof(Search<Organization.organizationID, Where<Organization.organizationType.IsNotEqual<OrganizationTypes.group>>>), null, IsKey = true, FieldClass = null)]
		public virtual int? OrganizationID { get; set; }
        #endregion

        #region LedgerID
        public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }

		/// <summary>
		/// The reference to the associated <see cref="Ledger"/> record.
		/// </summary>
		[PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = Messages.Ledger)]
        [PXSelector(
			typeof(Search<Ledger.ledgerID>),
			SubstituteKey = typeof(Ledger.ledgerCD),
			DescriptionField = typeof(Ledger.descr),
			DirtyRead =true)]
		[PXDBDefault(typeof(Ledger.ledgerID))]
		public virtual int? LedgerID { get; set; } 
        #endregion
    }
}

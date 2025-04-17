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
using PX.Objects.CR;
using PX.Objects.GL.DAC;

namespace PX.Objects.Localizations.GB.HMRC.DAC
{
	[PXProjection(typeof(Select2<Organization,
		InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Organization.bAccountID>>,
			LeftJoin<BAccountMTDApplication, On<BAccountMTDApplication.bAccountID, Equal<Organization.bAccountID>>>>,
		Where<True, Equal<True>>>))]
	[PXHidden]
	[Serializable]
	public partial class OrganizationTaxInfo : PX.Data.IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt(BqlField = typeof(Organization.bAccountID), IsKey = true)]
		public virtual Int32? BAccountID { get; set; }
		#endregion
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
		[PXDBInt(BqlField = typeof(Organization.organizationID))]
		public virtual Int32? OrganizationID { get; set; }
		#endregion
		#region FileTaxesByBranches
		public abstract class fileTaxesByBranches : PX.Data.BQL.BqlBool.Field<fileTaxesByBranches> { }
		[PXDBBool(BqlField = typeof(Organization.fileTaxesByBranches))]
		public virtual bool? FileTaxesByBranches { get; set; }
		#endregion
		#region TaxRegistrationID
		public abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }
		[PXDBString(50, IsUnicode = true, BqlField = typeof(BAccountR.taxRegistrationID))]
		public virtual String TaxRegistrationID { get; set; }
		#endregion
		#region ApplicationID
		[PXDBInt(BqlField = typeof(BAccountMTDApplication.applicationID))]
		public int? ApplicationID { get; set; }
		public abstract class applicationID : PX.Data.BQL.BqlInt.Field<applicationID> { }
		#endregion
	}
}

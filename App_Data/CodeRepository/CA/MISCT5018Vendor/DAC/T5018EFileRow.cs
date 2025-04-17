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

using PX.Data;
using System;
using PX.Data.BQL;
using PX.Objects.Localizations.CA.Messages;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.Localizations.CA.MISCT5018Vendor {
	/// <summary>
	/// Rows that are specific to vendor's summarized transactions for a T5018 revision. Each row belongs to <see cref="T5018MasterTable"/> and is comprised of one or multiple <see cref="APInvoiceEFileRevision"/> records.
	/// </summary>
	[Serializable]
	[PXCacheName("T5018 EFile Row")]
	public class T5018EFileRow : IBqlTable
	{
		public class PK : PrimaryKeyOf<T5018EFileRow>.By<organizationID, year, revision, bAccountID>
		{
			public static T5018EFileRow Find(PXGraph graph, int? organizationID, string year, string revision, int? bAccountID) =>
			   FindBy(graph, organizationID, year, revision, bAccountID);
		}

		#region Keys

		public static class FK
		{
			public class T5018MasterTableFK : T5018MasterTable.PK.ForeignKeyOf<T5018EFileRow>.By<organizationID, year, revision> { }
		}

		#endregion

		#region Organization ID
		public abstract class organizationID : BqlInt.Field<organizationID> { }
		/// <summary>
		/// The organization ID of associated <see cref="T5018MasterTable">revision</see>.
		/// The field is included in <see cref="FK.T5018MasterTableFK"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="T5018MasterTable.OrganizationID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXParent(typeof(FK.T5018MasterTableFK))]
		public virtual int? OrganizationID
		{
			get;
			set;
		}
		#endregion

		#region Year
		public abstract class year : BqlString.Field<year> { }
		/// <summary>
		/// The year of the associated <see cref="T5018MasterTable">revision</see>.
		/// The field is included in <see cref="FK.T5018MasterTableFK"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="T5018MasterTable.Year"/> field.
		/// </value>
		[PXDBString(4, IsKey = true)]
		public virtual string Year
		{
			get;
			set;
		}
		#endregion

		#region Revision
		public abstract class revision : BqlString.Field<revision> { }
		/// <summary>
		/// The revision number of the associated <see cref="T5018MasterTable">revision</see>.
		/// The field is included in <see cref="FK.T5018MasterTableFK"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="T5018MasterTable.Revision"/> field.
		/// </value>
		[PXDBString(3, IsKey = true)]
		[PXUIField(DisplayName = T5018Messages.Revision)]
		public virtual string Revision
		{
			get;
			set;
		}
		#endregion

		#region Organization Name
		public abstract class organizationName : BqlString.Field<organizationName> { }
		/// <summary>
		/// The name of the organization associated with the revision.
		/// </summary>
		[PXDBString(60)]
		[PXUIField(DisplayName = "Payer")]
		public virtual string OrganizationName
		{
			get;
			set;
		}
		#endregion

		#region BAccountID
		public abstract class bAccountID : BqlInt.Field<bAccountID> { }
		/// <summary>
		/// The <see cref="BAccount.BAccountID">business account ID</see> of the associated <see cref="BAccount">vendor's account</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public virtual int? BAccountID
		{
			get;
			set;
		}
		#endregion

		#region Amount
		public abstract class amount : BqlDecimal.Field<amount> { }
		/// <summary>
		/// The summed amount of <see cref="APAdjust"/> records applicable for the vendor in the revision.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Amount")]
		public virtual decimal? Amount
		{
			get;
			set;
		}
		#endregion

		#region Vendor Account CD
		public abstract class vAcctCD : BqlString.Field<vAcctCD> { }
		/// <summary>
		/// The vendor account CD.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="BAccount.AcctCD"/> field.
		/// </value>
		[PXDBString(30)]
		[PXUIField(DisplayName = "Vendor")]
		public virtual string VAcctCD
		{
			get;
			set;
		}
		#endregion

		#region Vendor Name
		public abstract class vendorName : BqlString.Field<vendorName> { }
		/// <summary>
		/// The name of the vendor.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="BAccount.AcctName"/> field.
		/// </value>
		[PXDBString(60)]
		[PXUIField(DisplayName = "Vendor Name")]
		public virtual string VendorName
		{
			get;
			set;
		}
		#endregion

		#region Tax Registration ID
		public abstract class taxRegistrationID : BqlString.Field<taxRegistrationID> { }
		/// <summary>
		/// The tax registration number of the vendor.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="T5018VendorExt.BusinessNumber"/> field.
		/// </value>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Tax Registration ID")]
		public virtual string TaxRegistrationID
		{
			get;
			set;
		}
		#endregion

		#region AmmendmentRow
		public abstract class amendmentRow : BqlBool.Field<amendmentRow> { }
		/// <summary>
		/// A Boolean value that indicates if the row should be included in an amendment.
		/// </summary>
		[PXDBBool]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		[PXDefault(false)]
		public virtual bool? AmendmentRow { get; set; }
		#endregion
	}
}

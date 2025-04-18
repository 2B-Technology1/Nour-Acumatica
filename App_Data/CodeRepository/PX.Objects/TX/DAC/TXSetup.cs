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
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.TX
{
	/// <summary>
	/// Provides access to the preferences of the Taxes module.
	/// Can be edited by user through the Tax Preferences (TX103000) form.
	/// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(TXSetupMaint))]
	[PXCacheName(Messages.TXSetupMaint)]
	public partial class TXSetup : PX.Data.IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class TaxRoundingGainAccount : GL.Account.PK.ForeignKeyOf<TXSetup>.By<taxRoundingGainAcctID> { }
			public class TaxRoundingGainSubaccount : GL.Sub.PK.ForeignKeyOf<TXSetup>.By<taxRoundingGainSubID> { }
			public class TaxRoundingLossAccount : GL.Account.PK.ForeignKeyOf<TXSetup>.By<taxRoundingLossAcctID> { }
			public class TaxRoundingLossSubaccount : GL.Sub.PK.ForeignKeyOf<TXSetup>.By<taxRoundingLossSubID> { }
			public class TaxAdjustmentNumberingSequence : CS.Numbering.PK.ForeignKeyOf<TXSetup>.By<taxAdjustmentNumberingID> { }
		}
		#endregion
		#region TaxAdjustmentNumberingID
		public abstract class taxAdjustmentNumberingID : PX.Data.BQL.BqlString.Field<taxAdjustmentNumberingID> { }
		/// <summary>
		/// The numbering sequence used for Tax Adjustments.
		/// </summary>
		/// <value>
		/// This field is a link to a <see cref="Numbering"/> record.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("APBILL")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Tax Adjustment Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String TaxAdjustmentNumberingID { get; set; }
		#endregion
		#region TaxRoundingGainAcctID
		public abstract class taxRoundingGainAcctID : PX.Data.BQL.BqlInt.Field<taxRoundingGainAcctID> { }
		/// <summary>
		/// An expense account to book positive discrepancy by the credit side.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXDefault]
		[Account(null,
			DisplayName = "Tax Rounding Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		[PXUIRequired(typeof(FeatureInstalled<CS.FeaturesSet.netGrossEntryMode>))]
		public virtual int? TaxRoundingGainAcctID { get; set; }
		#endregion
		#region TaxRoundingGainSubID
		public abstract class taxRoundingGainSubID : PX.Data.BQL.BqlInt.Field<taxRoundingGainSubID> { }
		/// <summary>
		/// A subaccount to book positive discrepancy by the credit side. Visible if Subaccounts feature is activated.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXDefault]
		[SubAccount(typeof(taxRoundingGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Tax Rounding Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXUIRequired(typeof(FeatureInstalled<CS.FeaturesSet.netGrossEntryMode>))]
		public virtual int? TaxRoundingGainSubID { get; set; }
		#endregion
		#region TaxRoundingLossAcctID
		public abstract class taxRoundingLossAcctID : PX.Data.BQL.BqlInt.Field<taxRoundingLossAcctID> { }
		/// <summary>
		/// An expense account to book negative discrepancy by the debit side.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXDefault]
		[Account(null,
			DisplayName = "Tax Rounding Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		[PXUIRequired(typeof(FeatureInstalled<CS.FeaturesSet.netGrossEntryMode>))]
		public virtual int? TaxRoundingLossAcctID { get; set; }
		#endregion
		#region TaxRoundingLossSubID
		public abstract class taxRoundingLossSubID : PX.Data.BQL.BqlInt.Field<taxRoundingLossSubID> { }
		/// <summary>
		/// A subaccount to book negative discrepancy by the debit side. Visible if Subaccounts feature is activated.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXDefault]
		[SubAccount(typeof(taxRoundingLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Tax Rounding Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXUIRequired(typeof(FeatureInstalled<CS.FeaturesSet.netGrossEntryMode>))]
		public virtual int? TaxRoundingLossSubID { get; set; }
		#endregion

		#region system fields
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion
	}
}

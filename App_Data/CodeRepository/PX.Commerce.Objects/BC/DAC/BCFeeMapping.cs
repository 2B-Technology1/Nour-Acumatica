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

using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using System;
using PX.Objects.CA;
using PX.Commerce.Core;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// Fee Mapping
	/// </summary>
	[Serializable]
	[PXCacheName("BCFeeMapping")]
	public class BCFeeMapping : IBqlTable
	{
		#region Keys

		public class PK : PrimaryKeyOf<BCFeeMapping>.By<BCFeeMapping.bindingID, BCFeeMapping.feeMappingID>
		{
			public static BCFeeMapping Find(PXGraph graph, int? bindingID, int? feeMappingID) => FindBy(graph, bindingID, feeMappingID);
		}

		public static class FK
		{
			public class Binding : BCBinding.BindingIndex.ForeignKeyOf<BCFeeMapping>.By<bindingID> { }
			public class EntryType : CAEntryType.PK.ForeignKeyOf<BCFeeMapping>.By<entryTypeID> { }
			public class PaymentMapping : BCPaymentMethods.PK.ForeignKeyOf<BCFeeMapping>.By<paymentMappingID> { }
		}
		#endregion


		#region BindingID
		/// <summary>
		/// The primary key and identity of the related <see cref="BCBinding"/>.
		/// </summary>
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
		[PXDBInt(IsKey = true)]
		[PXParent(typeof(Select<BCBinding,
			Where<BCBinding.bindingID, Equal<Current<BCFeeMapping.bindingID>>>>))]
		[PXDBDefault(typeof(BCBinding.bindingID))]
		public virtual int? BindingID { get; set; }
		/// <inheritdoc cref="BindingID"/>
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion

		#region FeeMappingID
		/// <summary>
		/// The record identity.
		/// </summary>
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
		[PXDBIdentity(IsKey = true)]
		public int? FeeMappingID { get; set; }
		/// <inheritdoc cref="FeeMappingID"/>
		public abstract class feeMappingID : PX.Data.BQL.BqlInt.Field<feeMappingID> { }
		#endregion

		#region PaymentMappingID
		/// <summary>
		/// The primary key and identity of the related <see cref="BCPaymentMethods"/>.
		/// </summary>
		[PXDBInt]
		[PXDBDefault(typeof(BCPaymentMethods.paymentMappingID))]
		public int? PaymentMappingID { get; set; }
		/// <inheritdoc cref="PaymentMappingID"/>
		public abstract class paymentMappingID : PX.Data.BQL.BqlInt.Field<paymentMappingID> { }
		#endregion

		#region FeeType
		/// <summary>
		/// The fee that is charged by Shopify for the order level.
		/// </summary>
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Fee Type")]
		[PXDefault]
		public virtual string FeeType { get; set; }
		/// <inheritdoc cref="FeeType"/>
		public abstract class feeType : PX.Data.BQL.BqlString.Field<feeType> { }
		#endregion

		#region EntryTypeID
		/// <summary>
		/// The corresponding entry type is chosen for the particular fee type.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "ERP Entry Type")]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId,
			InnerJoin<CashAccountETDetail, On<CAEntryType.entryTypeId, Equal<CashAccountETDetail.entryTypeID>>,
				InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CashAccountETDetail.cashAccountID>>>>,		
			Where<CAEntryType.drCr, Equal<CADrCr.cACredit>,
				And<CashAccount.active, Equal<True>,
				And<CAEntryType.accountID, IsNotNull,
				And<CashAccountETDetail.cashAccountID, Equal<Current<BCPaymentMethods.cashAccountID>>>>>>>),
			SubstituteKey = typeof(CAEntryType.entryTypeId),
			DescriptionField = typeof(CAEntryType.descr))]
		public virtual string EntryTypeID { get; set; }
		/// <inheritdoc cref="EntryTypeID"/>
		public abstract class entryTypeID : PX.Data.BQL.BqlString.Field<entryTypeID> { }
		#endregion

		#region Description
		/// <summary>
		/// The detailed description for the entry type.
		/// </summary>
		[PXString]
		[PXFormula(typeof(Selector<BCFeeMapping.entryTypeID, CAEntryType.descr>))]
		[PXUIField(DisplayName = "Entry Type Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual string EntryDescription { get; set; }
		/// <inheritdoc cref="EntryDescription"/>
		public abstract class entryDescription : PX.Data.BQL.BqlString.Field<entryDescription> { }
		#endregion

		#region TransactionType
		/// <summary>
		/// The type of transaction the entry type is based on.
		/// </summary>
		[CADrCr.List]
		[PXUIField(DisplayName = "Transaction Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXString]
		[PXFormula(typeof(Selector<BCFeeMapping.entryTypeID, CAEntryType.drCr>))]
		public virtual string TransactionType { get; set; }
		/// <inheritdoc cref="TransactionType"/>
		public abstract class transactionType : PX.Data.BQL.BqlString.Field<transactionType> { }
		#endregion
	}
}

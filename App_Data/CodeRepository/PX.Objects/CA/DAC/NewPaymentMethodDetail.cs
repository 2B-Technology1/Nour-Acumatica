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

namespace PX.Objects.CA
{
	[Serializable]
	[PXCacheName(nameof(NewPaymentMethodDetail))]
	public class NewPaymentMethodDetail : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion
		#region UseFor
		public abstract class useFor : PX.Data.BQL.BqlString.Field<useFor> { }

		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(PaymentMethodDetailUsage.UseForAll)]
		[PXUIField(DisplayName = "Used In")]
		public virtual string UseFor { get; set; }
		#endregion
		#region DetailIDInt
		public abstract class detailIDInt : PX.Data.BQL.BqlInt.Field<detailIDInt> { }

		[PXInt]
		public virtual int? DetailIDInt { get; set; }
		#endregion
		#region DetailID
		public abstract class detailID : PX.Data.BQL.BqlString.Field<detailID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "ID", Visible = true)]
		public virtual string DetailID { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description { get; set; }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Status")]
		public virtual string Status { get; set; }
		#endregion
		#region EntryMask
		public abstract class entryMask : PX.Data.BQL.BqlString.Field<entryMask> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Entry Mask")]
		public virtual string EntryMask { get; set; }
		#endregion
		#region ValidRegexp
		public abstract class validRegexp : PX.Data.BQL.BqlString.Field<validRegexp> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Validation Reg. Exp.")]
		public virtual string ValidRegexp { get; set; }
		#endregion
		#region DisplayMask
		public abstract class displayMask : PX.Data.BQL.BqlString.Field<displayMask> { }
		[PXDBString(255)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Display Mask", Enabled = false)]
		public virtual string DisplayMask { get; set; }
		#endregion
		#region IsRequired
		public abstract class isRequired : PX.Data.BQL.BqlBool.Field<isRequired> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Required")]
		public virtual bool? IsRequired { get; set; }
		#endregion
		#region ControlType
		public abstract class controlType : PX.Data.BQL.BqlInt.Field<controlType> { }

		[PXDBInt]
		[PXDefault((int)PaymentMethodDetailType.Text)]
		[PXUIField(DisplayName = "Control Type")]
		[PXIntList(new int[] { (int)PaymentMethodDetailType.Text, (int)PaymentMethodDetailType.AccountType }, new string[] { "Text", "Account Type List" })]
		public virtual int? ControlType { get; set; }
		#endregion
		#region DefaultValue
		public abstract class defaultValue : PX.Data.BQL.BqlString.Field<defaultValue> { }

		[PXDBString(255, IsUnicode = true)]
		[PXIntList(new int[] { (int)ACHPlugInBase.TransactionCode.CheckingAccount, (int)ACHPlugInBase.TransactionCode.SavingAccount }, new string[] { Messages.CheckingAccount, Messages.SavingAccount })]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Default Value")]
		public virtual string DefaultValue { get; set; }
		#endregion
	}

	public static class NewPaymentMethodDetailHelper
	{
		public static PaymentMethodDetail ToPaymentMethodDetail(this NewPaymentMethodDetail newPaymentMethodDetail, PaymentMethod paymentMethod)
		{
			return new PaymentMethodDetail
			{
				DetailID = newPaymentMethodDetail.DetailID,
				Descr = newPaymentMethodDetail.Description,
				ControlType = newPaymentMethodDetail.ControlType,
				DefaultValue = newPaymentMethodDetail.DefaultValue,
				EntryMask = newPaymentMethodDetail.EntryMask,
				IsRequired = newPaymentMethodDetail.IsRequired,
				UseFor = newPaymentMethodDetail.UseFor,
				OrderIndex = (short?)newPaymentMethodDetail.DetailIDInt,
				PaymentMethodID = paymentMethod.PaymentMethodID,
				ValidRegexp = newPaymentMethodDetail.ValidRegexp,
			};
		}
	}
}

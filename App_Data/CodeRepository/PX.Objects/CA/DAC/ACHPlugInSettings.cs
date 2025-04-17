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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.ACHPlugInBase;
using PX.Data;
using SCC = PX.ACHPlugInBase.ServiceClassCode;

namespace PX.Objects.CA
{
	[PXHidden]
	public class ACHPlugInSettings : IBqlTable
	{
		#region CheckBox
		public abstract class checkBox : PX.Data.BQL.BqlBool.Field<checkBox> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? CheckBox {	get; set; }

		#endregion
		#region PriorityCode
		public abstract class priorityCode : PX.Data.BQL.BqlString.Field<priorityCode> { }

		[PXDBString(2, IsUnicode = true, InputMask = "00")]
		[PXUIField(DisplayName = "Priority Code")]
		[PXDefault(DefaultValues.PriorityCode)]
		public string PriorityCode { get; set; }
		#endregion
		#region RemittanceSetting
		public abstract class remittanceSetting : PX.Data.BQL.BqlString.Field<remittanceSetting> { }

		[PXDBString(60, IsUnicode = true, InputMask = "")]
		[PXSelector(typeof(Search<PaymentMethodDetail.detailID,
				Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
					And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
						Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>),
			SubstituteKey = typeof(PaymentMethodDetail.descr), DirtyRead = true)]
		[PXDefault]
		public virtual string RemittanceSetting	{ get; set; }
		#endregion
		#region VendorSetting
		public abstract class vendorSetting : PX.Data.BQL.BqlString.Field<vendorSetting> { }

		[PXDBString(60, IsUnicode = true, InputMask = "")]
		[PXSelector(typeof(Search<PaymentMethodDetail.detailID,
				Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
					And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
						Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>),
			SubstituteKey = typeof(PaymentMethodDetail.descr), DirtyRead = true)]
		[PXDefault]
		public virtual string VendorSetting { get; set; }
		#endregion
		#region FileIDModifier
		public abstract class fileIDModifier : PX.Data.BQL.BqlString.Field<fileIDModifier> { }

		[PXDBString(1, IsUnicode = true, InputMask = "CCCCCCCC")]
		[FileIDModifier.List]
		[PXDefault(ACHPlugInBase.FileIDModifier.AZ09)]
		public virtual string FileIDModifier { get; set; }
		#endregion

		#region ServiceClassCode
		public abstract class serviceClassCode : PX.Data.BQL.BqlString.Field<serviceClassCode> { }

		[PXDBString(3)]
		[SCC.List]
		[PXDefault(SCC._200)]
		[PXUIField(DisplayName = "Service Class Code")]
		public string ServiceClassCode { get; set; }
		#endregion
		#region CompanyDiscretionaryData
		public abstract class companyDiscretionaryData : PX.Data.BQL.BqlString.Field<companyDiscretionaryData> { }

		[PXDBString(20, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string CompanyDiscretionaryData { get; set; }
		#endregion
		#region StandardEntryClassCode
		public abstract class standardEntryClassCode : PX.Data.BQL.BqlString.Field<standardEntryClassCode> { }

		[PXDBString(3, IsUnicode = true, InputMask = "")]
		[StandardEntryClassCode.List]
		[PXDefault(ACHPlugInBase.StandardEntryClassCode.CCD)]
		public virtual string StandardEntryClassCode { get; set; }
		#endregion
		#region CompanyEntryDescription
		public abstract class companyEntryDescription : PX.Data.BQL.BqlString.Field<companyEntryDescription> { }

		[PXDBString(10, IsUnicode = true, InputMask = "CCCCCCCCCC")]
		[PXUIField(DisplayName = "Company Entry Description")]
		[PXDefault(DefaultValues.CompanyEntryDescription)]
		public string CompanyEntryDescription { get; set; }
		#endregion
		#region OriginatorStatusCode
		public abstract class originatorStatusCode : PX.Data.BQL.BqlString.Field<originatorStatusCode> { }

		[PXDBString(1, IsUnicode = true, InputMask = "0")]
		[PXUIField(DisplayName = "Originator Status Code")]
		[PXDefault(DefaultValues.OriginatorStatusCode)]
		public virtual string OriginatorStatusCode { get; set; }
		#endregion
	}
}

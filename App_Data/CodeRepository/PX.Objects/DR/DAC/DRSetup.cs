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
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.DR
{
	[Serializable]
	[PXPrimaryGraph(typeof(DRSetupMaint))]
	[PXCacheName(Messages.DRSetup)]
	public class DRSetup: IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class DeferralScheduleNumberingSequence : CS.Numbering.PK.ForeignKeyOf<DRSetup>.By<scheduleNumberingID> { }
		}
		#endregion
		#region ScheduleNumberingID
		public abstract class scheduleNumberingID : PX.Data.BQL.BqlString.Field<scheduleNumberingID> { }

		/// <summary>
		/// The identifier of the <see cref="Numbering">Numbering Sequence</see> used for the Deferred Revenue.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Numbering.NumberingID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("DRSCHEDULE")]
		[PXUIField(DisplayName = "Deferral Schedule Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual String ScheduleNumberingID { get; set; }
		#endregion

		#region PendingRevenueValidate
		public abstract class pendingRevenueValidate : PX.Data.BQL.BqlBool.Field<pendingRevenueValidate> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? PendingRevenueValidate { get; set; }
		#endregion

		#region PendingExpenseValidate
		public abstract class pendingExpenseValidate : PX.Data.BQL.BqlBool.Field<pendingExpenseValidate> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? PendingExpenseValidate { get; set; }
		#endregion

		#region UseFairValuePricesInBaseCurrency
		public abstract class useFairValuePricesInBaseCurrency : PX.Data.BQL.BqlBool.Field<useFairValuePricesInBaseCurrency> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Fair Value Prices in Base Currency", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ASC606))]
		public virtual bool? UseFairValuePricesInBaseCurrency { get; set; }
		#endregion

		#region SuspenseAccountID
		public abstract class suspenseAccountID : PX.Data.BQL.BqlInt.Field<suspenseAccountID> { }

		[Account(DisplayName = "Suspense Аccount")]
		public virtual int? SuspenseAccountID { get; set; }
		#endregion

		#region SuspenseSubID
		public abstract class suspenseSubID : PX.Data.BQL.BqlInt.Field<suspenseSubID> { }

		[SubAccount(DisplayName = "Suspense Sub.")]
		public virtual int? SuspenseSubID { get; set; }
		#endregion

		#region RecognizeAdjustmentsInPreviousPeriods
		public abstract class recognizeAdjustmentsInPreviousPeriods : PX.Data.BQL.BqlBool.Field<recognizeAdjustmentsInPreviousPeriods> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Recognize Adjustments in Previous Periods", Visibility = PXUIVisibility.Visible,
			FieldClass = nameof(FeaturesSet.ASC606))]
		public virtual bool? RecognizeAdjustmentsInPreviousPeriods { get; set; }
		#endregion
	}
}

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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.EP;
using System;
using System.Linq;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information about the earnings related to a paycheck. The information will be displayed on the Paychecks and Adjustments (PR302000) form.
	/// </summary>
	[PXCacheName(Messages.PRPaymentEarning)]
	[Serializable]
	public class PRPaymentEarning : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRPaymentEarning>.By<docType, refNbr, typeCD, locationID>
		{
			public static PRPaymentEarning Find(PXGraph graph, string docType, string refNbr, string typeCD, int? locationID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, docType, refNbr, typeCD, locationID, options);
		}

		public static class FK
		{
			public class Payment : PRPayment.PK.ForeignKeyOf<PRPaymentEarning>.By<docType, refNbr> { }
			public class EarningType : EPEarningType.PK.ForeignKeyOf<PRPaymentEarning>.By<typeCD> { }
			public class Location : PRLocation.PK.ForeignKeyOf<PRPaymentEarning>.By<locationID> { }
		}
		#endregion

		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXDBString(3, IsFixed = true, IsKey = true)]
		[PXUIField(DisplayName = "Payment Doc. Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Payment Ref. Number")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<docType>>, And<PRPayment.refNbr, Equal<Current<refNbr>>>>>))]
		public String RefNbr { get; set; }
		#endregion
		#region TypeCD
		public abstract class typeCD : BqlString.Field<typeCD> { }
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, IsKey = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXUIField(DisplayName = "Code")]
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>), DescriptionField = typeof(EPEarningType.description))]
		[PXForeignReference(typeof(Field<typeCD>.IsRelatedTo<EPEarningType.typeCD>))] //ToDo: AC-142439 Ensure PXForeignReference attribute works correctly with PXCacheExtension DACs.
		public string TypeCD { get; set; }
		#endregion
		#region LocationID
		public abstract class locationID : BqlInt.Field<locationID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Location")]
		[PXSelector(typeof(PRLocation.locationID), SubstituteKey = typeof(PRLocation.locationCD))]
		//[PXFormula(typeof(Switch<Case<Where<PRPaymentEarning.jobID, IsNotNull>, Selector<PRPaymentEarning.jobID, PRJobCode.locationID>>, Selector<PRPaymentEarning.bAccountID, PREmployee.locationID>>))]
		public int? LocationID { get; set; }
		#endregion
		#region Hours
		public abstract class hours : BqlDecimal.Field<hours> { }
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? Hours { get; set; }
		#endregion
		#region RoundedHours
		public abstract class roundedHours : BqlDecimal.Field<roundedHours> { }
		/// <summary>
		/// The number of worked hours.
		/// </summary>
		[PXDecimal]
		[PXUIField(DisplayName = "Hours", Enabled = false)]
		[PXFormula(typeof(PRPaymentEarning.hours))]
		public Decimal? RoundedHours { get; set; }
		#endregion
		#region Rate
		public abstract class rate : BqlDecimal.Field<rate> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Rate")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PaymentEarningRate]
		[PayRatePrecision]
		public Decimal? Rate { get; set; }
		#endregion
		#region Amount
		public abstract class amount : BqlDecimal.Field<amount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Ext Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? Amount { get; set; }
		#endregion
		#region MTDAmount
		public abstract class mtdAmount : BqlDecimal.Field<mtdAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "MTD Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? MTDAmount { get; set; }
		#endregion
		#region QTDAmount
		public abstract class qtdAmount : BqlDecimal.Field<qtdAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "QTD Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? QTDAmount { get; set; }
		#endregion
		#region YTDAmount
		public abstract class ytdAmount : BqlDecimal.Field<ytdAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "YTD Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? YTDAmount { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public abstract class tStamp : BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}

	public class PaymentEarningRateAttribute : PXEventSubscriberAttribute
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler<PRPaymentEarning.hours>(HoursOrAmountUpdated);
			sender.Graph.FieldUpdated.AddHandler<PRPaymentEarning.amount>(HoursOrAmountUpdated);
		}

		public void HoursOrAmountUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			PRPaymentEarning row = e.Row as PRPaymentEarning;
			if (row == null)
			{
				return;
			}

			PREarningDetail[] childrenEarningDetails =
				PXParentAttribute.SelectChildren(cache.Graph.Caches[typeof(PREarningDetail)], row, typeof(PRPaymentEarning)).Cast<PREarningDetail>().ToArray();
			row.Rate = GetPaymentEarningRate(row, childrenEarningDetails);
		}

		public static decimal GetPaymentEarningRate(PRPaymentEarning paymentEarning, PREarningDetail[] childrenEarningDetails)
		{
			decimal preciseEarningAmount = 0m;
			foreach (PREarningDetail earningDetail in childrenEarningDetails)
			{
				decimal preciseEarningDetailAmount = 0m;

				if (earningDetail.IsAmountBased != true)
				{
					if (earningDetail.IsRegularRate == true)
					{
						preciseEarningDetailAmount = earningDetail.Amount.GetValueOrDefault();
					}
					else
					{
						decimal hours = earningDetail.Hours.GetValueOrDefault();
						decimal units = earningDetail.Units.GetValueOrDefault();
						decimal rate = earningDetail.Rate.GetValueOrDefault();

						preciseEarningDetailAmount = (earningDetail.UnitType == UnitType.Hour ? hours : units) * rate;
					}
				}

				preciseEarningAmount += preciseEarningDetailAmount;
			}

			return paymentEarning.Hours != 0 ? preciseEarningAmount / paymentEarning.Hours.GetValueOrDefault() : 0m;
		}
	}
}

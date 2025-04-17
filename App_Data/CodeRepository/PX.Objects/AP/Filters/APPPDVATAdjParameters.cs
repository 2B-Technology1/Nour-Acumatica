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

using PX.Objects.GL;

namespace PX.Objects.AP
{

	/// <summary>
	/// Pending Prompt Payment Discount (PPD) VAT Adjustment Parameters - Filter DAC
	/// </summary>
	[Serializable]
	[PXHidden]
	public partial class APPPDVATAdjParameters : IBqlTable
	{
		#region ApplicationDate
		public abstract class applicationDate : PX.Data.BQL.BqlDateTime.Field<applicationDate> { }
		protected DateTime? _ApplicationDate;
		/// <summary>
		/// Application date, it is a filter field for selecting documents to the grid.
		/// </summary>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual DateTime? ApplicationDate
		{
			get
			{
				return _ApplicationDate;
			}
			set
			{
				_ApplicationDate = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected int? _BranchID;
		/// <summary>
		/// Branch, it is a filter field for selecting documents to the grid.
		/// </summary>
		[Branch]
		public virtual int? BranchID
		{
			get
			{
				return _BranchID;
			}
			set
			{
				_BranchID = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		/// <summary>
		/// Vendor, it is a filter field for selecting documents to the grid.
		/// Related to the <see cref="Vendor.bAccountID"/> 
		/// </summary>
		[Vendor]
		public virtual int? VendorID
		{
			get; set;
		}
		#endregion
		#region GenerateOnePerVendor
		public abstract class generateOnePerVendor : PX.Data.BQL.BqlBool.Field<generateOnePerVendor> { }
		/// <summary>
		/// GenerateOnePerVendor, when this value is true then the processing Generate VAT Adj. will generate consolidated by Vendor adjustments.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Consolidate VAT Adjustments by Vendor", Visibility = PXUIVisibility.Visible)]
		public virtual bool? GenerateOnePerVendor
		{
			get; set;
		}
		#endregion
		#region DebitAdjDate
		public abstract class debitAdjDate : PX.Data.BQL.BqlDateTime.Field<debitAdjDate> { }
		protected DateTime? _DebitAdjDate;
		/// <summary>
		/// VAT Adjustment date for the Consolidated VAT Adjustments generation
		/// </summary>
		[PXDBDate]
		[PXFormula(typeof(Switch<Case<Where<APPPDVATAdjParameters.generateOnePerVendor, Equal<True>>, Current<AccessInfo.businessDate>>, Null>))]
		[PXUIField(DisplayName = "VAT Adjustment Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? DebitAdjDate
		{
			get
			{
				return _DebitAdjDate;
			}
			set
			{
				_DebitAdjDate = value;
			}
		}
		#endregion
		#region FinPeriod
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected string _FinPeriodID;
		/// <summary>
		/// Fin. Period for the Consolidated VAT Adjustments generation
		/// </summary>
		[APOpenPeriod(typeof(APPPDVATAdjParameters.debitAdjDate), typeof(APPPDVATAdjParameters.branchID))]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Visible)]
		public virtual string FinPeriodID
		{
			get
			{
				return _FinPeriodID;
			}
			set
			{
				_FinPeriodID = value;
			}
		}
		#endregion
	}
}

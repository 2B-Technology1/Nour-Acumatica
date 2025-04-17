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
using System.Text;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.TX;

namespace PX.Objects.PO
{
	public class LandedCostCodeMaint : PXGraph<LandedCostCodeMaint, LandedCostCode>
	{
		public PXSelect<LandedCostCode> LandedCostCode;						
		
		public LandedCostCodeMaint()
		{

		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<NotExists<Select2<TaxCategoryDet,
			InnerJoin<Tax,
			On<TaxCategoryDet.taxID, Equal<Tax.taxID>>>,
			Where<TaxCategory.taxCategoryID, Equal<TaxCategoryDet.taxCategoryID>,
				And<TaxCategory.taxCatFlag, Equal<False>,
				And<Tax.directTax, Equal<True>,
				And<LandedCostCode.allocationMethod.FromCurrent, NotEqual<LandedCostAllocationMethod.none>>>>>>>>), null)]
		protected virtual void _(Events.CacheAttached<LandedCostCode.taxCategoryID> e) { }

		protected virtual void _(Events.RowUpdated<LandedCostCode> e)
		{
			if (e.Row == null || e.Row.AllocationMethod == LandedCostAllocationMethod.None) return;

			object taxCategoryId = e.Row.TaxCategoryID;

			if (e.Row.AllocationMethod != (string)e.Cache.GetValueOriginal<LandedCostCode.allocationMethod>(e.Row))
			{
				try
				{
					e.Cache.RaiseFieldVerifying<LandedCostCode.taxCategoryID>(e.Row, ref taxCategoryId);
				}
				catch (PXSetPropertyException ex)
				{
					e.Cache.RaiseExceptionHandling<LandedCostCode.taxCategoryID>(e.Row, taxCategoryId, ex);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<LandedCostCode.taxCategoryID> e)
		{
			var row = e.Row as LandedCostCode;
			string newValue = e.NewValue as string;
			if (row == null || string.IsNullOrEmpty(newValue) || row.AllocationMethod == LandedCostAllocationMethod.None) return;

			TaxCategoryDet taxCategoryDetail = SelectFrom<TaxCategoryDet>
				.InnerJoin<TaxCategory>
				.On<TaxCategory.taxCategoryID.IsEqual<TaxCategoryDet.taxCategoryID>>
				.InnerJoin<Tax>
				.On<Tax.taxID.IsEqual<TaxCategoryDet.taxID>>
				.Where<TaxCategoryDet.taxCategoryID.IsEqual<P.AsString>
					.And<TaxCategory.taxCatFlag.IsEqual<False>>
					.And<Tax.directTax.IsEqual<True>>>.View.Select(this, newValue);

			if (taxCategoryDetail != null)
			{
				throw new PXSetPropertyException(Messages.CanApplyDirectTaxOnlyToAlocationMethodNone,
					taxCategoryDetail.TaxCategoryID, taxCategoryDetail.TaxID);
			}
		}

		protected virtual void LandedCostCode_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e) 
		{
			LandedCostCode row = (LandedCostCode)e.Row;
			sender.SetDefaultExt<LandedCostCode.vendorLocationID>(e.Row);
			sender.SetDefaultExt<LandedCostCode.termsID>(e.Row);
			doCancel = true;
		}

		protected virtual void LandedCostCode_VendorLocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if(doCancel)
			{
				e.NewValue = ((LandedCostCode)e.Row).VendorLocationID;
				e.Cancel = true;
				doCancel = false;
			}
			
		}

		protected virtual void LandedCostCode_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			LandedCostCode row = (LandedCostCode)e.Row;
			if (row != null)
			{
				bool hasVendor = row.VendorID.HasValue;
				PXDefaultAttribute.SetPersistingCheck<LandedCostCode.vendorLocationID>(sender, e.Row, hasVendor ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetRequired<LandedCostCode.vendorLocationID>(sender, hasVendor);
				PXUIFieldAttribute.SetEnabled<LandedCostCode.vendorLocationID>(sender,e.Row,hasVendor);
				sender.RaiseExceptionHandling<LandedCostCode.vendorID>(row, row.VendorID, null);
				if (hasVendor) 
				{
					Vendor vnd = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(this, row.VendorID);
					if (vnd != null && vnd.LandedCostVendor == false)
					{
						sender.RaiseExceptionHandling<LandedCostCode.vendorID>(row, row.VendorID, new PXSetPropertyException(Messages.LCCodeUsesNonLCVendor, PXErrorLevel.Warning));
					}
				}
			}
		}

		protected virtual void LandedCostCode_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			//new LC entities should be handled here
		}



		private bool doCancel = false;

	}

}

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
using System.Collections;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO.DAC.Projections;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	public class AddReturnLineToDirectInvoice : PXGraphExtension<SOInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.advancedSOInvoices>();
		}

		[PXCopyPasteHiddenView]
		public PXSelect<ARTranForDirectInvoice,
		  Where<ARTranForDirectInvoice.customerID, Equal<Current<ARInvoice.customerID>>>>
		  arTranList;

		public PXAction<ARInvoice> selectARTran;
		[PXUIField(DisplayName = "Add Return Line", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SelectARTran(PXAdapter adapter)
		{
			if (Base.Document.Cache.AllowInsert)
				arTranList.AskExt();
			return adapter.Get();
		}

		public PXAction<ARInvoice> addARTran;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddARTran(PXAdapter adapter)
		{
			foreach (ARTranForDirectInvoice origTran in arTranList.Cache.Updated)
			{
				if (origTran.Selected != true) continue;

				var tran = (ARTran)Base.Transactions.Cache.CreateInstance();
				tran.InventoryID = origTran.InventoryID;
				tran.SubItemID = origTran.SubItemID;
				tran.SiteID = origTran.SiteID;
				tran.LocationID = origTran.LocationID;
				tran.LotSerialNbr = origTran.LotSerialNbr;
				tran.ExpireDate = origTran.ExpireDate;
				tran.UOM = origTran.UOM;
				tran.Qty = INTranType.InvtMultFromInvoiceType(Base.Document.Current.DocType) * Math.Abs(origTran.Qty ?? 0m);
				tran.CuryUnitPrice = origTran.CuryUnitPrice;
				tran.OrigInvoiceType = origTran.TranType;
				tran.OrigInvoiceNbr = origTran.RefNbr;
				tran.OrigInvoiceLineNbr = origTran.LineNbr;  // not necessary

				Base.InsertInvoiceDirectLine(tran);

				origTran.Selected = false;
			}
			return adapter.Get();
		}

		protected void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = (ARInvoice)e.Row;
			selectARTran.SetEnabled(Base.Transactions.AllowInsert && row?.CustomerID != null);
		}
	}
}

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
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.SO.DAC.Projections;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	public class AddSOLineToDirectInvoice : PXGraphExtension<SOInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.advancedSOInvoices>();
		}

		[PXCopyPasteHiddenView]
		public PXSelect<SOLineForDirectInvoice,
			  Where<SOLineForDirectInvoice.customerID, Equal<Current<ARInvoice.customerID>>>>
			  soLineList;

		public PXAction<ARInvoice> selectSOLine;
		[PXUIField(DisplayName = "Add SO Line", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SelectSOLine(PXAdapter adapter)
		{
			if (Base.Document.Cache.AllowInsert)
				soLineList.AskExt();
			return adapter.Get();
		}

		public PXAction<ARInvoice> addSOLine;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddSOLine(PXAdapter adapter)
		{
			HashSet<(string orderType, string orderNbr)> ordersToProcessFilesAndNotes = new HashSet<(string orderType, string orderNbr)>();

			foreach (SOLineForDirectInvoice sol in soLineList.Cache.Updated)
			{
				if (sol.Selected != true) continue;

				var tran = (ARTran)Base.Transactions.Cache.CreateInstance();
				tran.SOOrderType = sol.OrderType;
				tran.SOOrderNbr = sol.OrderNbr;
				tran.SOOrderLineNbr = sol.LineNbr;

				Base.InsertInvoiceDirectLine(tran);

				sol.Selected = false;

				ordersToProcessFilesAndNotes.Add((tran.SOOrderType, tran.SOOrderNbr));
			}

			SOOrderType soOrderType = new SOOrderType();
			foreach (var order in ordersToProcessFilesAndNotes)
			{
				if (soOrderType.OrderType != order.orderType)
					soOrderType = SOOrderType.PK.Find(Base, order.orderType);

				if (soOrderType != null && (soOrderType.CopyHeaderNotesToInvoice == true || soOrderType.CopyHeaderFilesToInvoice == true))
				{
					SOOrder soOrder = SOOrder.PK.Find(Base, order.orderType, order.orderNbr);
					if (soOrderType != null && soOrderType.OrderType != null && soOrder != null && Base.Document.Current != null)
						Base.CopyOrderHeaderNoteAndFiles(soOrder, Base.Document.Current, soOrderType);
				}
			}

			return adapter.Get();
		}

		protected virtual void _(Events.RowSelected<ARInvoice> e)
		{
			selectSOLine.SetEnabled(Base.Transactions.AllowInsert && e.Row?.CustomerID != null);
		}

		/// Overrides <see cref="SOInvoiceEntry.Persist"/>
		[PXOverride]
		public virtual void Persist(Action baseMethod)
		{
			baseMethod();
			soLineList.Cache.Clear();
			soLineList.Cache.ClearQueryCache();
		}
	}
}

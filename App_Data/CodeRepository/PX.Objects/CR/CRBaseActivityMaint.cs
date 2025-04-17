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
using System.Linq;
using System.Web.Compilation;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	public class CRBaseActivityMaint<TGraph, TMaster> : PXGraph<TGraph>, IActivityMaint 
		where TGraph : PXGraph
		where TMaster : CRActivity, new()
	{
		#region Selects
		[PXHidden]
		public PXSelect<BAccount> BaseBAccount;

		[PXHidden]
		public PXSelect<AP.Vendor> BaseVendor;

		[PXHidden]
		public PXSelect<AR.Customer> BaseCustomer;

		[PXHidden]
		public PXSelect<EPEmployee> BaseEmployee;
		
		[PXHidden]
		public PXSelect<EPView> EPViews;

		[PXHidden]
		public PXSelect<CRActivityStatistics> Stats;
		#endregion

		#region Ctor
		public CRBaseActivityMaint()
		{
			Views.Caches.Remove(typeof(CRActivityStatistics));
			Views.Caches.Add(typeof(CRActivityStatistics));
		}
		#endregion
		
		#region Actions
		public PXSave<TMaster> Save;
		public PXSaveClose<TMaster> SaveClose;
		public PXCancel<TMaster> Cancel;
		public PXInsert<TMaster> Insert;
		#endregion

		#region IActivityMaint implementation
		public virtual void CancelRow(CRActivity row) {}

		public virtual void CompleteRow(CRActivity row) {}
		#endregion

		protected virtual void MarkAs(PXCache cache, CRActivity row, int? contactID, int status)
		{
			if (IsImport || row.NoteID == null || contactID == null) return;

			var epviewSelect = new SelectFrom<EPView>
				.Where<
					EPView.noteID.IsEqual<@P.AsGuid>
					.And<EPView.contactID.IsEqual<@P.AsInt>>>
				.View(this);

			EPView epview = epviewSelect
				.Select(row.NoteID, contactID)
				.FirstOrDefault();

			bool dirty = EPViews.Cache.IsDirty;
			if (epview == null)
			{
				var epView = EPViews.Cache.Insert(
					new EPView
					{
						NoteID = row.NoteID,
						ContactID = contactID,
						Status = status,
					}
				);

				EPViews.Cache.PersistInserted(epView);
				epviewSelect.View.Clear();
				EPViews.Cache.SetStatus(epView, PXEntryStatus.Notchanged);
			}
			else if(status != epview.Status)
			{
				epview.Status = status;
				EPViews.Cache.PersistUpdated(epview);
			}
			EPViews.Cache.IsDirty = dirty;
		}
	}
}

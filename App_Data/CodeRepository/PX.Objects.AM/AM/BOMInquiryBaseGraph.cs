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
using System;

namespace PX.Objects.AM
{
	/// <summary>
	/// Use for inquiry based BOM screens
	/// </summary>
	public abstract class BOMInquiryGraph<TGraph> : BOMInquiryBaseGraph<TGraph>
		where TGraph : BOMInquiryBaseGraph<TGraph>, new()
	{
		public new PXSaveCancel<AMBomItem> Save;
		public new PXRevisionableCancel<TGraph, AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Cancel;
		public new PXRevisionableInsert<AMBomItem> Insert;
		public new PXDelete<AMBomItem> Delete;
		public new PXRevisionableFirst<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> First;
		public new PXRevisionablePrevious<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Previous;
		public new PXRevisionableNext<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Next;
		public new PXRevisionableLast<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Last;
	}


	/// <summary>
	/// Use for inquiry based BOM screens
	/// </summary>
	public abstract class BOMInquiryBaseGraph<TGraph> : PXRevisionableGraphBase<TGraph, AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID>
		where TGraph : PXRevisionableGraphBase<TGraph, AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID>, new()
	{
		public BOMInquiryBaseGraph()
		{
			Documents.AllowDelete = false;
		}

		public override bool CanCreateNewRevision(TGraph fromGraph, TGraph toGraph, string keyValue, string revisionValue, out string error)
		{
			error = string.Empty;
			return false;
		}

		public override void CopyRevision(TGraph fromGraph, TGraph toGraph, string keyValue, string revisionValue)
		{
			throw new NotImplementedException();
		}

        protected virtual void _(Events.RowSelected<AMBomItem> e)
        {
            if (e.Row == null)
            {
                return;
            }
            
            PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<AMBomItem.bOMID>(e.Cache, e.Row, true);
            PXUIFieldAttribute.SetEnabled<AMBomItem.revisionID>(e.Cache, e.Row, true);
        }
	}
}

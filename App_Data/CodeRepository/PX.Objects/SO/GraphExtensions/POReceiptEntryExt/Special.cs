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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.GraphExtensions.POReceiptEntryExt
{
	public class Special : PXGraphExtension<POReceiptEntry>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.specialOrders>();

		/// <summary>
		/// Extends <see cref="POReceiptEntry.UpdateSOOrderLink(INTran, POLineUOpen, POReceiptLine)"/>
		/// </summary>
		[PXOverride]
		public virtual void UpdateSOOrderLink(INTran newtran, POLineUOpen poLine, POReceiptLine line)
		{
			if (poLine?.IsSpecialOrder == true)
			{
				newtran.IsSpecialOrder = true;

				// TODO: Special: Refactor, join the table in the main select, see POReceiptEntry.GetLinesToReleaseQuery method.
				SOLineSplit split = SelectFrom<SOLineSplit>
					.Where<SOLineSplit.pOType.IsEqual<POLineUOpen.orderType.FromCurrent>
						.And<SOLineSplit.pONbr.IsEqual<POLineUOpen.orderNbr.FromCurrent>>
						.And<SOLineSplit.pOLineNbr.IsEqual<POLineUOpen.lineNbr.FromCurrent>>>
					.View.ReadOnly.SelectSingleBound(Base, new object[] { poLine });

				if (split == null)
				{
					throw new Common.Exceptions.RowNotFoundException(
						Base.Caches<SOLineSplit>(), poLine.OrderType, poLine.OrderNbr, poLine.LineNbr);
				}

				newtran.SOOrderType = split.OrderType;
				newtran.SOOrderNbr = split.OrderNbr;
				newtran.SOOrderLineNbr = split.LineNbr;
			}
			else if (line.IsSpecialOrder == true && line.ReceiptType == POReceiptType.TransferReceipt)
			{
				var costCenter = INCostCenter.PK.Find(Base, line.CostCenterID);
				if (costCenter == null)
					throw new Common.Exceptions.RowNotFoundException(Base.Caches<INCostCenter>(), line.CostCenterID);

				newtran.IsSpecialOrder = true;
				newtran.SOOrderType = costCenter.SOOrderType;
				newtran.SOOrderNbr = costCenter.SOOrderNbr;
				newtran.SOOrderLineNbr = costCenter.SOOrderLineNbr;
			}
		}
	}
}

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

namespace PX.Objects.PM.ChangeRequest.GraphExtensions
{
	public class ChangeOrderClassMaintExt : PXGraphExtension<PX.Objects.PM.ChangeOrderClassMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.changeRequest>();
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderClass, PMChangeOrderClass.isAdvance> e)
		{
			if (e.Row != null && e.Row.IsAdvance == true)
			{
				e.Row.IsCostBudgetEnabled = true;
				e.Row.IsPurchaseOrderEnabled = true;
			}
		}

		protected virtual void _(Events.RowSelected<PMChangeOrderClass> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMChangeOrderClass.isCostBudgetEnabled>(e.Cache, e.Row, e.Row.IsAdvance != true);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderClass.isPurchaseOrderEnabled>(e.Cache, e.Row, e.Row.IsAdvance != true);
			}
		}
	}
}

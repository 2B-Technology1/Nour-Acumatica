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

using PX.Common;
using PX.Data;

namespace PX.Objects.IN.GraphExtensions
{
	public abstract class INUnitValidatorBase<TGraph, TUnitID, TUnitType, TParent, TParentID, TParentBaseUnit, TParentSalesUnit, TParentPurchaseUnit>
		: UnitsOfMeasureBase<TGraph, TUnitID, TUnitType, TParent, TParentID, TParentBaseUnit, TParentSalesUnit, TParentPurchaseUnit>
		where TGraph : PXGraph
		where TUnitID : class, IBqlField
		where TUnitType : class, IConstant, new()
		where TParent : class, IBqlTable, new()
		where TParentID : class, IBqlField
		where TParentBaseUnit : class, IBqlField
		where TParentSalesUnit : class, IBqlField
		where TParentPurchaseUnit : class, IBqlField
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.OnBeforeCommit += graph => graph.Apply(g => ValidateUnitConversions(ParentCurrent));
		}

		protected abstract void ValidateUnitConversions(TParent validatedItem);
	}
}

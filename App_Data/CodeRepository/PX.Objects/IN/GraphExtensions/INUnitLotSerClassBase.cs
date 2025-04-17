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
	public abstract class INUnitLotSerClassBase<TGraph, TUnitID, TUnitType, TParent, TParentID, TParentBaseUnit, TParentSalesUnit, TParentPurchaseUnit, TParentLotSerClassID>
		: INUnitValidatorBase<TGraph, TUnitID, TUnitType, TParent, TParentID, TParentBaseUnit, TParentSalesUnit, TParentPurchaseUnit>
		where TGraph : PXGraph
		where TUnitID : class, IBqlField
		where TUnitType : class, IConstant, new()
		where TParent : class, IBqlTable, new()
		where TParentID : class, IBqlField
		where TParentBaseUnit : class, IBqlField
		where TParentSalesUnit : class, IBqlField
		where TParentPurchaseUnit : class, IBqlField
		where TParentLotSerClassID : class, IBqlField
	{
		#region Event Handlers

		protected virtual void _(Events.RowPersisting<INUnit> e)
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				PXCache cache = Base.Caches<INLotSerClass>();

				if (cache.Current != null && ((INLotSerClass)cache.Current).LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional(e.Row))
				{
					e.Cache.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, e.Row.UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
				}
			}
		}

		protected virtual void _(Events.RowSelected<INUnit> e)
		{
			INLotSerClass lotSerClass = ReadLotSerClass();

			if (lotSerClass?.LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional(e.Row))
			{
				e.Cache.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, e.Row.UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
			}
			else
			{
				e.Cache.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, null, null);
			}
		}

		protected virtual void _(Events.FieldVerifying<TParent, TParentLotSerClassID> e)
		{
			INLotSerClass lotSerClass = INLotSerClass.PK.FindDirty(Base, (string)e.NewValue);
			if (lotSerClass?.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				foreach (INUnit unit in SelectOwnConversions(null))
				{
					if (INUnitAttribute.IsFractional(unit))
					{
						UnitCache.MarkUpdated(unit, assertError: true);
						UnitCache.RaiseExceptionHandling<INUnit.unitMultDiv>(unit, unit.UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
					}
				}
			}
		}
		#endregion

		protected INLotSerClass ReadLotSerClass()
		{
			PXCache cache = Base.Caches[BqlCommand.GetItemType(typeof(TParentLotSerClassID))];
			return INLotSerClass.PK.FindDirty(Base, (string)cache.GetValue(cache.Current, typeof(TParentLotSerClassID).Name));
		}
	}
}

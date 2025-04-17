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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AM.Attributes;
using PX.Objects.CM;
using PX.Objects.Common.GraphExtensions;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Linq;

namespace PX.Objects.AM
{
    public class OvhdMaint : PXGraph<OvhdMaint>
    {
		public SelectFrom<AMOverhead>.View OvhdRecords;
		public PXSetup<AMBSetup> ambsetup;
        public PXSavePerRow<AMOverhead> Save;

		public class CurySettings : CurySettingsExtension<OvhdMaint, AMOverhead, AMOverheadCurySettings>
		{
			public static bool IsActive() => true;
			private AMOverhead updatedRecord;

			protected override void _(Events.RowUpdated<AMOverheadCurySettings> e)
			{
				PXCache cache = Base.Caches<AMOverhead>();
				var record = PXParentAttribute.SelectParent(e.Cache, e.Row);
				if (updatedRecord != record)
				{
					if (IsTenantBaseCurrency(e.Row) &&
						GetCurySettingsFields().Any(field =>
							!Equals(e.Cache.GetValue(e.Row, field), e.Cache.GetValue(e.OldRow, field))))
					{
						record = cache.CreateCopy(record);
						foreach (string field in GetCurySettingsFields())
						{
							cache.SetValue(record, field, e.Cache.GetValue(e.Row, field));
						}

						cache.Update(record);
					}
				}
			}
			protected override void _(Events.RowInserted<AMOverheadCurySettings> e)
			{
				PXCache cache = Base.Caches<AMOverhead>();
				var record = PXParentAttribute.SelectParent(e.Cache, e.Row);
				if (updatedRecord != record)
				{
					if (IsTenantBaseCurrency(e.Row))
					{
						record = cache.CreateCopy(record);
						foreach (string field in GetCurySettingsFields())
						{
							cache.SetValue(record, field, e.Cache.GetValue(e.Row, field));
						}

						cache.Update(record);
					}
				}
			}

			protected override void _(Events.RowUpdated<AMOverhead> e)
			{
				PXCache cache = Base.Caches<AMOverheadCurySettings>();
				if (GetCurySettingsFields().Any(field =>
					!Equals(e.Cache.GetValue(e.Row, field),
						e.Cache.GetValue(e.OldRow, field))))
				{
					updatedRecord = e.Row;
					try
					{
						object record = curySettings.SelectSingleBound(new object[] { updatedRecord },
							Base.Accessinfo.BaseCuryID ?? CurrencyCollection.GetBaseCurrency()?.CuryID);
						if (record == null)
						{
							record = cache.Insert();
						}

						record = cache.CreateCopy(record);
						foreach (string str in GetCurySettingsFields())
						{
							cache.SetValue(record, str, e.Cache.GetValue(e.Row, str));
						}

						cache.Update(record);
					}
					finally
					{
						updatedRecord = null;
					}
				}
			}
		}

		protected virtual IEnumerable ovhdRecords()
		{
			this.Caches[typeof(AMOverhead)].ClearQueryCache();
			this.Caches[typeof(AMOverheadCurySettings)].ClearQueryCache();

			bool itVar1 = false;
			IEnumerator enumerator = this.OvhdRecords.Cache.Inserted.GetEnumerator();
			while (enumerator.MoveNext())
			{
				AMOverhead ovhd = (AMOverhead)enumerator.Current;
				itVar1 = true;
				AMOverheadCurySettings ovhdCury = AMOverheadCurySettings.PK.Find(this, ovhd.OvhdID, Accessinfo.BaseCuryID);
				ovhd.CostRate = ovhdCury?.CostRate ?? 0;
				yield return ovhd;
			}
			if(!itVar1)
			{
				foreach (PXResult<AMOverhead> result in SelectFrom<AMOverhead>
					.View.Select(this))
				{
					var ovhd = (AMOverhead)result;
					if (ovhd == null)
						yield break;

					AMOverheadCurySettings ovhdCury = AMOverheadCurySettings.PK.Find(this, ovhd.OvhdID, Accessinfo.BaseCuryID);

					ovhd.CostRate = ovhdCury?.CostRate ?? 0;
					yield return ovhd;
				}
			}
		}

		protected virtual void AMOverhead_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
			AMOverhead ovhdRec = (AMOverhead)e.Row;
            AMBomOvhd ambomoper = PXSelect<AMBomOvhd, Where<AMBomOvhd.ovhdID, Equal<Required<AMBomOvhd.ovhdID>>>>.Select(this, ovhdRec.OvhdID);
            if (ambomoper != null)
            {
                throw new PXException(Messages.GetLocal(Messages.Overhead_NotDeleted_OnBOM), ambomoper.BOMID);
            }

            AMProdOvhd amprodovhd = PXSelect<AMProdOvhd, Where<AMProdOvhd.ovhdID, Equal<Required<AMProdOvhd.ovhdID>>>>.Select(this, ovhdRec.OvhdID);
            if (amprodovhd != null)
            {
                throw new PXException(Messages.GetLocal(Messages.Overhead_NotDeleted_OnProd), 
                    amprodovhd.OrderType.TrimIfNotNullEmpty(), amprodovhd.ProdOrdID.TrimIfNotNullEmpty());
            }

            AMEstimateOvhd amestimateovhd = PXSelect<AMEstimateOvhd, Where<AMEstimateOvhd.ovhdID, Equal<Required<AMEstimateOvhd.ovhdID>>
                    >>.Select(this, ovhdRec.OvhdID);
            if (amestimateovhd != null)
            {
                throw new PXException(Messages.GetLocal(Messages.Overhead_NotDeleted_OnEstimate),
                    amestimateovhd.EstimateID, amestimateovhd.RevisionID);
            }
        }

        protected virtual void AMOverhead_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            if (!sender.ObjectsEqual<AMOverhead.ovhdID>(e.Row, e.NewRow))
            {
                throw new PXException(Messages.GetLocal(Messages.OverheadIDCannotBeUpdated));
            }
        }
	}
}

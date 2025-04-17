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

using PX.Data;

namespace PX.Objects.FA
{
	public class BonusMaint : PXGraph<BonusMaint, FABonus>
	{
		#region Selects
		public PXSelect<FABonus> Bonuses;
		public PXSelect<FABonusDetails, Where<FABonusDetails.bonusID, Equal<Current<FABonus.bonusID>>>> Details;
		#endregion

		#region Ctor
		#endregion

		#region Events

		protected virtual void FABonusDetails_StartDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FABonusDetails det = (FABonusDetails) e.Row;
			if (det == null) return;

			DateTime? end = det.EndDate;
			if (end != null && (DateTime?)e.NewValue > end)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, end);
			}

			FABonusDetails prev = PXSelect<FABonusDetails, Where<FABonusDetails.lineNbr, Less<Current<FABonusDetails.lineNbr>>, And<FABonusDetails.bonusID, Equal<Current<FABonusDetails.bonusID>>>>, OrderBy<Desc<FABonusDetails.endDate>>>.SelectSingleBound(this, new object[] { det });
			DateTime? prevEnd = prev == null ? null : prev.EndDate;
			if (prevEnd != null && (DateTime?)e.NewValue <= prevEnd)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, prevEnd);
			}

			FABonusDetails next = PXSelect<FABonusDetails, Where<FABonusDetails.lineNbr, Greater<Current<FABonusDetails.lineNbr>>, And<FABonusDetails.bonusID, Equal<Current<FABonusDetails.bonusID>>>>, OrderBy<Desc<FABonusDetails.endDate>>>.SelectSingleBound(this, new object[] { det });
			DateTime? nextStart = next == null ? null : next.StartDate;
			if (nextStart != null && (DateTime?)e.NewValue >= nextStart)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LT, nextStart);
			}
		}

		protected virtual void FABonusDetails_EndDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FABonusDetails det = (FABonusDetails)e.Row;
			if (det == null) return;

			DateTime? start = det.StartDate;
			if (start != null && (DateTime?)e.NewValue < start)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, start);
			}

			FABonusDetails next = PXSelect<FABonusDetails, Where<FABonusDetails.lineNbr, Greater<Current<FABonusDetails.lineNbr>>, And<FABonusDetails.bonusID, Equal<Current<FABonusDetails.bonusID>>>>, OrderBy<Desc<FABonusDetails.endDate>>>.SelectSingleBound(this, new object[] { det });
			DateTime? nextStart = next == null ? null : next.StartDate;
			if (nextStart != null && (DateTime?)e.NewValue >= nextStart)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LT, nextStart);
			}

			FABonusDetails prev = PXSelect<FABonusDetails, Where<FABonusDetails.lineNbr, Less<Current<FABonusDetails.lineNbr>>, And<FABonusDetails.bonusID, Equal<Current<FABonusDetails.bonusID>>>>, OrderBy<Desc<FABonusDetails.endDate>>>.SelectSingleBound(this, new object[] { det });
			DateTime? prevEnd = prev == null ? null : prev.EndDate;
			if (prevEnd != null && (DateTime?)e.NewValue <= prevEnd)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, prevEnd);
			}
		}

		#endregion
	}
}
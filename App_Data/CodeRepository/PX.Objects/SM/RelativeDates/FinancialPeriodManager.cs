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
using System.Collections.Generic;
using PX.Data;
using PX.Data.RelativeDates;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.SM;

namespace PX.Objects.SM
{
	public class FinancialPeriodManager : IFinancialPeriodManager
	{
		private class FinancialPeriodDefinition : IPrefetchable
		{
			public const string SLOT_KEY = "FinancialPeriodDefinition";

			public static Type[] DependentTables
			{
				get
				{
					return new[] { typeof(FinPeriod) };
				}
			}

			public List<FinPeriod> Periods { get; private set; }

			public FinancialPeriodDefinition()
			{
				Periods = new List<FinPeriod>();
				_periodsIndexes = new Dictionary<DateTime, int>();
			}

			public void Prefetch()
			{
				Periods.Clear();
				_periodsIndexes.Clear();

				foreach (PXDataRecord record in PXDatabase.SelectMulti<FinPeriod>(
					new PXDataFieldOrder<FinPeriod.finPeriodID>(),
					new PXDataField<FinPeriod.startDate>(),
					new PXDataField<FinPeriod.endDate>(),
					new PXDataFieldValue<FinPeriod.organizationID>(PXDbType.Int, 4, FinPeriod.organizationID.MasterValue)))
				{
					FinPeriod period = new FinPeriod
					{
						StartDate = record.GetDateTime(0),
						EndDate = record.GetDateTime(1)
					};

					Periods.Add(period);
				}
			}

			private readonly Dictionary<DateTime, int> _periodsIndexes;
			public int FindPeriodIndex(DateTime dateTime)
			{
				var date = dateTime.Date;
				if (_periodsIndexes.TryGetValue(date, out var ind))
					return ind;

				var index = Periods
					.FindIndex(p => p.StartDate <= date && p.EndDate > date);
				_periodsIndexes[date] = index;
				return index;
			}
		}

		private FinancialPeriodDefinition Definition => 
			PXDatabase.GetSlot<FinancialPeriodDefinition>(FinancialPeriodDefinition.SLOT_KEY, FinancialPeriodDefinition.DependentTables);

		private readonly ITodayUtc todayer;

		private int CurrentIndex
		{
			get
			{
				var index = Definition.FindPeriodIndex(todayer.TodayUtc);
				if (index == -1)
					throw new PXException(MyMessages.CannotFindFinancialPeriod);
				return index;
			}
		}

		public FinancialPeriodManager(ITodayUtc today)
		{
			todayer = today;
		}

		public DateTime GetCurrentFinancialPeriodStart(int shift)
		{
			FinPeriod finPeriod = GetFinancialPeriod(shift);

			return finPeriod.StartDate.Value;
		}

		public DateTime GetCurrentFinancialPeriodEnd(int shift)
		{
			FinPeriod finPeriod = GetFinancialPeriod(shift);

			return finPeriod.EndDate.Value.AddTicks(-1);
		}

		private FinPeriod GetFinancialPeriod(int shift)
		{
			int index = CurrentIndex + shift;

			if (index < 0 || index > Definition.Periods.Count - 1)
			{
				throw new PXException(MyMessages.CannotFindNextFinancialPeriod);
			}

			return Definition.Periods[index];
		}
	}
}

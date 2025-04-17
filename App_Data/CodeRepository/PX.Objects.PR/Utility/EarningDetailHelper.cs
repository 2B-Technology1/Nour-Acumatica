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

namespace PX.Objects.PR
{
	public static class EarningDetailHelper
	{
		public static void CopySelectedEarningDetailRecord(PXCache cache)
		{
			PREarningDetail currentRecord = cache.Current as PREarningDetail;
			if (currentRecord?.AllowCopy != true)
				return;

			PREarningDetail newRecord = CreateEarningDetailCopy(cache, currentRecord);
			cache.Update(newRecord);
		}

		public static PREarningDetail CreateEarningDetailCopy(PXCache cache, PREarningDetail originalEarning, bool isFringeEarning)
		{
			// To ensure a true copy of all fields and avoid having fields defaulted in the Insert
			// or Update:
			// 1. Restore copy of original earning to new record
			// 2. Insert new record
			// 3. Restore copy of original record to the inserted new record again
			PREarningDetail copy = cache.CreateInstance() as PREarningDetail;
			cache.RestoreCopy(copy, originalEarning);
			copy.RecordID = null;

			if (isFringeEarning)
			{
				copy.IsRegularRate = false;
				copy.IsFringeRateEarning = true;
			}

			copy = (PREarningDetail)cache.Insert(copy);

			int? copyRecordID = copy.RecordID;
			cache.RestoreCopy(copy, originalEarning);

			copy.RecordID = copyRecordID;

			if (isFringeEarning)
			{
				copy.IsRegularRate = false;
				copy.IsFringeRateEarning = true;
			}

			copy.SourceNoteID = null;
			return copy;
		}

		public static PREarningDetail CreateEarningDetailCopy(PXCache cache, PREarningDetail originalEarning)
		{
			return CreateEarningDetailCopy(cache, originalEarning, false);
		}
	}
}

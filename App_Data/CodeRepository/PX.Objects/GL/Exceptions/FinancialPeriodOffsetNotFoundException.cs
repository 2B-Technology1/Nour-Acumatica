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
using System.Runtime.Serialization;

using PX.Data;

namespace PX.Objects.GL.Exceptions
{
	public class FinancialPeriodOffsetNotFoundException : PXFinPeriodException
	{
		/// <summary>
		/// Gets the financial period ID for which the offset period was not found.
		/// </summary>
		public string FinancialPeriodId
		{
			get;
			private set;
		}

		/// <summary>
		/// The positive or negative number of periods offset from the <see cref="FinancialPeriodId"/>,
		/// for which the financial period was not found.
		/// </summary>
		public int Offset
		{
			get;
			private set;
		}

		public FinancialPeriodOffsetNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }

		/// <param name="financialPeriodId">
		/// The financial period ID in the internal representation. 
		/// It will automatically be formatted for display in the error message.
		/// </param>
		public FinancialPeriodOffsetNotFoundException(string financialPeriodId, int offset)
			: base(GetMessage(financialPeriodId, offset))
		{
			FinancialPeriodId = financialPeriodId;
			Offset = offset;
		}

		private static string GetMessage(string financialPeriodId, int offset)
		{
			if (string.IsNullOrEmpty(financialPeriodId)) return Messages.NoPeriodsDefined;
			else switch (offset)
				{
					case 0: return PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodWithId, FinPeriodIDAttribute.FormatForError(financialPeriodId));
					case -1: return PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodBefore, FinPeriodIDAttribute.FormatForError(financialPeriodId));
					case 1: return PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodAfter, FinPeriodIDAttribute.FormatForError(financialPeriodId));
					default: return PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodForOffset,
						Math.Abs(offset),
						offset > 0 ? Messages.AfterLowercase : Messages.BeforeLowercase,
						FinPeriodIDAttribute.FormatForError(financialPeriodId));
				}
		}
	}
}

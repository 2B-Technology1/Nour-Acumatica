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

namespace PX.Objects.PR.AUF
{
	public class DatRecord : AufRecord
	{
		public DatRecord(DateTime start, DateTime end) : base(AufRecordType.Dat)
		{
			FirstDate = start;
			LastDate = end;
			Year = end.Year;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				Year,
				Quarter,
				Month,
				_FirstDate,
				_LastDate
			};

			return FormatLine(lineData);
		}

		public virtual int Year { get; set; }
		public virtual int? Quarter { get; set; }
		public virtual int? Month { get; set; }

		private DateTime _FirstDate;
		public virtual DateTime FirstDate
		{
			get
			{
				return _FirstDate.Date;
			}
			set
			{
				_FirstDate = value;
			}
		}
		private DateTime _LastDate;
		public virtual DateTime LastDate
		{
			get
			{
				return _LastDate.Date.AddDays(1).AddTicks(-1);
			}
			set
			{
				_LastDate = value;
			}
		}
	}
}

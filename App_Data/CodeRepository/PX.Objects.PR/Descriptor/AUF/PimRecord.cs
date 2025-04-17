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

namespace PX.Objects.PR.AUF
{
	public class PimRecord : AufRecord
	{
		public PimRecord(string itemID) : base(AufRecordType.Pim)
		{
			Title = itemID.TrimEnd();
			PimID = GetPimIDFromName(itemID);
		}

		public PimRecord(string itemID, int pimID) : base(AufRecordType.Pim)
		{
			Title = itemID.TrimEnd();
			PimID = pimID;
		}

		public static int GetPimIDFromName(string name)
		{
			return Math.Abs(name.TrimEnd().GetHashCode());
		}

		public override string ToString()
		{
			object[] lineData =
			{
				Title,
				PimID,
				Description,
				AatrixTaxType,
				State,
				AufConstants.UnusedField,
				AccountNumber,
				AufConstants.UnusedField
			};

			return FormatLine(lineData);
		}

		public virtual string Title { get; set; }
		public virtual int PimID { get; set; }
		public virtual string Description { get; set; }
		public virtual int? AatrixTaxType { get; set; }
		public virtual string State { get; set; }
		public virtual string AccountNumber { get; set; }
	}

	public class PimRecordComparer : IEqualityComparer<PimRecord>
	{
		public bool Equals(PimRecord x, PimRecord y)
		{
			return x.PimID == y.PimID;
		}

		public int GetHashCode(PimRecord obj)
		{
			return obj.PimID;
		}
	}
}

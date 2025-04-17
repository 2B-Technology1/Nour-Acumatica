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

using PX.CloudServices.DocumentRecognition;
using PX.Common;
using PX.Objects.AP.InvoiceRecognition.Feedback.VendorSearch;
using System;

namespace PX.Objects.AP.InvoiceRecognition.VendorSearch
{
	internal class RankedSearchResult : IComparable<RankedSearchResult>
	{
		public Candidate Candidate { get; }
		public float Rank => Candidate.Score.Value;
		public int? TermIndex { get; }
		public Vendor Vendor { get; }

		public RankedSearchResult(Vendor vendor, int? termIndex)
		{
			vendor.ThrowOnNull(nameof(vendor));

			Vendor = vendor;
			TermIndex = termIndex;

			Candidate = new Candidate { Score = 0 };
		}

		public void IncreaseRank()
		{
			Candidate.Score++;
		}

		public int CompareTo(RankedSearchResult other)
		{
			if (other == null)
			{
				return 1;
			}

			if (Rank > other.Rank)
			{
				return 1;
			}

			if (Rank < other.Rank)
			{
				return -1;
			}

			return 0;
		}
	}
}

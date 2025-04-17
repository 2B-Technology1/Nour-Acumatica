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
using PX.Objects.AP.InvoiceRecognition.Feedback;
using System;
using System.Linq;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.AP.InvoiceRecognition.VendorSearch
{
	internal class RankedSearchResultCollection
	{
		private readonly Dictionary<Guid?, RankedSearchResult> _resultsByNoteId = new Dictionary<Guid?, RankedSearchResult>();
		private readonly VendorSearchFeedbackBuilder _feedbackBuilder;

		public int Count => _resultsByNoteId.Count;

		public RankedSearchResultCollection(VendorSearchFeedbackBuilder feedbackBuilder)
		{
			feedbackBuilder.ThrowOnNull(nameof(feedbackBuilder));

			_feedbackBuilder = feedbackBuilder;
		}

		public RankedSearchResult Add(PXGraph graph, Vendor vendor, int? termIndex = null)
		{
			vendor.ThrowOnNull(nameof(vendor));

			if (_resultsByNoteId.TryGetValue(vendor.NoteID, out var existingResult))
			{
				existingResult.IncreaseRank();

				return existingResult;
			}
			else
			{
				var newResult = new RankedSearchResult(vendor, termIndex);
				_resultsByNoteId.Add(vendor.NoteID, newResult);

				_feedbackBuilder.AddCandidate(graph, newResult.Vendor.BAccountID.Value, newResult.Vendor.DefContactID, newResult.Vendor.PrimaryContactID,
					newResult.Vendor.NoteID, newResult.Candidate);

				return newResult;
			}
		}

		public RankedSearchResult GetMaxRankResult()
		{
			if (_resultsByNoteId.Count == 0)
			{
				return null;
			}

			return _resultsByNoteId.Values.Max();
		}
	}
}

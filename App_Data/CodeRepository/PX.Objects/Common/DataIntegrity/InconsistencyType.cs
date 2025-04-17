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

using System.Collections.Generic;

namespace PX.Objects.Common.DataIntegrity
{
	public class InconsistencyCode : ILabelProvider
	{
		public const string UnreleasedDocumentHasGlTransactions = "UNRELDOCHASGL";
		public const string ReleasedDocumentHasNoGlTransactions = "RELDOCHASNOGL";
		public const string BatchTotalNotEqualToTransactionTotal = "BATCHTRANTOTALMISMATCH";
		public const string ReleasedDocumentHasUnreleasedApplications = "RELDOCUNRELADJUST";
		public const string UnreleasedDocumentHasReleasedApplications = "UNRELDOCRELADJUST";
		public const string DocumentTotalsWrongPrecision = "DOCTOTALSPRECISION";
		public const string DocumentNegativeBalance = "DOCNEGATIVEBALANCE";

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			{ UnreleasedDocumentHasGlTransactions, Messages.DataIntegrityGLBatchExistsForUnreleasedDocument },
			{ ReleasedDocumentHasNoGlTransactions, Messages.DataIntegrityGLBatchNotExistsForReleasedDocument },
			{ BatchTotalNotEqualToTransactionTotal, Messages.DataIntegrityGLBatchSumsNotEqualGLTransSums },
			{ ReleasedDocumentHasUnreleasedApplications, Messages.DataIntegrityReleasedDocumentWithUnreleasedApplications },
			{ UnreleasedDocumentHasReleasedApplications, Messages.DataIntegrityUnreleasedDocumentWithReleasedApplications },
			{ DocumentTotalsWrongPrecision, Messages.DataIntegrityDocumentTotalsHaveLargerPrecisionThanCurrency },
			{ DocumentNegativeBalance, Messages.DataIntegrityDocumentHasNegativeBalance },
		};

		public class unreleasedDocumentHasGlTransactions : PX.Data.BQL.BqlString.Constant<unreleasedDocumentHasGlTransactions>
		{
			public unreleasedDocumentHasGlTransactions() 
				: base(UnreleasedDocumentHasGlTransactions)
			{ }
		}

		public class releasedDocumentHasNoGlTransactions : PX.Data.BQL.BqlString.Constant<releasedDocumentHasNoGlTransactions>
		{
			public releasedDocumentHasNoGlTransactions()
				: base(ReleasedDocumentHasNoGlTransactions)
			{ }
		}

		public class batchTotalNotEqualToTransactionTotal : PX.Data.BQL.BqlString.Constant<batchTotalNotEqualToTransactionTotal>
		{
			public batchTotalNotEqualToTransactionTotal()
				: base(BatchTotalNotEqualToTransactionTotal)
			{ }
		}

		public class releasedDocumentHasUnreleasedApplications : PX.Data.BQL.BqlString.Constant<releasedDocumentHasUnreleasedApplications>
		{
			public releasedDocumentHasUnreleasedApplications()
				: base(ReleasedDocumentHasUnreleasedApplications)
			{ }
		}

		public class unreleasedDocumentHasReleasedApplications : PX.Data.BQL.BqlString.Constant<unreleasedDocumentHasReleasedApplications>
		{
			public unreleasedDocumentHasReleasedApplications()
				: base(UnreleasedDocumentHasReleasedApplications)
			{ }
		}

		public class documentTotalsWrongPrecision : PX.Data.BQL.BqlString.Constant<documentTotalsWrongPrecision>
		{
			public documentTotalsWrongPrecision()
				: base(DocumentTotalsWrongPrecision)
			{ }
		}

		public class documentNegativeBalance : PX.Data.BQL.BqlString.Constant<documentNegativeBalance>
		{
			public documentNegativeBalance()
				: base(DocumentNegativeBalance)
			{ }
		}
	}
}

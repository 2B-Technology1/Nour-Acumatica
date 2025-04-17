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
using PX.Objects.AR.CustomerStatements;
using PX.Objects.Common;
using PX.Objects.Common.Abstractions;
using System;
using ARTranPostType = PX.Objects.AR.ARTranPost.type;

namespace PX.Objects.AR
{
	public partial class StatementCycleProcessBO : PXGraph<StatementCycleProcessBO>
	{
		protected abstract class ARTranPostStatement : IDocumentKey
		{
			public ARTranPostGL ARTranPost { get; }
			public ARRegister ARRegister { get; }
			public ARRegister2 SourceARRegister { get; }

			public string DocType
			{
				get => ARTranPost.DocType;
				set => ARTranPost.DocType = value;
			}
			public string RefNbr
			{
				get => ARTranPost.RefNbr;
				set => ARTranPost.RefNbr = value;
			}

			public ARTranPostStatement(PXResult<ARTranPostGL> pXResult)
			{
				ARTranPost = pXResult;
				ARRegister = pXResult.GetItem<ARRegister>();
				SourceARRegister = pXResult.GetItem<ARRegister2>();
			}

			public ARStatementKey GetARStatementKey(Customer customer, DateTime statementDate)
			{
				return new ARStatementKey(ARTranPost.BranchID.Value, ARTranPost.CuryID, customer.BAccountID.Value, statementDate);
			}

			public bool ARRegisterHasBalance()
			{
				return ARTranPostType.Origin == ARTranPost.Type && ARRegister.HasBalance();
			}

			public virtual void AdjustStatementEndBalance(ARStatement statement)
			{
				if (ARTranPost.Type != ARTranPostType.Origin) return;
				statement.EndBalance += ARRegister.SignBalance * ARRegister.DocBal;
				statement.CuryEndBalance += ARRegister.SignBalance * ARRegister.CuryDocBal;
			}

			public abstract bool ShouldBeConvertedToStatementDetail();
		}

		protected class ARTranPostOpenItem : ARTranPostStatement
		{
			public ARTranPostOpenItem(PXResult<ARTranPostGL> pXResult) : base(pXResult) { }

			public override bool ShouldBeConvertedToStatementDetail() => ARRegisterHasBalance();
		}

		protected class ARTranPostBBF : ARTranPostStatement
		{
			public ARTranPostBBF(PXResult<ARTranPostGL> pXResult) : base(pXResult) { }

			private bool WithinSameCuryBranchCustomer() =>
				ARTranPost.CuryID == SourceARRegister.CuryID
					&& ARTranPost.BranchID == SourceARRegister.BranchID
					&& ARTranPost.CustomerID == SourceARRegister.CustomerID;

			private bool ShouldDocumentBePresent() => ARRegister.StatementDate == null;
			private bool ShouldSourceDocumentBePresent() => SourceARRegister.StatementDate == null;
			private bool ShouldApplicationBePresent() => ARTranPost.StatementDate == null;

			private bool IsSelfVoidingApplication => SourceARRegister.Voided == true && ARDocType.IsSelfVoiding(ARTranPost.SourceDocType);

			public override bool ShouldBeConvertedToStatementDetail()
			{
				switch (ARTranPost.Type)
				{
					case ARTranPostType.Origin:
						return ShouldDocumentBePresent();
					case ARTranPostType.Application:
						{
							if (!ShouldApplicationBePresent()) return false;
							if (!WithinSameCuryBranchCustomer()) return true;

							if (IsSelfVoidingApplication && ARTranPost.VoidAdjNbr == null) return false;

							return !ShouldDocumentBePresent() && (ShouldSourceDocumentBePresent() || IsSelfVoidingApplication);
						}
					case ARTranPostType.Adjustment:
						{
							if (IsSelfVoidingApplication && ARTranPost.VoidAdjNbr == null) return false;
							return ShouldApplicationBePresent();
						}
					default: return false;
				}
			}
		}
	}
}

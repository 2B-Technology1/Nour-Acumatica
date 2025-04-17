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
using System.Collections.Generic;
using System.Collections;
using System.Text;
using PX.Objects.AP;
using PX.Data.Api.Export;
using System.Linq;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	[NonOptimizable(new Type[] { typeof(Tax.taxType), typeof(Tax.taxApplyTermsDisc) }, IgnoreOptimizationBehavior = true)]
	public class TaxCategoryMaint : PXGraph<TaxCategoryMaint, TaxCategory>
	{
		public PXSelect<TaxCategory> TxCategory;
		public PXSelectJoin<TaxCategoryDet,InnerJoin<Tax, On<TaxCategoryDet.taxID,Equal<Tax.taxID>>>, Where<TaxCategoryDet.taxCategoryID, Equal<Current<TaxCategory.taxCategoryID>>>> Details;
        public PXSelect<TaxCategoryDet> TxCategoryDet;

		public TaxCategoryMaint()
		{
			if (Company.Current.BAccountID.HasValue == false)
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(GL.Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
		}
		public PXSetup<GL.Branch> Company;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search<Tax.taxID, Where<Tax.isExternal, Equal<False>>>), new Type[] { typeof(Tax.taxID), typeof(Tax.descr), typeof(Tax.directTax) })]
		public virtual void _(Events.CacheAttached<TaxCategoryDet.taxID> e) { }

		protected virtual void TaxCategory_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			TaxCategory row = e.Row as TaxCategory;
			if (row != null)
			{
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(AP.APTran.taxCategoryID), typeof(Search<AP.APTran.taxCategoryID, Where<AP.APTran.released, Equal<False>>>));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(AR.ARTran.taxCategoryID), typeof(Search<AR.ARTran.taxCategoryID, Where<AR.ARTran.released, Equal<False>>>));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(AR.ARFinCharge.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(CA.CASplit.taxCategoryID), typeof(Search2<CA.CASplit.taxCategoryID, InnerJoin<CA.CAAdj, 
					On<CA.CASplit.adjRefNbr, Equal<CA.CAAdj.adjRefNbr>>>, Where<CA.CAAdj.released, Equal<False>>>));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(EP.EPExpenseClaimDetails.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(PO.POLine.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(SO.SOLine.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(SO.SOOrder.freightTaxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(CR.CROpportunityProducts.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(IN.INItemClass.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(IN.InventoryItem.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(CS.Carrier.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(TX.TaxZone.dfltTaxCategoryID));
			}
		}

		protected virtual void TaxCategoryDet_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			TaxCategoryDet row = e.Row as TaxCategoryDet;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<TaxCategoryDet.taxID>(sender, row, string.IsNullOrEmpty(row.TaxID));
			}
		}

		protected virtual void TaxCategoryDet_TaxID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			TaxCategoryDet tax = (TaxCategoryDet)e.Row;
			string taxId = (string)e.NewValue;
			if (tax.TaxID != taxId)
			{
				List<string> allTaxxes = new List<string>() { taxId };

				foreach (TaxCategoryDet iTax in this.Details.Select())
				{
					if (iTax.TaxID == taxId)
					{
						e.Cancel = true;
						throw new PXSetPropertyException(Messages.TaxAlreadyInList);
					}

					if (!IsValidTaxCombination(taxId, iTax.TaxID, TxCategory.Current?.TaxCatFlag))
					{
						e.Cancel = true;
						throw new PXSetPropertyException(Messages.NonImportAndImportCanNotExistTogether, TxCategory.Current?.TaxCategoryID);
					}
						
					allTaxxes.Add(iTax.TaxID);
				}

				if (Tax.PK.Find(this, taxId).DirectTax == true
					&& !TryValidateTaxZoneCombinationWithDirectTax(allTaxxes.ToArray(), TxCategory.Current?.TaxCatFlag,
					out PXResultset<TaxZoneDet> invalidZoneCombinations))
				{
					e.Cancel = true;
					throw new PXSetPropertyException(Messages.SeveralImportTaxesCanNotBeInSameZoneAndCategory,
						TxCategory.Current?.TaxCategoryID, ((TaxZoneDet)invalidZoneCombinations)?.TaxZoneID);
				}
			}
		}


		protected virtual bool IsValidTaxCombination(string firstTaxId, string secondTaxId, bool? taxCatFlag)
		{
			if (taxCatFlag == true) return true;

			bool isDirectTax = Tax.PK.Find(this, firstTaxId).DirectTax == true;
			return Tax.PK.Find(this, secondTaxId).DirectTax == isDirectTax;
		}

		protected virtual bool TryValidateTaxZoneCombinationWithDirectTax(string[] taxIds, bool? taxCatFlag, out PXResultset<TaxZoneDet> invalidZoneCombinations)
		{
			if (taxCatFlag == true || taxIds.Length < 1)
			{
				invalidZoneCombinations = new PXResultset<TaxZoneDet>();
				return true;
			}

			invalidZoneCombinations = SelectFrom<TaxZoneDet>
			.Where<TaxZoneDet.taxID.IsIn<P.AsString>>
			.AggregateTo<GroupBy<TaxZoneDet.taxZoneID>, Count<TaxZoneDet.taxID>>
			.Having<TaxZoneDet.taxID.Counted.IsGreater<decimal1>>
			.View.Select(this, new object[] { taxIds });

			return invalidZoneCombinations.Count == 0;
		}

		protected virtual void _(Events.FieldVerifying<TaxCategory, TaxCategory.taxCatFlag> e)
		{
			if (e.Row == null || e.NewValue == null || (bool)e.NewValue == true) return;

			if (!IsValidConfiguration((bool?)e.NewValue))
			{
				e.Cancel = true;
				e.NewValue = e.OldValue;
			}
		}

		protected virtual void _(Events.FieldVerifying<TaxCategory, TaxCategory.active> e)
		{
			if (e.Row == null || e.NewValue == null || (bool)e.NewValue != true) return;

			if (!IsValidConfiguration(TxCategory.Current?.TaxCatFlag))
			{
				e.Cancel = true;
				e.NewValue = e.OldValue;
			}
		}

		protected virtual void _(Events.RowPersisting<TaxCategory> e)
		{
			if (e.Row == null) return;

			IsValidConfiguration(TxCategory.Current?.TaxCatFlag);
		}

		protected virtual bool IsValidConfiguration(bool? taxCatFlag)
		{
			List<TaxCategoryDet> allTaxes = this.Details.Select().RowCast<TaxCategoryDet>().ToList();

			if (taxCatFlag == true || allTaxes.Count < 1) return true;

			bool isDirectTax = Tax.PK.Find(this, allTaxes.First().TaxID).DirectTax == true;
			foreach (TaxCategoryDet tax in allTaxes)
			{
				if (Tax.PK.Find(this, tax.TaxID).DirectTax != isDirectTax)
				{
					Details.Cache.RaiseExceptionHandling<TaxCategoryDet.taxID>(tax, tax.TaxID,
						new PXSetPropertyException(Messages.NonImportAndImportCanNotExistTogether, TxCategory.Current?.TaxCategoryID));

					return false;
				}
			}

			if (isDirectTax && !TryValidateTaxZoneCombinationWithDirectTax(allTaxes.Select(a => a.TaxID).ToArray(), taxCatFlag,
				out PXResultset<TaxZoneDet> invalidZoneCombinations))
			{
				TaxCategoryDet invalidRow = Details.Cache
					.Locate(new TaxCategoryDet()
					{
						TaxID = ((TaxZoneDet)invalidZoneCombinations)?.TaxID,
						TaxCategoryID = TxCategory.Current?.TaxCategoryID
					}) as TaxCategoryDet;

				Details.Cache.RaiseExceptionHandling<TaxCategoryDet.taxID>(invalidRow, invalidRow.TaxID,
					new PXSetPropertyException(Messages.SeveralImportTaxesCanNotBeInSameZoneAndCategory,
					invalidRow?.TaxCategoryID, ((TaxZoneDet)invalidZoneCombinations)?.TaxZoneID));

				return false;
			}

			return true;
		}


		/*
		 * Add Later. Currently, there is no constraints in the database.
		public override void Persist()
		{
			try
			{
				base.Persist();
			}
			catch (PXDatabaseException e)
			{
				if (e.Message.IndexOf("DELETE") != -1 && e.Message.IndexOf("constraint 'Segment_SegmentValue_FK1'") != -1)
				{
					throw new Exception("Segment '" + e.Keys[1] + "' has values and cannot be deleted.");
				}
				else
				{
					throw;
				}
			}
		}
		*/
	}
}

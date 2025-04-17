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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PM;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.GraphExtensions.NonStockItemMaintExt
{
	public class ConvertNonStockToStockExt : ConvertStockToNonStockExtBase<NonStockItemMaint, InventoryItemMaint>
	{
		public static bool IsActive()
			=> InventoryItemMaintExt.ConvertStockToNonStockExt.IsActive();

		[PXCopyPasteHiddenView]
		public
			SelectFrom<INItemSite>.
			Where<INItemSite.inventoryID.IsEqual<@P.AsInt>>.
			View AllItemSiteRecords;

		[PXCopyPasteHiddenView]
		public
			SelectFrom<INItemRep>.
			Where<INItemRep.inventoryID.IsEqual<@P.AsInt>>.
			View AllReplenishmentRecords;

		[PXCopyPasteHiddenView]
		public
			SelectFrom<INItemBoxEx>.
			Where<INItemBoxEx.inventoryID.IsEqual<@P.AsInt>>.
			View Boxes;

		public override void Initialize()
		{
			base.Initialize();

			convert.SetCaption(Messages.ConvertToStockItem);
		}

		#region Verification

		protected override int Verify(InventoryItem item, List<string> errors)
		{
			int numberOfErrors = base.Verify(item, errors);

			numberOfErrors += VerifyKitSpec(item, errors);
			numberOfErrors += VerifyCashEntry(item, errors);
			numberOfErrors += VerifyARSetup(item, errors);

			if (PXAccess.FeatureInstalled<FeaturesSet.contractManagement>())
			{
				numberOfErrors += VerifyPMTran(item, errors);
				numberOfErrors += VerifyContractTemplate(item, errors);
				numberOfErrors += VerifyContractItem(item, errors);
			}

			return numberOfErrors;
		}

		protected override int VerifyInventoryItem(InventoryItem item, List<string> errors)
		{
			if (item.ItemType != INItemTypes.NonStockItem || item.NonStockReceipt != true || item.NonStockShip != true)
			{
				throw new PXException(Messages.CannotConvertItemTypeIsNotNonStock, item.InventoryCD.Trim());
			}

			return base.VerifyInventoryItem(item, errors);
		}

		protected virtual int VerifyKitSpec(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<INKitSpecHdr>
				.Where<Exists<Select<INKitSpecNonStkDet, Where<INKitSpecNonStkDet.compInventoryID.IsEqual<@P.AsInt>
					.And<INKitSpecNonStkDet.FK.KitSpecification>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<INKitSpecHdr>().ToArray();

			if (documents.Any())
			{
				var listOfSpec = documents.Select(s => PXLocalizer.LocalizeFormat(Messages.CannotConvertKitSpecListItem,
					InventoryItem.PK.Find(Base, s.KitInventoryID)?.InventoryCD.Trim(), s.RevisionID));

				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertKitSpecList, item.InventoryCD.Trim(), string.Join("; ", listOfSpec));

				PXTrace.WriteError(error);
				errors.Add(documents.Length == 1 ? error :
					PXLocalizer.LocalizeFormat(Messages.CannotConvertKitSpecSeeTrace, item.InventoryCD.Trim()));
			}

			return documents.Length;
		}

		protected virtual int VerifyCashEntry(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<CAAdj>
				.Where<CAAdj.released.IsNotEqual<True>
					.And<Exists<Select<CASplit, Where<CASplit.inventoryID.IsEqual<@P.AsInt>
						.And<CASplit.FK.CashTransaction>>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<CAAdj>().ToArray();

			if (documents.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertCashEntries, item.InventoryCD.Trim(),
					string.Join(", ", documents.Select(c => c.RefNbr)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return documents.Length;
		}


		protected override List<string> GetAPTranTypes()
		{
			var list = base.GetAPTranTypes();
			list.Add(APDocType.CreditAdj);
			list.Add(APDocType.QuickCheck);
			list.Add(APDocType.VoidQuickCheck);

			return list;
		}

		protected override string GetAPTranMessage(string apDocType)
		{
			switch (apDocType)
			{
				case APDocType.CreditAdj:
					return Messages.CannotConvertAPCreditAdjustments;
				case APDocType.QuickCheck:
				case APDocType.VoidQuickCheck:
					return Messages.CannotConvertQuickChecks;
				default:
					return base.GetAPTranMessage(apDocType);
			}
		}

		protected virtual int VerifyARSetup(InventoryItem item, List<string> errors)
		{
			ARSetup setup = SelectFrom<ARSetup>
				.Where<ARSetup.dunningFeeInventoryID.IsEqual<@P.AsInt>>
				.View.ReadOnly.Select(Base, item.InventoryID);

			if (setup != null)
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertDunningFeeItem, item.InventoryCD.Trim());

				PXTrace.WriteError(error);
				errors.Add(error);

				return 1;
			}

			return 0;
		}

		protected virtual int VerifyPMTran(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<Contract>
				.Where<Contract.baseType.IsEqual<CTPRType.contract>
					.And<Contract.status.IsNotIn<Contract.status.draft, Contract.status.inApproval>>
					.And<Exists<Select<PMTran, Where<PMTran.inventoryID.IsEqual<@P.AsInt>
						.And<PMTran.billed.IsNotEqual<True>>
						.And<PMTran.projectID.IsEqual<Contract.contractID>>>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<Contract>().ToArray();

			if (documents.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertContracts, item.InventoryCD.Trim(),
					string.Join(",", documents.Select(d => d.ContractCD.Trim())));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return documents.Length;
		}

		protected virtual int VerifyContractTemplate(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<ContractTemplate>
				.Where<ContractTemplate.caseItemID.IsEqual<@P.AsInt>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<ContractTemplate>().ToArray();

			if (documents.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertContractTemplates, item.InventoryCD.Trim(),
					string.Join(", ", documents.Select(d => d.ContractCD)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return documents.Length;
		}

		protected virtual int VerifyContractItem(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<ContractItem>
				.Where<ContractItem.baseItemID.IsEqual<@P.AsInt>
					.Or<ContractItem.renewalItemID.IsEqual<@P.AsInt>>
					.Or<ContractItem.recurringItemID.IsEqual<@P.AsInt>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID, item.InventoryID, item.InventoryID)
				.RowCast<ContractItem>().ToArray();


			if (documents.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertContractItems, item.InventoryCD.Trim(),
					string.Join(", ", documents.Select(d => d.ContractItemCD)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return documents.Length;
		}

		protected virtual void VerifySingleLocationStatus(InventoryItem item)
		{
			INLocationStatusByCostCenter locationStatus = SelectFrom<INLocationStatusByCostCenter>
				.Where<INLocationStatusByCostCenter.qtyOnHand.IsNotEqual<decimal0>
					.And<INLocationStatusByCostCenter.inventoryID.IsEqual<@P.AsInt>>
					.And<INLocationStatusByCostCenter.siteID.IsNotEqual<@P.AsInt>>>
				.View.ReadOnly.Select(Base, item.InventoryID, Base.insetup.Current.TransitSiteID);
			if (locationStatus != null)
			{
				var site = INSite.PK.Find(Base, locationStatus.SiteID);
				throw new PXException(Messages.CannotConvertSiteStatuses, item.InventoryCD.Trim(), site?.SiteCD.Trim());
			}
		}

		protected override void OnBeforeCommitVerifyNoTransactionsCreated(InventoryItem item)
		{
			// verifying item converted from stock to non-stock
			base.OnBeforeCommitVerifyNoTransactionsCreated(item);
			VerifySingleLocationStatus(item);
		}

		#endregion // Verification

		#region Convert

		protected override InventoryItem ConvertMainFields(InventoryItemMaint graph, InventoryItem source, InventoryItem newItem)
		{
			newItem = base.ConvertMainFields(graph, source, newItem);

			InventoryItemCurySettings curySettings = Base.ItemCurySettings.Select(source.InventoryID);
			if (curySettings?.StdCost.IsNotIn(null, 0m) == true)
			{
				newItem.ValMethod = INValMethod.Standard;
			}
			else
			{
				newItem.ValMethod = null;
			}

			return newItem;
		}

		#endregion // Convert

		#region Events

		protected override void _(Events.RowSelected<InventoryItem> e)
		{
			base._(e);

			if (e.Row?.IsConverted == true)
			{
				e.Cache.AdjustUI().For<InventoryItem.itemType>(a => a.Enabled = false)
					.SameFor<InventoryItem.nonStockReceipt>()
					.SameFor<InventoryItem.nonStockShip>();
			}
		}

		#endregion // Events
	}
}

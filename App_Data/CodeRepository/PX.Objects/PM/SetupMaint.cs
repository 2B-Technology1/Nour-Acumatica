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
using PX.Common;
using PX.Data;
using PX.Metadata;
using PX.Objects.CN.ProjectAccounting.PM.GraphExtensions;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.PM
{
	public class SetupMaint : PXGraph<SetupMaint>
	{
		[InjectDependency]
		protected IScreenInfoCacheControl ScreenInfoCacheControl { get; set; }

		#region DAC Overrides

		#region PMProject
		[PXDBIdentity(IsKey = true)]
		protected virtual void _(Events.CacheAttached<PMProject.contractID> e)
		{ }

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Project Template ID")]
		protected virtual void _(Events.CacheAttached<PMProject.contractCD> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<PMProject.customerID> e)
		{ }

		[PXDBBool]
		[PXDefault(true)]
		protected virtual void _(Events.CacheAttached<PMProject.nonProject> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<PMProject.templateID> e)
		{ }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<PMProject.curyID> e)
		{ }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<PMProject.baseCuryID> e)
		{ }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<PMProject.billingCuryID> e)
		{ }


		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<PMProject.curyInfoID> e)
		{ }

		#endregion

		#region PMCostCode
		[PXDBString(30, IsUnicode = true)]
		[PXDefault()]
		protected virtual void _(Events.CacheAttached<PMCostCode.costCodeCD> e)
		{ }

		[PXDBIdentity(IsKey = true)]
		protected virtual void _(Events.CacheAttached<PMCostCode.costCodeID> e)
		{ }
		#endregion

		#region Inventory Item

		[PXDefault()]
		[PXDBString(InputMask = "", IsUnicode = true)]//IsKey = false in order to update the <N/A>
		protected virtual void _(Events.CacheAttached<InventoryItem.inventoryCD> e)
		{ }

		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		protected virtual void _(Events.CacheAttached<InventoryItem.baseUnit> e)
		{ }

		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		protected virtual void _(Events.CacheAttached<InventoryItem.salesUnit> e)
		{ }

		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		protected virtual void _(Events.CacheAttached<InventoryItem.purchaseUnit> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.reasonCodeSubID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.salesAcctID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.salesSubID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.invtAcctID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.invtSubID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.cOGSAcctID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.cOGSSubID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.stdCstRevAcctID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.stdCstRevSubID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.stdCstVarAcctID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.stdCstVarSubID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.pPVAcctID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.pPVSubID> e)
		{ }
		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.pOAccrualAcctID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.pOAccrualSubID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.lCVarianceAcctID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.lCVarianceSubID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.deferralAcctID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.deferralSubID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.defaultSubItemID> e)
		{ }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<InventoryItem.itemClassID> e)
		{ }

		[PXDBString(TX.TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		protected virtual void _(Events.CacheAttached<InventoryItem.taxCategoryID> e)
		{ }
		#endregion

		#region NotificationSetupRecipient
		[PXDBString(10)]
		[PXDefault]
		[NotificationContactType.ProjectTemplateList]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckDistinct(typeof(NotificationSetupRecipient.contactID),
			Where = typeof(Where<NotificationSetupRecipient.setupID, Equal<Current<NotificationSetupRecipient.setupID>>>))]
		public virtual void _(Events.CacheAttached<NotificationSetupRecipient.contactType> e)
		{
		}

		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationSetupRecipient.contactType),
			typeof(Search2<Contact.contactID,
				LeftJoin<EPEmployee,
							On<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>,
							And<EPEmployee.defContactID, Equal<Contact.contactID>>>>,
				Where<Current<NotificationSetupRecipient.contactType>, Equal<NotificationContactType.employee>,
							And<EPEmployee.acctCD, IsNotNull>>>))]
		public virtual void _(Events.CacheAttached<NotificationSetupRecipient.contactID> e)
		{
		}
		#endregion

		#endregion

		public PXSelect<PMSetup> Setup;
		public PXSave<PMSetup> Save;
		public PXCancel<PMSetup> Cancel;
		public PXSetup<Company> Company;
		public PXSelect<PMProject,
			Where<PMProject.nonProject, Equal<True>>> DefaultProject;
		public PXSelect<PMCostCode,
			Where<PMCostCode.isDefault, Equal<True>>> DefaultCostCode;
		public PXSelect<InventoryItem,
		Where<InventoryItem.itemStatus, Equal<InventoryItemStatus.unknown>>> EmptyItem;

		public CRNotificationSetupList<PMNotification> Notifications;
		public PXSelect<NotificationSetupRecipient,
			Where<NotificationSetupRecipient.setupID, Equal<Current<PMNotification.setupID>>>> Recipients;

		public SetupMaint()
		{
			if (string.IsNullOrEmpty(Company.Current.BaseCuryID))
			{
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), PXMessages.LocalizeNoPrefix(CS.Messages.OrganizationMaint));
			}

			PXDefaultAttribute.SetPersistingCheck<PM.PMProject.defaultSalesSubID>(DefaultProject.Cache, null, PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<PM.PMProject.defaultExpenseSubID>(DefaultProject.Cache, null, PXPersistingCheck.Nothing);
		}


		//public PXAction<PMSetup> debug;
		//[PXUIField(DisplayName = "Debug")]
		//[PXProcessButton]
		//public void Debug()
		//{

		//}

		
		protected virtual void _(Events.RowSelected<PMSetup> e)
		{
			EnsureDefaultCostCode(e.Row);

			PMProject rec = DefaultProject.SelectWindowed(0, 1);
			if (rec != null && IsInvalid(rec))
			{
				rec.IsActive = true;
				rec.Status = ProjectStatus.Active;
				rec.RestrictToEmployeeList = false;
				rec.RestrictToResourceList = false;
				rec.VisibleInAP = true;
				rec.VisibleInAR = true;
				rec.VisibleInCA = true;
				rec.VisibleInCR = true;
				rec.VisibleInEA = true;
				rec.VisibleInGL = true;
				rec.VisibleInIN = true;
				rec.VisibleInPO = true;
				rec.VisibleInSO = true;
				rec.VisibleInTA = true;
				rec.CustomerID = null;

				if (DefaultProject.Cache.GetStatus(rec) == PXEntryStatus.Notchanged)
					DefaultProject.Cache.SetStatus(rec, PXEntryStatus.Updated);

				DefaultProject.Cache.IsDirty = true;
			}

			SetVisibilityToCostProjectionRows();
			SetVisibilityForProjectQuoteSettings();

			var remainderOptionsRequired = e.Row.UnbilledRemainderAccountID.HasValue || e.Row.UnbilledRemainderOffsetAccountID.HasValue;
			PXUIFieldAttribute.SetRequired<PMSetup.unbilledRemainderAccountID>(e.Cache, remainderOptionsRequired);
			PXUIFieldAttribute.SetRequired<PMSetup.unbilledRemainderOffsetAccountID>(e.Cache, remainderOptionsRequired);
			PXUIFieldAttribute.SetRequired<PMSetup.unbilledRemainderSubID>(e.Cache, remainderOptionsRequired);
			PXUIFieldAttribute.SetRequired<PMSetup.unbilledRemainderOffsetSubID>(e.Cache, remainderOptionsRequired);
			PXDefaultAttribute.SetPersistingCheck<PMSetup.unbilledRemainderAccountID>(e.Cache, e.Row, remainderOptionsRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<PMSetup.unbilledRemainderOffsetAccountID>(e.Cache, e.Row, remainderOptionsRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<PMSetup.unbilledRemainderSubID>(e.Cache, e.Row, remainderOptionsRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<PMSetup.unbilledRemainderOffsetSubID>(e.Cache, e.Row, remainderOptionsRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);

			if (e.Row != null && !PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				PXStringListAttribute.SetList<PMSetup.dropshipExpenseAccountSource>(e.Cache, e.Row,
					new string[] {
							DropshipExpenseAccountSourceOption.PostingClassOrItem,
							DropshipExpenseAccountSourceOption.Project,
							DropshipExpenseAccountSourceOption.Task },
					new string[] {
							Messages.AccountSource_Item,
							Messages.AccountSource_Project,
							Messages.AccountSource_Task });
			}
		}

		protected virtual void _(Events.RowSelected<NotificationSetup> e)
		{
			if (e.Row != null)
			{
				if (e.Row.NotificationCD == ProformaEntry.ProformaNotificationCD || e.Row.NotificationCD == ChangeOrderEntry.ChangeOrderNotificationCD)
				{
					PXUIFieldAttribute.SetEnabled<NotificationSetup.active>(e.Cache, e.Row, false);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<NotificationSetup.active>(e.Cache, e.Row, true);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMSetup, PMSetup.costCommitmentTracking> e)
		{
			PXPageCacheUtils.InvalidateCachedPages();
		}

		protected virtual void _(Events.FieldUpdated<PMSetup, PMSetup.calculateProjectSpecificTaxes> e)
		{
			if (e.Row != null && (bool)e.NewValue == false)
			{
				throw new PXSetPropertyException<PMSetup.calculateProjectSpecificTaxes>(Warnings.ProjectTaxZoneFeatureIsInUse, PXErrorLevel.Warning); ;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMSetup, PMSetup.dropshipReceiptProcessing> e)
		{
			if (e.Row != null && e.NewValue.ToString() == DropshipReceiptProcessingOption.SkipReceipt)
			{
				e.Cache.SetValueExt<PMSetup.dropshipExpenseRecording>(e.Row, DropshipExpenseRecordingOption.OnBillRelease);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMNotification.reportID> e)
        {
			if ( (string)e.NewValue == ProformaEntryExt.AIAReport ||
				(string)e.NewValue == ProformaEntryExt.AIAWithQtyReport)
            {
				throw new PXSetPropertyException(Messages.ReportsNotSupported, ProformaEntryExt.AIAReport, ProformaEntryExt.AIAWithQtyReport); ;
            }
        }

		protected virtual void _(Events.FieldDefaulting<PMSetup, PMSetup.stockInitRequired> e)
        {
			e.NewValue = PXAccess.FeatureInstalled<FeaturesSet.materialManagement>();
        }

		public virtual void PMSetup_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PMSetup row = (PMSetup)e.Row;
			if (row == null) return;

            PMProject rec = DefaultProject.SelectWindowed(0, 1);
            if (rec == null)
            {
                InsertDefaultProject(row);
            }
            else
            {
                rec.ContractCD = row.NonProjectCode;
				rec.IsActive = true;
				rec.Status = ProjectStatus.Active;
				rec.VisibleInAP = true;
				rec.VisibleInAR = true;
				rec.VisibleInCA = true;
				rec.VisibleInCR = true;
				rec.VisibleInEA = true;
				rec.VisibleInGL = true;
				rec.VisibleInIN = true;
				rec.VisibleInPO = true;
				rec.VisibleInSO = true;
				rec.VisibleInTA = true;
				rec.RestrictToEmployeeList = false;
				rec.RestrictToResourceList = false;
				rec.CuryID = Company.Current.BaseCuryID;				

	            DefaultProject.Cache.MarkUpdated(rec, assertError: true);
            }

			EnsureDefaultCostCode(row);
			EnsureEmptyItem(row);
		}
		public virtual void PMSetup_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PMProject rec = DefaultProject.SelectWindowed(0, 1);
			PMSetup row = (PMSetup)e.Row;
			if (row == null) return;

			if(rec == null)
			{
				InsertDefaultProject(row);
			}
			else if(!sender.ObjectsEqual<PMSetup.nonProjectCode>(e.Row, e.OldRow))
			{
				rec.ContractCD = row.NonProjectCode;
				DefaultProject.Cache.MarkUpdated(rec, assertError: true);
			}

			InventoryItem item = EmptyItem.SelectWindowed(0, 1);
			
			if (item == null)
			{
				InsertEmptyItem(row);
			}
			else if (!sender.ObjectsEqual<PMSetup.emptyItemCode>(e.Row, e.OldRow))
			{
				item.InventoryCD = row.EmptyItemCode;
				
				if (EmptyItem.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
					EmptyItem.Cache.SetStatus(item, PXEntryStatus.Updated);
			}

			EnsureEmptyItem(row);
		}

		protected virtual void _(Events.FieldVerifying<PMSetup, PMSetup.dropshipExpenseRecording> e)
		{
			if (e.Row != null && (string)e.NewValue == DropshipExpenseRecordingOption.OnReceiptRelease && PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				INSetup inSetup = PXSelect<INSetup>.Select(this);
				if (inSetup?.UpdateGL != true)
				{
					throw new PXSetPropertyException<PMSetup.dropshipExpenseRecording>(Messages.ProjectDropShipPostExpensesUpdateGLInactive);
				}
			}
		}
				
		public virtual bool IsInvalid(PMProject nonProject)
		{
			if (nonProject.IsActive == false) return true;
			if (nonProject.Status != ProjectStatus.Active) return true;
			if (nonProject.RestrictToEmployeeList == true) return true;
			if (nonProject.RestrictToResourceList == true) return true;
			if (nonProject.VisibleInAP == false) return true;
			if (nonProject.VisibleInAR == false) return true;
			if (nonProject.VisibleInCA == false) return true;
			if (nonProject.VisibleInCR == false) return true;
			if (nonProject.VisibleInEA == false) return true;
			if (nonProject.VisibleInGL == false) return true;
			if (nonProject.VisibleInIN == false) return true;
			if (nonProject.VisibleInPO == false) return true;
			if (nonProject.VisibleInSO == false) return true;
			if (nonProject.VisibleInTA == false) return true;
			if (nonProject.CustomerID != null) return true;

			return false;
		}
		
		public virtual void InsertDefaultProject(PMSetup row)
		{
			PMProject rec = new PMProject();
			rec.CustomerID = null;
			rec.ContractCD = row.NonProjectCode;
			rec.Description = PXLocalizer.Localize(Messages.NonProjectDescription);
			PXDBLocalizableStringAttribute.SetTranslationsFromMessage<PMProject.description>
				(Caches[typeof(PMProject)], rec, Messages.NonProjectDescription);
			rec.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
			rec.IsActive = true;
			rec.Status = ProjectStatus.Active;
			rec.ServiceActivate = false;
			rec.VisibleInAP = true;
			rec.VisibleInAR = true;
			rec.VisibleInCA = true;
			rec.VisibleInCR = true;
			rec.VisibleInEA = true;
			rec.VisibleInGL = true;
			rec.VisibleInIN = true;
			rec.VisibleInPO = true;
			rec.VisibleInSO = true;
			rec.VisibleInTA = true;
			rec = DefaultProject.Insert(rec);
		}

		public virtual void EnsureDefaultCostCode(PMSetup row)
		{
			PMCostCode costcode = DefaultCostCode.SelectWindowed(0, 1);
			if (costcode == null)
			{
				InsertDefaultCostCode(row);
			}
			else 
			{
				if (costcode.CostCodeCD.Length != GetCostCodeLength() )
				{
					costcode.CostCodeCD = new string('0', GetCostCodeLength());
					if (DefaultCostCode.Cache.GetStatus(costcode) == PXEntryStatus.Notchanged)
						DefaultCostCode.Cache.SetStatus(costcode, PXEntryStatus.Updated);

					DefaultCostCode.Cache.IsDirty = true;
				}
				if (costcode.NoteID == null)
				{
					costcode.NoteID = Guid.NewGuid();
					if (DefaultCostCode.Cache.GetStatus(costcode) == PXEntryStatus.Notchanged)
						DefaultCostCode.Cache.SetStatus(costcode, PXEntryStatus.Updated);

					DefaultCostCode.Cache.IsDirty = true;
				}

			}
		}
		
		public virtual void InsertDefaultCostCode(PMSetup row)
		{
			PMCostCode rec = new PMCostCode();
			rec.CostCodeCD = new string('0', GetCostCodeLength());
			rec.Description = "DEFAULT";
			rec.IsDefault = true;
			rec = DefaultCostCode.Insert(rec);
		}

		public virtual short GetCostCodeLength()
		{
			Dimension dm = PXSelect<Dimension, Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>.Select(this, CostCodeAttribute.COSTCODE);

			if (dm != null && dm.Length != null)
			{
				return dm.Length.Value;
			}
			else
			{
				return 4;
			}
		}

		public virtual void EnsureEmptyItem(PMSetup row)
		{
			InventoryItem item = EmptyItem.SelectWindowed(0, 1);
			if (item == null)
			{
				InsertEmptyItem(row);
			}
			else
			{
				UpdateEmptyItem(item, row);
			}
		}

		public virtual void InsertEmptyItem(PMSetup row)
		{
			InventoryItem rec = new InventoryItem();
			rec.InventoryCD = row.EmptyItemCode;
			rec.ItemStatus = InventoryItemStatus.Unknown;
			rec.ItemType = INItemTypes.NonStockItem;
			rec.BaseUnit = row.EmptyItemUOM;
			rec.SalesUnit = row.EmptyItemUOM;
			rec.PurchaseUnit = row.EmptyItemUOM;
			rec.StkItem = false;
			rec.NonStockReceipt = false;
			rec.TaxCalcMode = TX.TaxCalculationMode.TaxSetting;
			rec = EmptyItem.Insert(rec);
		}

		protected virtual void UpdateEmptyItem(InventoryItem rec, PMSetup row)
		{
			if (rec.BaseUnit != row.EmptyItemUOM || rec.SalesUnit != row.EmptyItemUOM || rec.PurchaseUnit != row.EmptyItemUOM)
			{
				rec.BaseUnit = row.EmptyItemUOM;
				rec.SalesUnit = row.EmptyItemUOM;
				rec.PurchaseUnit = row.EmptyItemUOM;

				EmptyItem.Cache.MarkUpdated(rec, assertError: true);
			}
		}

		private void SetVisibilityToCostProjectionRows()
		{
			bool isVisible = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>() && PXAccess.FeatureInstalled<FeaturesSet.construction>();

			SetSetupPropertyVisible<PMSetup.costProjectionApprovalMapID>(isVisible);
			SetSetupPropertyVisible<PMSetup.costProjectionApprovalNotificationID>(isVisible);
		}

		private void SetVisibilityForProjectQuoteSettings()
		{
			bool isVisible = PXAccess.FeatureInstalled<FeaturesSet.projectQuotes>();

			SetSetupPropertyVisible<PMSetup.quoteApprovalMapID>(isVisible);
			SetSetupPropertyVisible<PMSetup.quoteApprovalNotificationID>(isVisible);
		}

		private void SetSetupPropertyVisible<TPropertyType>(bool visible)
			where TPropertyType : IBqlField
		{
			PXUIVisibility visibility = visible ? PXUIVisibility.Visible : PXUIVisibility.Invisible;

			PXUIFieldAttribute.SetVisibility<TPropertyType>(Setup.Cache, null, visibility);
			PXUIFieldAttribute.SetVisible<TPropertyType>(Setup.Cache, null, visible);
		}

		public override void Persist()
		{
			bool reactivateExtensions = false;
			if (Setup.Current != null)
			{
				bool? oldValue = (bool?)Setup.Cache.GetValueOriginal<PMSetup.calculateProjectSpecificTaxes>(Setup.Current);
				if (oldValue.GetValueOrDefault() != Setup.Current.CalculateProjectSpecificTaxes.GetValueOrDefault())
				{
					reactivateExtensions = true;
				}
			}

			base.Persist();

			if (reactivateExtensions)
			{
				PXDatabase.Update<FeaturesSet>(new PXDataFieldAssign(typeof(FeaturesSet.projectAccounting).Name, PXDbType.Bit, 1));
			}

			if (this.TryGetScreenIdFor<ProformaEntry>(out string screenID))
			{
				// Visibility of Insert action at ProformaEntry screen
				// depends on the value of PMSetup.MigrationMode.
				// If the value is changed Insert should be hidden/shown at PM3070PL also.
				// But the state of primary screen for GenInq is cached
				// (see PXGenericInqGrphInitializePrimaryScreenActions).
				// Hence to change Insert state at PM3070PL cache should be
				// invalidated for PM307000.
				ScreenInfoCacheControl.InvalidateCache(screenID);
			}
		}
	}
}

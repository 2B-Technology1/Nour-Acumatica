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
using System.Collections;
using System.Text;
using PX.Data;
using PX.Data.Descriptor;
using PX.Objects.CA;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;
using PX.Objects.TX;
using System.Linq;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.AR
{
	public class CustomerClassMaint : PXGraph<CustomerClassMaint, CustomerClass>
	{
		[InjectDependency]
		internal IBAccountRestrictionHelper BAccountRestrictionHelper { get; set; }

		public PXSelect<CustomerClass> CustomerClassRecord;
		public PXSelect<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<CustomerClass.customerClassID>>>> CurCustomerClassRecord;
        [PXViewName(CR.Messages.Attributes)]
        public CSAttributeGroupList<CustomerClass, Customer> Mapping;

        public CRClassNotificationSourceList<NotificationSource, 
			CustomerClass.customerClassID, ARNotificationSource.customer> NotificationSources;

		public PXSelect<NotificationRecipient,
			Where<NotificationRecipient.refNoteID, IsNull,
			  And<NotificationRecipient.sourceID, Equal<Optional<NotificationSource.sourceID>>>>> NotificationRecipients;

		public PXSelect<Customer,
			Where<Customer.customerClassID, Equal<Current<CustomerClass.customerClassID>>>> Customers;

		public PXSelect<ARDunningCustomerClass,
			Where<ARDunningCustomerClass.customerClassID, Equal<Current<CustomerClass.customerClassID>>>> DunningSetup;

		public PXSetup<ARSetup> ARSetup;

		#region Cache Attached
		#region NotificationSource
		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search<NotificationSetup.setupID,
			Where<NotificationSetup.sourceCD, Equal<ARNotificationSource.customer>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
		[PXUIField(DisplayName = "Mailing ID")]
		[PXUIEnabled(typeof(Where<NotificationSource.setupID.IsNull>))]
		protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(CustomerClass.customerClassID))]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select2<CustomerClass,
			InnerJoin<NotificationSetup, On<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>,
			Where<CustomerClass.customerClassID, Equal<Current<NotificationSource.classID>>>>))]
		protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report")]
		[PXDefault(typeof(Search<NotificationSetup.reportID,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SiteMap.screenID,
		Where<SiteMap.url, Like<Common.urlReports>,
			And<Where<SiteMap.screenID, Like<PXModule.ar_>,
						 Or<SiteMap.screenID, Like<PXModule.so_>,
						 Or<SiteMap.screenID, Like<PXModule.cr_>>>>>>,
		OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
		Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
		DescriptionField = typeof(SiteMap.title))]
		[PXFormula(typeof(Default<NotificationSource.setupID>))]
		protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region NotificationRecipient
		[PXDBInt]
		[PXDBDefault(typeof(NotificationSource.sourceID))]
		protected virtual void NotificationRecipient_SourceID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10)]
		[PXDefault]
		[CustomerContactType.ClassList]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckDistinct(typeof(NotificationRecipient.contactID),
			Where = typeof(Where<NotificationRecipient.refNoteID, IsNull, And<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>>>))]
		protected virtual void NotificationRecipient_ContactType_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationRecipient.contactType))]
		protected virtual void NotificationRecipient_ContactID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#endregion

		public PXMenuAction<CustomerClass> ActionsMenu;

		public PXAction<CustomerClass> resetGroup;
		[PXProcessButton(IsLockedOnToolbar = true)]
		[PXUIField(DisplayName = "Include Customers in Restriction Group")]
		protected virtual IEnumerable ResetGroup(PXAdapter adapter)
		{
			if (CustomerClassRecord.Ask(Messages.Warning, Messages.GroupUpdateConfirm, MessageButtons.OKCancel) == WebDialogResult.OK)
			{
				Save.Press();
				string classID = CustomerClassRecord.Current.CustomerClassID;
				PXLongOperation.StartOperation(this, delegate()
				{
					Reset(classID);
				});
			}
			return adapter.Get();
		}
		protected static void Reset(string classID)
		{
			CustomerClassMaint graph = PXGraph.CreateInstance<CustomerClassMaint>();
			graph.CustomerClassRecord.Current = graph.CustomerClassRecord.Search<CustomerClass.customerClassID>(classID);
			if (graph.CustomerClassRecord.Current != null)
			{
				foreach (Customer cust in graph.Customers.Select())
				{
					cust.GroupMask = graph.CustomerClassRecord.Current.GroupMask;
					graph.Customers.Cache.SetStatus(cust, PXEntryStatus.Updated);
				}
				graph.Save.Press();
			}
		}
		public PXSelect<PX.SM.Neighbour> Neighbours;
		public override void Persist()
		{
			if (CustomerClassRecord.Current != null)
			{
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(PX.SM.Users), typeof(PX.SM.Users));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(Customer), typeof(Customer));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(CustomerClass), typeof(CustomerClass));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(PX.SM.Users), typeof(Customer));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(Customer), typeof(PX.SM.Users));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(PX.SM.Users), typeof(CustomerClass));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(CustomerClass), typeof(PX.SM.Users));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(CustomerClass), typeof(Customer));
				CS.SingleGroupAttribute.PopulateNeighbours<CustomerClass.groupMask>(CustomerClassRecord, Neighbours, typeof(Customer), typeof(CustomerClass));
			}
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				BAccountRestrictionHelper.Persist();
			base.Persist();
			GroupHelper.Clear();
				ts.Complete();
			}
		}

		#region Setups
		public PXSetup<GL.Company> Company;
        #endregion

		public virtual void CustomerClass_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{						
			CustomerClass row = (CustomerClass)e.Row;
            if (row == null) return;

			NotificationSource source = this.NotificationSources.Select(row.CustomerClassID);
            this.NotificationRecipients.Cache.AllowInsert = source != null;
            PXUIFieldAttribute.SetEnabled<CustomerClass.creditLimit>(cache, row, (row.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT
                        || row.CreditRule == CreditRuleTypes.CS_BOTH));
            PXUIFieldAttribute.SetEnabled<CustomerClass.overLimitAmount>(cache, row, (row.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT
                        || row.CreditRule == CreditRuleTypes.CS_BOTH));
			PXUIFieldAttribute.SetEnabled<CustomerClass.creditDaysPastDue>(cache, row, (row.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE
						|| row.CreditRule == CreditRuleTypes.CS_BOTH));

			PXUIFieldAttribute.SetEnabled<CustomerClass.smallBalanceLimit>(cache, row, (row.SmallBalanceAllow ?? false));

			PXUIFieldAttribute.SetEnabled<CustomerClass.finChargeID>(cache, row, (row.FinChargeApply ?? false));

			var mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<CustomerClass.curyID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.curyRateTypeID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.printCuryStatements>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.allowOverrideCury>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.allowOverrideRate>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.unrealizedGainAcctID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.unrealizedGainSubID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.unrealizedLossAcctID>(cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CustomerClass.unrealizedLossSubID>(cache, null, mcFeatureInstalled);
		}

        public virtual void CustomerClass_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            CustomerClass cclass = (CustomerClass)e.Row;
            if (cclass == null) return;

            ARSetup setup = PXSelect<ARSetup>.Select(this);
            if (setup != null && cclass.CustomerClassID == setup.DfltCustomerClassID)
            {
                throw new PXException(Messages.CustomerClassCanNotBeDeletedBecauseItIsUsed);
            }
        }

		protected virtual void _(Events.RowUpdated<CustomerClass> e)
		{
			if (IsCopyPasteContext)
			{
				DunningSetup.Cache.Clear();
			}
		}


		protected virtual void CustomerClass_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void CustomerClass_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() == false)
			{
				e.Cancel = true;
			}
		}

		protected virtual void CustomerClass_CuryRateTypeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() == false)
			{
				e.Cancel = true;
			}
		}

		public virtual void CustomerClass_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			CustomerClass row = (CustomerClass)e.Row;
			if (row.FinChargeApply ?? false)
			{
				if (row.FinChargeID== null)
				{
					if (cache.RaiseExceptionHandling<CustomerClass.finChargeID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(CustomerClass.finChargeID)}]")))
					{
						throw new PXRowPersistingException(typeof(CustomerClass.finChargeID).Name, null, ErrorMessages.FieldIsEmpty, typeof(CustomerClass.finChargeID).Name);
					}
				}
			}

			if (row?.RequireAvalaraCustomerUsageType == true && row.AvalaraCustomerUsageType == TXAvalaraCustomerUsageType.Default)
			{
				throw new PXRowPersistingException(typeof(CustomerClass.avalaraCustomerUsageType).Name,
					row.AvalaraCustomerUsageType, Common.Messages.NonDefaultAvalaraUsageType);
			}
		}
		
		public virtual void CustomerClass_StatementCycleId_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerClass row = (CustomerClass)e.Row;
			if (row == null) return;
			if ((row.StatementCycleId != null))
			{
				ARSetup setup = (ARSetup)PXSelect<ARSetup>.Select(this);
				if (setup != null && (setup.DefFinChargeFromCycle == true))
				{
					ARStatementCycle arSC = PXSelect<ARStatementCycle,
												Where<ARStatementCycle.statementCycleId, Equal<Required<ARStatementCycle.statementCycleId>>>>.
																		 Select(this, row.StatementCycleId);
					if ((arSC != null) && (arSC.FinChargeID != null))
					{
						row.FinChargeID = arSC.FinChargeID;
						this.CustomerClassRecord.Cache.RaiseFieldUpdated<CustomerClass.finChargeID>(row, null);
					}
				}
			}
		}	

		public virtual void CustomerClass_CreditRule_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerClass row = (CustomerClass)e.Row;
			if (row.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT
				|| row.CreditRule == CreditRuleTypes.CS_NO_CHECKING) 
			{
				row.CreditDaysPastDue = 0;
			}
			if (row.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE
				|| row.CreditRule == CreditRuleTypes.CS_NO_CHECKING)
			{
				row.CreditLimit = 0m;
			} 

		}

		public virtual void CustomerClass_SmallBalanceAllow_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerClass row = (CustomerClass)e.Row;
			row.SmallBalanceLimit = 0m;
		}

		public virtual void CustomerClass_FinChargeApply_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerClass row = (CustomerClass)e.Row;
			if (!(row.FinChargeApply ?? false))
			{
				row.FinChargeID= null;			
			}
		}

		#region DunningSetup event handling
		protected virtual void _(Events.RowInserted<CustomerClass> e)
		{
			if (e.Row.CustomerClassID != null)
			{
				foreach (ARDunningSetup s in SelectFrom<ARDunningSetup>.View.SelectMultiBound(this, null, null))
				{
					ARDunningCustomerClass d = new ARDunningCustomerClass();
					d.DunningLetterLevel = s.DunningLetterLevel;
					d.CustomerClassID = CustomerClassRecord.Current.CustomerClassID;
					d = DunningSetup.Cache.Insert(d) as ARDunningCustomerClass;

					d.DueDays = s.DueDays;
					d.DaysToSettle = s.DaysToSettle;
					d.Descr = s.Descr;
					d.DunningFee = s.DunningFee;
					DunningSetup.Cache.Update(d);
				}
			}
		}


		// Deleting order control. Prevents break of consecutive enumeration.
		protected virtual void _(Events.RowDeleting<ARDunningCustomerClass> e)
		{
			
			if (e.Row != null
				&& !this.Caches<CustomerClass>().Deleted.Cast<CustomerClass>().Any(c => c.CustomerClassID == e.Row.CustomerClassID)
				&& this.Caches<CustomerClass>().GetStatus(CustomerClassRecord.Current) != PXEntryStatus.InsertedDeleted)
			{
				int MaxRN = 0;
				foreach (ARDunningCustomerClass v in SelectFrom<ARDunningCustomerClass>
							.Where<ARDunningCustomerClass.customerClassID.IsEqual<@P.AsString>>.View.SelectMultiBound(this, null, e.Row.CustomerClassID))
				{					
					int MaxR = v.DunningLetterLevel.Value;
					MaxRN = MaxRN < MaxR ? MaxR : MaxRN;
				}

				if (e.Row.DunningLetterLevel.Value < MaxRN)
				{
					throw new PXException(Messages.OnlyLastRowCanBeDeleted);
				}				
			}
		}

		// Prevents break of monotonically increasing values
		protected virtual void _(Events.FieldVerifying<ARDunningCustomerClass, ARDunningCustomerClass.dueDays> e) 
		{
			if (e.Row != null)
			{
				int llevel = e.Row.DunningLetterLevel.Value;
				int nv = Convert.ToInt32(e.NewValue);
				if (llevel == 1 && nv <= 0)
				{
					throw new PXSetPropertyException(Messages.ThisValueMUSTExceed, 0);
				}
				else
				{
					int NextValue = 0;
					int PrevValue = 0;
					foreach (ARDunningCustomerClass v in PXSelect<ARDunningCustomerClass,
								Where<ARDunningCustomerClass.customerClassID, Equal<Required<ARDunningCustomerClass.customerClassID>>>>
							.Select(this, e.Row.CustomerClassID))
					{						
						if (v.DunningLetterLevel.Value == llevel - 1)
						{
							PrevValue = v.DueDays.Value;
						}
						if (v.DunningLetterLevel.Value == llevel + 1)
						{
							NextValue = v.DueDays.Value;
						}
					}
					if (nv <= PrevValue)
					{
						throw new PXSetPropertyException(Messages.ThisValueMUSTExceed, PrevValue);
					}
					if (nv >= NextValue && NextValue > 0)
					{
						throw new PXSetPropertyException(Messages.ThisValueCanNotExceed, NextValue);
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<ARDunningCustomerClass.dunningFee> e)
		{
			if (e.NewValue != null
				&& (Decimal)e.NewValue != 0
				&& !ARSetup.Current.DunningFeeInventoryID.HasValue)
			{
				throw new PXSetPropertyException(Messages.DunningFeeItem);
			}
		}

		// Computing default value on the basis of the previous values
		protected virtual void _(Events.FieldDefaulting<ARDunningCustomerClass, ARDunningCustomerClass.dueDays> e)
		{
			if (e.Row?.DunningLetterLevel != null)
			{
				int llevel = e.Row.DunningLetterLevel.Value;

				if (llevel == 1)
				{
					e.NewValue = 30;
				}
				else
				{
					int PrevValue = 0;
					foreach (ARDunningCustomerClass v in PXSelect<ARDunningCustomerClass,
						Where<ARDunningCustomerClass.customerClassID, Equal<Current<ARDunningCustomerClass.customerClassID>>>>.Select(this))
					{
						if (v.DunningLetterLevel.Value == llevel - 1)
						{
							PrevValue += v.DueDays.Value;
						}
						if (v.DunningLetterLevel.Value == 1 && llevel > 1)
						{
							PrevValue += v.DueDays.Value;
						}
					}
					e.NewValue = PrevValue;
				}
			}
		}

		// Computing default value on the basis of the previous values
		protected virtual void _(Events.FieldDefaulting<ARDunningCustomerClass.dunningLetterLevel> e)
		{
			var items = PXSelect<ARDunningCustomerClass,
				Where<ARDunningCustomerClass.customerClassID, Equal<Current<ARDunningCustomerClass.customerClassID>>>>
				.Select(this).RowCast<ARDunningCustomerClass>().ToList();
			e.NewValue = items.Any() ? items.OrderByDescending(_ => _.DunningLetterLevel).First().DunningLetterLevel + 1 : 1;
		}

		[PXMergeAttributes (Method = MergeMethod.Merge)]
		[PXDefault(typeof(CustomerClass.customerClassID))]
		[PXParent(typeof(Select<CustomerClass,
			Where<CustomerClass.customerClassID, Equal<Current<ARDunningCustomerClass.customerClassID>>>>))]
		protected virtual void _(Events.CacheAttached<ARDunningCustomerClass.customerClassID> e)
		{ }

		protected virtual void _(Events.RowSelected<ARDunningCustomerClass> e)
		{
			if (e.Row != null)
			{
				ARDunningCustomerClass nextDL =
					SelectFrom<ARDunningCustomerClass>
					.Where<ARDunningCustomerClass.customerClassID.IsEqual<@P.AsString>
						.And<ARDunningCustomerClass.dunningLetterLevel.IsGreater<@P.AsInt>>>
					.OrderBy<ARDunningCustomerClass.dunningLetterLevel.Asc>
					.View.SelectSingleBound(this, null, e.Row.CustomerClassID, e.Row.DunningLetterLevel);

				bool clear = true;
				if (nextDL != null && nextDL.DueDays.HasValue)
				{
					if (e.Row.DueDays.HasValue && e.Row.DaysToSettle.HasValue)
					{
						int delay = e.Row.DueDays.Value + e.Row.DaysToSettle.Value;
						if (delay > nextDL.DueDays)
						{
							string dueDaysLabel = PXUIFieldAttribute.GetDisplayName<ARDunningCustomerClass.dueDays>(e.Cache);
							string daysToSettleLabel = PXUIFieldAttribute.GetDisplayName<ARDunningCustomerClass.daysToSettle>(e.Cache);
							e.Cache.RaiseExceptionHandling<ARDunningCustomerClass.daysToSettle>(e.Row, e.Row.DaysToSettle, new PXSetPropertyException(Messages.DateToSettleCrossDunningLetterOfNextLevel, PXErrorLevel.Warning, dueDaysLabel, daysToSettleLabel));
	
							clear = false;
						}
					}
				}
				if (clear)
				{
					e.Cache.RaiseExceptionHandling<ARDunningCustomerClass.daysToSettle>(e.Row, e.Row.DaysToSettle, null);
				}
			}
		}

		#endregion

		public CustomerClassMaint() 
		{
			PXUIFieldAttribute.SetVisible<CustomerClass.cOGSAcctID>(CustomerClassRecord.Cache, null, false);
			PXUIFieldAttribute.SetVisible<CustomerClass.cOGSSubID>(CustomerClassRecord.Cache, null, false);
			PXUIFieldAttribute.SetVisible<CustomerClass.miscAcctID>(CustomerClassRecord.Cache, null, false);
			PXUIFieldAttribute.SetVisible<CustomerClass.miscSubID>(CustomerClassRecord.Cache, null, false);

			PXUIFieldAttribute.SetVisible<CustomerClass.localeName>(CustomerClassRecord.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);

		}
	}
}

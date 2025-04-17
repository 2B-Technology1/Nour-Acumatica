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
using System.Collections;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.EP;
using System.Linq;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.PM
{
    [Serializable]
	public class ReverseUnbilledProcess : PXGraph<ReverseUnbilledProcess>
	{
		#region DAC Attributes Override
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDecimal]
        [PXUIField(DisplayName = "Total Quantity", Enabled = false)]
        protected virtual void PMRegister_QtyTotal_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDecimal]
        [PXUIField(DisplayName = "Total Billable Quantity", Enabled = false)]
        protected virtual void PMRegister_BillableQtyTotal_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDecimal]
        [PXUIField(DisplayName = "Total Amount", Enabled = false)]
        protected virtual void PMRegister_AmtTotal_CacheAttached(PXCache sender)
        {
        }

        [ProjectTask(typeof(PMTran.projectID))]
        protected virtual void _(Events.CacheAttached<PMTran.taskID> e)
        {
        } 
        #endregion

        public PXCancel<TranFilter> Cancel;
		public PXFilter<TranFilter> Filter;

		/*
		SQL used in BQL:
		SELECT * FROM PMTran where Allocated=1 and Billed=0 and Released=1 and Reversed=0
		AND ( 
			(@dateFrom IS NOT NULL AND @dateTo IS NOT NULL AND @dateFrom=@dateTo AND [Date] = @dateFrom )
		OR
			(@dateFrom IS NOT NULL AND @dateTo IS NOT NULL AND @dateFrom<>@dateTo AND [Date] BETWEEN @dateFrom AND @dateTo )
		OR
			(@dateFrom IS NOT NULL AND @dateto IS NULL AND [Date] >= @dateFrom)
		OR
			(@dateTo IS NOT NULL AND @dateFrom IS NULL AND [Date] <= @dateTo )
		OR
			(@dateFrom IS NULL AND @dateTo IS NULL)
		)
		*/
		public PXFilteredProcessingJoin<PMTran, TranFilter,
			InnerJoin<PMRegister, On<PMRegister.module, Equal<PMTran.tranType>, And<PMRegister.refNbr, Equal<PMTran.refNbr>, And<PMRegister.isAllocation, Equal<True>>>>,
			InnerJoin<PMProject, On<PMProject.contractID, Equal<PMTran.projectID>>,
			InnerJoin<PMTask, On<PMTask.projectID, Equal<PMTran.projectID>, And<PMTask.taskID, Equal<PMTran.taskID>>>,
			InnerJoin<Customer, On<Customer.bAccountID, Equal<PMProject.customerID>>>>>>,
			Where<PMTran.billed, Equal<False>,
				And<PMTran.excludedFromBilling, Equal<False>,
				And<PMTran.released, Equal<True>,
				And2<Where<PMTran.allocated, Equal<True>, Or<PMTran.excludedFromAllocation, Equal<True>>>,
				And2<Where<PMTask.billingID, Equal<Current<TranFilter.billingID>>, Or<Current<TranFilter.billingID>, IsNull>>,
				And2<Where<PMTran.projectID, Equal<Current<TranFilter.projectID>>, Or<Current<TranFilter.projectID>, IsNull>>,
				And2<Where<PMTran.taskID, Equal<Current<TranFilter.projectTaskID>>, Or<Current<TranFilter.projectTaskID>, IsNull>>,
				And2<Where<PMProject.customerID, Equal<Current<TranFilter.customerID>>, Or<Current<TranFilter.customerID>, IsNull>>,
				And2<Where<Customer.customerClassID, Equal<Current<TranFilter.customerClassID>>, Or<Current<TranFilter.customerClassID>, IsNull>>,
				And2<Where<PMTran.inventoryID, Equal<Current<TranFilter.inventoryID>>, Or<Current<TranFilter.inventoryID>, IsNull>>,
				And2<Where<PMTran.resourceID, Equal<Current<TranFilter.resourceID>>, Or<Current<TranFilter.resourceID>, IsNull>>,
				And2<Where2<Where<Current<TranFilter.dateFrom>, IsNotNull, 
						And<Current<TranFilter.dateTo>, IsNotNull, 
						And<Current<TranFilter.dateFrom>, Equal<Current<TranFilter.dateTo>>, 
						And<PMTran.date, Equal<Current<TranFilter.dateFrom>>>>>>,
					Or2<Where<Current<TranFilter.dateFrom>, IsNotNull,
						And<Current<TranFilter.dateTo>, IsNotNull,
						And<Current<TranFilter.dateFrom>, NotEqual<Current<TranFilter.dateTo>>,
						And<PMTran.date, Between<Current<TranFilter.dateFrom>, Current<TranFilter.dateTo>>>>>>,
					Or2<Where<Current<TranFilter.dateFrom>, IsNotNull,
						And<Current<TranFilter.dateTo>, IsNull,
						And<PMTran.date, GreaterEqual<Current<TranFilter.dateFrom>>>>>,
					Or2<Where<Current<TranFilter.dateFrom>, IsNull,
						And<Current<TranFilter.dateTo>, IsNotNull,
						And<PMTran.date, LessEqual<Current<TranFilter.dateTo>>>>>,
					Or<Where<Current<TranFilter.dateFrom>, IsNull,
						And<Current<TranFilter.dateTo>, IsNull>>
					>>>>
				>,
				And<CurrentMatch<PMProject, AccessInfo.userName>>>>>>>>>>>>>>> Items;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMRegister> dummy;//Needed for CacheAttached to work on a Joined PMRegister

		public ReverseUnbilledProcess()
		{			
		}

		public PXAction<TranFilter> viewDocument;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			RegisterEntry graph = CreateInstance<RegisterEntry>();
			graph.Document.Current = graph.Document.Search<PMRegister.refNbr>(Items.Current.RefNbr, Items.Current.TranType);
			throw new PXRedirectRequiredException(graph, "PMTransactions") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}


		#region EventHandlers
		protected virtual void TranFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Items.Cache.Clear();
		}
		protected virtual void TranFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			TranFilter filter = Filter.Current;

			Items.SetProcessDelegate(ReverseAllocatedTran);
		}
		#endregion

		public static void ReverseAllocatedTran(PMTran tran)
		{
			RegisterEntry pmEntry = PXGraph.CreateInstance<RegisterEntry>();
            pmEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            pmEntry.FieldVerifying.AddHandler<PMTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            pmEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

			PMRegister origDoc = PXSelectReadonly<PMRegister, Where<PMRegister.refNbr, Equal<Required<PMRegister.refNbr>>>>.Select(pmEntry, tran.RefNbr);
			PMRegister reversalDoc = (PMRegister)pmEntry.Document.Cache.Insert();
			reversalDoc.OrigDocType = PMOrigDocType.Reversal;
			reversalDoc.OrigNoteID = origDoc.NoteID;
			reversalDoc.Description = PXMessages.LocalizeNoPrefix(Messages.AllocationReversal);
			pmEntry.Document.Current = reversalDoc;

			PMBillEngine engine = PXGraph.CreateInstance<PMBillEngine>();
			foreach (PMTran reverse in engine.ReverseTran(tran))
			{
				UpdateReversalTransactionDateOnAllocationSetting(tran, reverse, pmEntry);

				var inserted = pmEntry.Transactions.Insert(reverse);
				pmEntry.ValidateOffestAccountGroup(tran, inserted);
			}
			tran.ExcludedFromBilling = true;
			tran.ExcludedFromBillingReason = PXMessages.LocalizeNoPrefix(Messages.ExcludedFromBillingAsReversed);
			PM.RegisterReleaseProcess.SubtractFromUnbilledSummary(pmEntry, tran);
			pmEntry.Transactions.Update(tran);

			pmEntry.Save.Press();

			PMSetup setup = PXSelect<PMSetup>.Select(pmEntry);
			if (setup.AutoReleaseAllocation == true)
			{
				RegisterRelease.Release(reversalDoc);
			}
		}

		private static void UpdateReversalTransactionDateOnAllocationSetting(PMTran original, PMTran reversal, RegisterEntry registerEntry)
		{
			if (original.AllocationID == null)
			{
				reversal.Date = null;
				reversal.FinPeriodID = null;

				return;
			}

			PMAllocationDetail allocationDetail = SelectFrom<PMAllocationDetail>
				.Where<PMAllocationDetail.allocationID.IsEqual<P.AsString>>
				.View
				.SelectSingleBound(registerEntry, null, original.AllocationID);
			if (allocationDetail != null && allocationDetail.UseReversalDateFromOriginal == false)
			{
				reversal.Date = null;
				reversal.FinPeriodID = null;
			}
		}


		[PXHidden]
        [Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class TranFilter : IBqlTable
		{
			#region BillingID
			public abstract class billingID : PX.Data.BQL.BqlString.Field<billingID> { }
			protected String _BillingID;
			[PXSelector(typeof(PMBilling.billingID))]
			[PXDBString(PMBilling.billingID.Length, IsUnicode = true)]
			[PXUIField(DisplayName = "Billing Rule")]
			public virtual String BillingID
			{
				get
				{
					return this._BillingID;
				}
				set
				{
					this._BillingID = value;
				}
			}
			#endregion
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			protected String _CustomerClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
			[PXUIField(DisplayName = "Customer Class")]
			public virtual String CustomerClassID
			{
				get
				{
					return this._CustomerClassID;
				}
				set
				{
					this._CustomerClassID = value;
				}
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[Customer()]
			public virtual Int32? CustomerID
			{
				get
				{
					return this._CustomerID;
				}
				set
				{
					this._CustomerID = value;
				}
			}
			#endregion
			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			protected Int32? _ProjectID;
			[Project()]
			public virtual Int32? ProjectID
			{
				get
				{
					return this._ProjectID;
				}
				set
				{
					this._ProjectID = value;
				}
			}
			#endregion
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(TranFilter.projectID))]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[Inventory]
			public virtual Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region ResourceID
			public abstract class resourceID : PX.Data.BQL.BqlInt.Field<resourceID> { }
			protected Int32? _ResourceID;
			[PXEPEmployeeSelector]
			[PXDBInt()]
			[PXUIField(DisplayName = "Employee")]
			public virtual Int32? ResourceID
			{
				get
				{
					return this._ResourceID;
				}
				set
				{
					this._ResourceID = value;
				}
			}
			#endregion
			#region DateFrom
			public abstract class dateFrom : PX.Data.BQL.BqlDateTime.Field<dateFrom> { }
			protected DateTime? _DateFrom;
			[PXDBDate()]
			[PXUIField(DisplayName = "From", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? DateFrom
			{
				get
				{
					return this._DateFrom;
				}
				set
				{
					this._DateFrom = value;
				}
			}
			#endregion
			#region DateTo
			public abstract class dateTo : PX.Data.BQL.BqlDateTime.Field<dateTo> { }
			protected DateTime? _DateTo;
			[PXDBDate()]
			[PXUIField(DisplayName = "To", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? DateTo
			{
				get
				{
					return this._DateTo;
				}
				set
				{
					this._DateTo = value;
				}
			}
			#endregion
		}
				
	}
}

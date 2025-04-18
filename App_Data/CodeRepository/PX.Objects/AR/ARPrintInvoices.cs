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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.TM;
using PX.Objects.CR;
using PX.Common;
using PX.SM;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
	public class ARPrintInvoices : PXGraph<ARPrintInvoices>
	{
		#region Type Override events

		#region BranchID

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Branch", Visible = false)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.branch>.Or<FeatureInstalled<FeaturesSet.multiCompany>>))]
		protected virtual void _(Events.CacheAttached<ARInvoice.branchID> e) { }
		#endregion

		#endregion

		public PXFilter<PrintInvoicesFilter> Filter;
        public PXCancel<PrintInvoicesFilter> Cancel;

		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<PrintInvoicesFilter> EditDetail;
		[PXFilterable]
        public PXFilteredProcessing<ARInvoice, PrintInvoicesFilter> ARDocumentList;	
		public PXSetup<ARSetup> arsetup;

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXEditDetailButton]
        public virtual IEnumerable editDetail(PXAdapter adapter)
        {
            if (ARDocumentList.Current != null)
            {
                PXRedirectHelper.TryRedirect(ARDocumentList.Cache, ARDocumentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
            }
            return adapter.Get();
        }


		public ARPrintInvoices()
		{
			ARSetup setup = arsetup.Current;
			PXUIFieldAttribute.SetEnabled(ARDocumentList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARInvoice.selected>(ARDocumentList.Cache, null, true);
			ARDocumentList.Cache.AllowInsert = false;
			ARDocumentList.Cache.AllowDelete = false;

			ARDocumentList.SetSelected<ARInvoice.selected>();
			ARDocumentList.SetProcessCaption(IN.Messages.Process);
			ARDocumentList.SetProcessAllCaption(IN.Messages.ProcessAll);
		}

		public virtual IEnumerable ardocumentlist(PXAdapter adapter)
		{

			Type select = GetBQLStatement();

			PXView view = new PXView(this, false, BqlCommand.CreateInstance(select));
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = view.
			 Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
				 ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		protected virtual Type GetBQLStatement()
		{
			Type where = PX.TM.OwnedFilter.ProjectionAttribute.ComposeWhere(
				typeof(PrintInvoicesFilter),
				typeof(ARInvoice.workgroupID),
				typeof(ARInvoice.ownerID));

			Type printWhere =
				typeof(Where<ARInvoice.hold, Equal<False>, And<ARInvoice.scheduled, Equal<False>, And<ARInvoice.voided, Equal<False>,
								 And<ARInvoice.dontPrint, Equal<False>,
								 And<ARInvoice.printed, NotEqual<True>>>>>>);
			Type emailWhere =
				typeof(Where<ARInvoice.hold, Equal<False>, And<ARInvoice.scheduled, Equal<False>, And<ARInvoice.voided, Equal<False>,
								And<ARInvoice.creditHold, Equal<False>,
								 And<ARInvoice.dontEmail, Equal<False>,
								And<ARInvoice.emailed, NotEqual<True>>>>>>>);

			Type dateWhere =
				typeof(Where<ARInvoice.docDate, LessEqual<Current<PrintInvoicesFilter.endDate>>,
						And<ARInvoice.docDate, GreaterEqual<Current<PrintInvoicesFilter.beginDate>>>>);

			Type whereAnd;
			if (Filter.Current.ShowAll == true)
			{
				dateWhere = typeof(Where<True, Equal<True>>);
				whereAnd = Filter.Current.Action == "<SELECT>"
								? typeof(Where<True, Equal<False>>)
								: typeof(Where<ARInvoice.hold, Equal<False>, And<ARInvoice.scheduled, Equal<False>, And<ARInvoice.voided, Equal<False>,
											And<ARInvoice.creditHold, Equal<False>>>>>);
			}
			else
			{
				whereAnd = Filter.Current.Action == "<SELECT>" 
					? typeof(Where<True, Equal<False>>) 
					: typeof(Where<True, Equal<True>>);
				switch (Filter.Current.Action)
				{
					case WellKnownActions.ARInvoiceScreen.PrintInvoice:
						whereAnd = printWhere;
						break;
					case WellKnownActions.ARInvoiceScreen.EmailInvoice:
					case WellKnownActions.ARInvoiceScreen.MarkAsDontEmail:
						whereAnd = emailWhere;
						break;
					default:
						whereAnd = typeof(Where<True, Equal<False>>);
						break;
				}
				
			}


			Type select =
				BqlCommand.Compose(
					typeof(Select2<,,>), typeof(ARInvoice),
					typeof(InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>>),
					typeof(Where2<,>),
					typeof(Match<Customer, Current<AccessInfo.userName>>),
					typeof(And2<,>), whereAnd,
					typeof(And2<,>), dateWhere,
					typeof(And<>), where);
			return select;
		}		
		
		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
		protected virtual void PrintInvoicesFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PrintInvoicesFilter filter = (PrintInvoicesFilter)e.Row;

			if (filter != null && !String.IsNullOrEmpty(filter.Action))
			{
				Dictionary<string, object> parameters = Filter.Cache.ToDictionary(e.Row);
				ARDocumentList.SetProcessWorkflowAction(filter.Action, parameters);

				bool showPrintSettings = IsPrintingAllowed(filter);

				PXUIFieldAttribute.SetVisible<PrintInvoicesFilter.printWithDeviceHub>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<PrintInvoicesFilter.definePrinterManually>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<PrintInvoicesFilter.printerID>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<PrintInvoicesFilter.numberOfCopies>(sender, filter, showPrintSettings);

				if (PXContext.GetSlot<AUSchedule>() == null)
				{
					PXUIFieldAttribute.SetEnabled<PrintInvoicesFilter.definePrinterManually>(sender, filter, filter.PrintWithDeviceHub == true);
					PXUIFieldAttribute.SetEnabled<PrintInvoicesFilter.numberOfCopies>(sender, filter, filter.PrintWithDeviceHub == true);
					PXUIFieldAttribute.SetEnabled<PrintInvoicesFilter.printerID>(sender, filter, filter.PrintWithDeviceHub == true && filter.DefinePrinterManually == true);
				}

				if (filter.PrintWithDeviceHub != true || filter.DefinePrinterManually != true)
				{
					filter.PrinterID = null;
				}
			}
		}

		protected virtual bool IsPrintingAllowed(PrintInvoicesFilter filter)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
				filter != null && !String.IsNullOrEmpty(filter.Action) && filter.Action == WellKnownActions.ARInvoiceScreen.PrintInvoice;
		}

		protected virtual void PrintInvoicesFilter_Action_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach (ARInvoice document in ARDocumentList.Cache.Updated)
			{
				ARDocumentList.Cache.SetDefaultExt<ARInvoice.selected>(document);
			}
		}

		public virtual void PrintInvoicesFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if ((!sender.ObjectsEqual<PrintInvoicesFilter.action>(e.Row, e.OldRow) || !sender.ObjectsEqual<PrintInvoicesFilter.definePrinterManually>(e.Row, e.OldRow) || !sender.ObjectsEqual<PrintInvoicesFilter.printWithDeviceHub>(e.Row, e.OldRow)) 
				&& Filter.Current != null && PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && Filter.Current.PrintWithDeviceHub == true && Filter.Current.DefinePrinterManually == true
				&& (PXContext.GetSlot<AUSchedule>() == null || !(Filter.Current.PrinterID != null && ((PrintInvoicesFilter)e.OldRow).PrinterID == null)))
			{
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(ARNotificationSource.Customer, ARReports.InvoiceMemoReportID, Accessinfo.BranchID);
			}
		}

		protected virtual void PrintInvoicesFilter_BeginDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Accessinfo.BusinessDate.Value.AddMonths(-1);
		}

		protected virtual void PrintInvoicesFilter_PrinterName_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PrintInvoicesFilter row = (PrintInvoicesFilter)e.Row;
			if (row != null)
			{
				if (!IsPrintingAllowed(row))
					e.NewValue = null;
			}
		}
		public class WellKnownActions
		{
			public class ARInvoiceScreen
			{
				public const string ScreenID = "AR301000";

				public const string EmailInvoice
					= ScreenID + "$" + nameof(ARInvoiceEntry.emailInvoice);
				public const string MarkAsDontEmail
					= ScreenID + "$" + ARInvoiceEntry_Workflow.MarkAsDontEmail;
				public const string PrintInvoice
					= ScreenID + "$" + nameof(ARInvoiceEntry.printInvoice);


			}
		}
	}

    [Serializable]
	public partial class PrintInvoicesFilter : IBqlTable, PX.SM.IPrintable
	{
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.BQL.BqlInt.Field<currentOwnerID> { }
		[PXDBInt]
		[CR.CRCurrentOwnerID]
		public virtual int? CurrentOwnerID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		protected int? _OwnerID;
		[PX.TM.SubordinateOwner(DisplayName = "Assigned To")]
		public virtual int? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, IsWorkgroupOrSubgroupOfContact<Current<AccessInfo.contactID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion
		#region FilterSet
		public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }
		[PXDefault(false)]
		[PXDBBool]
        public virtual Boolean? FilterSet
		{
			get
			{
				return
					this.OwnerID != null ||
					this.WorkGroupID != null ||
					this.MyWorkGroup == true;
			}
		}
		#endregion
		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
		protected string _Action;
		[PX.Data.Automation.PXWorkflowMassProcessing(DisplayName = "Action")]
		public virtual string Action
		{
			get
			{
				return this._Action;
			}
			set
			{
				this._Action = value;
			}
		}
		#endregion

		#region ShowAll
		public abstract class showAll : PX.Data.BQL.BqlBool.Field<showAll> { }
		protected Boolean? _ShowAll;
		[PXDefault()]
		[PXDBBool]
		[PXUIField(DisplayName = "Show All", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ShowAll
		{
			get
			{
				return _ShowAll;
			}
			set
			{
				_ShowAll = value;
			}
		}
		#endregion
		#region BeginDate
		public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }
		protected DateTime? _BeginDate;
		[PXDate()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? BeginDate
		{
			get
			{
				return this._BeginDate;
			}
			set
			{
				this._BeginDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;
		[PXDate()]
		[PXDefault(typeof(AccessInfo.businessDate),PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion

		#region PrintWithDeviceHub
		public abstract class printWithDeviceHub : PX.Data.BQL.BqlBool.Field<printWithDeviceHub> { }
		protected bool? _PrintWithDeviceHub;
		[PXDBBool]
		[PXDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
		[PXUIField(DisplayName = "Print with DeviceHub")]
		public virtual bool? PrintWithDeviceHub
		{
			get
			{
				return _PrintWithDeviceHub;
			}
			set
			{
				_PrintWithDeviceHub = value;
			}
		}
		#endregion
		#region DefinePrinterManually
		public abstract class definePrinterManually : PX.Data.BQL.BqlBool.Field<definePrinterManually> { }
		protected bool? _DefinePrinterManually = false;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Define Printer Manually")]
		public virtual bool? DefinePrinterManually
		{
			get
			{
				return _DefinePrinterManually;
			}
			set
			{
				_DefinePrinterManually = value;
			}
		}
		#endregion
		#region PrinterID
		public abstract class printerID : PX.Data.BQL.BqlGuid.Field<printerID> { }
		protected Guid? _PrinterID;
		[PX.SM.PXPrinterSelector]
		public virtual Guid? PrinterID
		{
			get
			{
				return this._PrinterID;
			}
			set
			{
				this._PrinterID = value;
			}
		}
		#endregion
		#region NumberOfCopies
		public abstract class numberOfCopies : PX.Data.BQL.BqlInt.Field<numberOfCopies> { }
		protected int? _NumberOfCopies;
		[PXDBInt(MinValue = 1)]
		[PXDefault(1)]
		[PXFormula(typeof(Selector<PrintInvoicesFilter.printerID, PX.SM.SMPrinter.defaultNumberOfCopies>))]
		[PXUIField(DisplayName = "Number of Copies", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? NumberOfCopies
		{
			get
			{
				return this._NumberOfCopies;
			}
			set
			{
				this._NumberOfCopies = value;
			}
		}
		#endregion
	}
}

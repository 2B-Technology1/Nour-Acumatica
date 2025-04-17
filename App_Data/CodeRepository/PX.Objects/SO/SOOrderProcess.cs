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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.TM;
using PX.Objects.RQ;
using PX.Objects.CS;
using PX.Objects.AR.MigrationMode;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;
using PX.Data.BQL;

namespace PX.Objects.SO
{
	public class SOOrderProcess : PXGraph<SOOrderProcess>
	{
		public PXCancel<SOProcessFilter> Cancel;
		public PXAction<SOProcessFilter> viewDocument;

		public PXFilter<SOProcessFilter> Filter;
		[PXFilterable]
		public SOEmailProcessing Records;

		public SOOrderProcess()
		{
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);
		}

		public virtual void _(Events.RowSelected<SOProcessFilter> e)
		{
			if (!string.IsNullOrEmpty(e.Row?.Action))
			{
				Records.SetProcessWorkflowAction(e.Row.Action, Filter.Cache.ToDictionary(e.Row));

				bool showPrintSettings = IsPrintingAllowed(e.Row);

				e.Cache.AdjustUI(e.Row)
					.For<SOProcessFilter.printWithDeviceHub>(ui => ui.Visible = showPrintSettings)
					.For<SOProcessFilter.definePrinterManually>(ui =>
					{
						ui.Visible = showPrintSettings;
						if (PXContext.GetSlot<PX.SM.AUSchedule>() == null)
						{
							ui.Enabled = e.Row.PrintWithDeviceHub == true;
						}
					})
					.SameFor<SOProcessFilter.numberOfCopies>()
					.For<SOProcessFilter.printerID>(ui =>
					{
						ui.Visible = showPrintSettings;
						if (PXContext.GetSlot<PX.SM.AUSchedule>() == null)
						{
							ui.Enabled = e.Row.PrintWithDeviceHub == true && e.Row.DefinePrinterManually == true;
						}
					});
			}
		}

		public virtual bool IsPrintingAllowed(SOProcessFilter filter)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.deviceHub>()
				&& filter.Action.IsIn(WellKnownActions.SOOrderScreen.PrintSalesOrder, WellKnownActions.SOOrderScreen.PrintQuote,
					WellKnownActions.SOOrderScreen.PrintBlanket);
		}
		
		[PXEditDetailButton, PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Records.Current != null)
			{
				SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();
				docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(Records.Current.OrderNbr, Records.Current.OrderType);
				throw new PXRedirectRequiredException(docgraph, true, "Order") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public virtual void _(Events.RowUpdated<SOProcessFilter> e)
		{
			if (!e.Cache.ObjectsEqual<SOProcessFilter.action, SOProcessFilter.definePrinterManually, SOProcessFilter.printWithDeviceHub>(e.Row, e.OldRow) &&
				PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && Filter.Current != null && Filter.Current.PrintWithDeviceHub == true && Filter.Current.DefinePrinterManually == true
				&& (PXContext.GetSlot<PX.SM.AUSchedule>() == null || !(Filter.Current.PrinterID != null && e.OldRow.PrinterID == null)))
			{
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(SONotificationSource.Customer, SOReports.PrintSalesOrder, Accessinfo.BranchID);
			}
		}

		public virtual void _(Events.FieldVerifying<SOProcessFilter, SOProcessFilter.printerID> e)
		{
			if (e.Row != null && !IsPrintingAllowed(e.Row))
				e.NewValue = null;
		}

		public class WellKnownActions
		{
			public class SOOrderScreen
			{
				public const string ScreenID = "SO301000";

				public const string PrintSalesOrder
					= ScreenID + "$" + nameof(SOOrderEntry.printSalesOrder);

				public const string PrintQuote
					= ScreenID + "$" + nameof(SOOrderEntry.printQuote);

				public const string PrintBlanket
					= ScreenID + "$" + nameof(Blanket.printBlanket);

				public const string EmailSalesOrder
					= ScreenID + "$" + nameof(SOOrderEntry.emailSalesOrder);

				public const string EmailQuote
					= ScreenID + "$" + nameof(SOOrderEntry.emailQuote);

				public const string EmailBlanket
					= ScreenID + "$" + nameof(Blanket.emailBlanket);


				public class printSalesOrder : PX.Data.BQL.BqlString.Constant<printSalesOrder>
				{
					public printSalesOrder() : base(PrintSalesOrder) {; }
				}

				public class printQuote : PX.Data.BQL.BqlString.Constant<printQuote>
				{
					public printQuote() : base(PrintQuote) {; }
				}

				public class printBlanket : PX.Data.BQL.BqlString.Constant<printBlanket>
				{
					public printBlanket() : base(PrintBlanket) {; }
				}

				public class emailSalesOrder : PX.Data.BQL.BqlString.Constant<emailSalesOrder>
				{
					public emailSalesOrder() : base(EmailSalesOrder) {; }
				}

				public class emailQuote : PX.Data.BQL.BqlString.Constant<emailQuote>
				{
					public emailQuote() : base(EmailQuote) {; }
				}

				public class emailBlanket : PX.Data.BQL.BqlString.Constant<emailBlanket>
				{
					public emailBlanket() : base(EmailBlanket) {; }
				}
			}
		}
	}

	[Serializable()]
	public partial class SOProcessFilter : IBqlTable, PX.SM.IPrintable
	{
		#region Action
		[PX.Data.Automation.PXWorkflowMassProcessing(DisplayName = "Action")]
		public virtual string Action { get; set; }
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
		#endregion
		#region CurrentOwnerID
		[PXDBInt]
		[CRCurrentOwnerID]
		public virtual int? CurrentOwnerID { get; set; }
		public abstract class currentOwnerID : PX.Data.BQL.BqlInt.Field<currentOwnerID> { }
		#endregion
		#region MyOwner
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner { get; set; }
		public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
		#endregion
		#region OwnerID
		[PX.TM.SubordinateOwner(DisplayName = "Assigned To")]
		public virtual int? OwnerID
		{
			get => (MyOwner == true) ? CurrentOwnerID : _OwnerID;
			set => _OwnerID = value;
		}
		protected int? _OwnerID;
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		#endregion
		#region WorkGroupID
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, IsWorkgroupOrSubgroupOfContact<Current<AccessInfo.contactID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get => (MyWorkGroup == true) ? null : _WorkGroupID;
			set => _WorkGroupID = value;
		}
		protected Int32? _WorkGroupID;
		public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
		#endregion
		#region MyWorkGroup
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup { get; set; }
		public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
		#endregion
		#region FilterSet
		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? FilterSet => 
			this.OwnerID != null ||
			this.WorkGroupID != null ||
			this.MyWorkGroup == true;
		public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }
		#endregion
		#region StartDate
		[PXDBDate]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion
		#region EndDate
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? EndDate { get; set; }
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		#endregion
		#region ShowAll
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show All")]
		public virtual Boolean? ShowAll { get; set; }
		public abstract class showAll : PX.Data.BQL.BqlBool.Field<showAll> { }
		#endregion

		#region OrderType
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXDefault(typeof(
			Search<SOOrderType.orderType,
			Where<
				SOOrderType.active.IsEqual<True>.
				And<SOOrderType.behavior.IsEqual<SOBehavior.qT>>>,
			OrderBy<				
				Desc<TestIf<SOOrderType.orderType.IsEqual<SOBehavior.qT>>>
			>>))]
		[PXSelector(typeof(Search<SOOrderType.orderType, Where<SOOrderType.active, Equal<True>>>))]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OrderType { get; set; }
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		#endregion
		#region CustomerID
		[Customer]
		public virtual Int32? CustomerID { get; set; }
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
		#region Status
		[PXDBString(1, IsFixed = true)]
		[SOOrderStatus.List]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Status { get; set; }
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion
		#region SalesPersonID
		[SalesPerson]
		public virtual Int32? SalesPersonID { get; set; }
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		#endregion

		#region PrintWithDeviceHub
		[PXDBBool]
		[PXDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
		[PXUIField(DisplayName = "Print with DeviceHub")]
		public virtual bool? PrintWithDeviceHub { get; set; }
		public abstract class printWithDeviceHub : PX.Data.BQL.BqlBool.Field<printWithDeviceHub> { }
		#endregion
		#region DefinePrinterManually
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Define Printer Manually")]
		public virtual bool? DefinePrinterManually { get; set; } = false;
		public abstract class definePrinterManually : PX.Data.BQL.BqlBool.Field<definePrinterManually> { }
		#endregion
		#region PrinterID
		[PX.SM.PXPrinterSelector]
		[PXFormula(typeof(
			Null.When<
				printWithDeviceHub.IsNotEqual<True>.
				Or<definePrinterManually.IsNotEqual<True>>>.
			Else<printerID>))]
		public virtual Guid? PrinterID { get; set; }
		public abstract class printerID : PX.Data.BQL.BqlGuid.Field<printerID> { }
		#endregion
		#region NumberOfCopies
		[PXDBInt(MinValue = 1)]
		[PXDefault(1)]
		[PXFormula(typeof(Selector<SOProcessFilter.printerID, PX.SM.SMPrinter.defaultNumberOfCopies>))]
		[PXUIField(DisplayName = "Number of Copies", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? NumberOfCopies { get; set; }
		public abstract class numberOfCopies : PX.Data.BQL.BqlInt.Field<numberOfCopies> { }
		#endregion
	}

	[TM.OwnedFilter.Projection(typeof(SOProcessFilter), typeof(workgroupID), typeof(ownerID))]
    [Serializable]
	public partial class SOOrderProcessSelected : SOOrder
	{
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		public new abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
	}

	public class SOEmailProcessing :
		PXFilteredProcessingJoin<SOOrderProcessSelected, SOProcessFilter,
		LeftJoinSingleTable<Customer, On<SOOrder.customerID.IsEqual<Customer.bAccountID>>>,
		Where<Brackets<SOOrderProcessSelected.behavior.IsEqual<SOBehavior.tR>.
					   Or<Match<Customer, AccessInfo.userName.FromCurrent>>>.
			And<SOOrderProcessSelected.orderDate.IsLessEqual<SOProcessFilter.endDate.FromCurrent>>.
			And<SOProcessFilter.startDate.FromCurrent.IsNull.
				Or<SOOrderProcessSelected.orderDate.IsGreaterEqual<SOProcessFilter.startDate.FromCurrent>>>.
			And<PX.SM.WorkflowAction.IsEnabled<SOOrderProcessSelected, SOProcessFilter.action>>.
			And<SOProcessFilter.showAll.FromCurrent.IsEqual<True>.
				Or<SOProcessFilter.action.FromCurrent.IsIn<
						SOOrderProcess.WellKnownActions.SOOrderScreen.printSalesOrder,
						SOOrderProcess.WellKnownActions.SOOrderScreen.printQuote,
						SOOrderProcess.WellKnownActions.SOOrderScreen.printBlanket>.
						And<SOOrderProcessSelected.printed.IsEqual<False>>>.
				Or<SOProcessFilter.action.FromCurrent.IsIn<
					SOOrderProcess.WellKnownActions.SOOrderScreen.emailSalesOrder,
					SOOrderProcess.WellKnownActions.SOOrderScreen.emailQuote,
					SOOrderProcess.WellKnownActions.SOOrderScreen.emailBlanket>.
					And<SOOrderProcessSelected.emailed.IsEqual<False>>>>>>
	{
		public SOEmailProcessing(PXGraph graph)
			: base(graph)
		{
			_OuterView.WhereAndCurrent<SOProcessFilter>(nameof(SOOrderProcessSelected.ownerID));
		}
		public SOEmailProcessing(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			_OuterView.WhereAndCurrent<SOProcessFilter>(nameof(SOOrderProcessSelected.ownerID));
		}
	}
}

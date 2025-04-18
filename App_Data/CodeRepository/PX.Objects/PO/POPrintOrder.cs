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
using System.Text;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.TM;
using PX.Objects.AP.MigrationMode;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Common;
using PX.SM;

namespace PX.Objects.PO
{
	[System.SerializableAttribute()]
	public partial class POPrintOrderFilter : IBqlTable, PX.SM.IPrintable
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
		[PX.Data.Automation.PXWorkflowMassProcessing(DisplayName = "Action")]
		public virtual string Action
		{
			get;
			set;
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
		[PXFormula(typeof(Selector<POPrintOrderFilter.printerID, PX.SM.SMPrinter.defaultNumberOfCopies>))]
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

	public class actionEmpty : PX.Data.BQL.BqlString.Constant<actionEmpty>
	{
		public actionEmpty():base("<SELECT>"){}
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class POPrintOrder : PXGraph<POPrintOrder>
	{
		public PXFilter<POPrintOrderFilter> Filter;

        public PXSelect<Vendor> vendors;
        public PXSelect<EPEmployee> employees;

        [Serializable]
		[PXProjection(typeof(Select5<POOrder, 
			LeftJoin<EPEmployee, On<EPEmployee.defContactID, Equal<POOrder.ownerID>>>,			
			Where2<
						Where<CurrentValue<POPrintOrderFilter.ownerID>, IsNull,
						   Or<CurrentValue<POPrintOrderFilter.ownerID>, Equal<EPEmployee.defContactID>>>,
					And2<
						Where<CurrentValue<POPrintOrderFilter.workGroupID>, IsNull,
						   Or<CurrentValue<POPrintOrderFilter.workGroupID>, Equal<POOrder.ownerWorkgroupID>>>,
					And2<
						Where<CurrentValue<POPrintOrderFilter.myWorkGroup>, Equal<CS.boolFalse>,
							 Or<POOrder.ownerWorkgroupID, IsWorkgroupOfContact<CurrentValue<POPrintOrderFilter.currentOwnerID>>>>,
					And<
						Where<POOrder.ownerWorkgroupID, IsNull,
							 Or<POOrder.ownerWorkgroupID, IsWorkgroupOrSubgroupOfContact<CurrentValue<POPrintOrderFilter.currentOwnerID>>>>>>>>,
					Aggregate<GroupBy<POOrder.orderNbr,
							GroupBy<POOrder.hold,
										GroupBy<POOrder.approved,
										GroupBy<POOrder.emailed,
										GroupBy<POOrder.dontEmail,
										GroupBy<POOrder.cancelled,
										GroupBy<POOrder.isUnbilledTaxValid,
										GroupBy<POOrder.isTaxValid,
										GroupBy<POOrder.ownerWorkgroupID,
										GroupBy<POOrder.createdByID,
										GroupBy<POOrder.lastModifiedByID,
										GroupBy<POOrder.dontPrint,
										GroupBy<POOrder.noteID,
										GroupBy<POOrder.printed>>>>>>>>>>>>>>>>))]
		public partial class POPrintOrderOwned : POOrder
		{
			public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			public new abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		}

		public PXCancel<POPrintOrderFilter> Cancel;
		public PXAction<POPrintOrderFilter> details;
		[PXFilterable]
		public PXFilteredProcessingJoin<POPrintOrderOwned, POPrintOrderFilter,
			InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POPrintOrderOwned.vendorID>>,
 	        LeftJoin<EPEmployee, On<EPEmployee.defContactID, Equal<POPrintOrderOwned.ownerID>>>>,
			Where<PX.SM.WhereWorkflowActionEnabled<POPrintOrderOwned, POPrintOrderFilter.action>>> Records;

		public PXSetup<EPSetup> EPSetup;

		public POPrintOrder()
		{
			APSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			PXUIFieldAttribute.SetRequired<POPrintOrderOwned.orderDate>(this.Records.Cache, false);
			PXUIFieldAttribute.SetRequired<POPrintOrderOwned.curyID>(this.Records.Cache, false);
			PXUIFieldAttribute.SetEnabled(Records.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<POPrintOrderOwned.selected>(Records.Cache, null, true);

			Records.SetSelected<POPrintOrderOwned.selected>();
			Records.SetProcessCaption(IN.Messages.Process);
			Records.SetProcessAllCaption(IN.Messages.ProcessAll);
			//Records.SetProcessDelegate(PrintOrders);			
			this.Records.Cache.AllowInsert = false;
			this.Records.Cache.AllowDelete = false;			
		}

		public virtual IEnumerable records(PXAdapter adapter)
		{
			if (string.IsNullOrEmpty(Filter.Current.Action)
				|| Filter.Current.Action == PX.Data.Automation.PXWorkflowMassProcessingAttribute.Undefined)
				return Array<object>.Empty;

			var query = Records.View.BqlSelect;

			//we need to filter orders because of these actions are using for many states - not only related with Print and Email statuses
			if (Filter.Current.Action.Contains("email", StringComparison.InvariantCultureIgnoreCase))
				query = query.WhereAnd<Where<POPrintOrderOwned.hold, Equal<False>,
								 And<POPrintOrderOwned.dontEmail, Equal<False>,
								 And<POPrintOrderOwned.emailed, NotEqual<True>>>>>();
			else
				query = query.WhereAnd<Where<POPrintOrderOwned.hold, Equal<False>,
								 And<POPrintOrderOwned.dontPrint, Equal<False>,
								 And<POPrintOrderOwned.printed, NotEqual<True>>>>>();

			PXView view = new PXView(this, false, query);
			return view.SelectMulti();
		}
			
		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXEditDetailButton]
		public virtual IEnumerable Details(PXAdapter adapter)
		{
			if (Records.Current != null && Filter.Current != null)
			{
				POOrderEntry graph = PXGraph.CreateInstance<POOrderEntry>();
				graph.Document.Current = Records.Current;
				throw new PXRedirectRequiredException(graph, true, Messages.ViewPOOrder) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		protected virtual void POPrintOrderFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			POPrintOrderFilter filter = (POPrintOrderFilter)e.Row;
			PXUIFieldAttribute.SetEnabled<POPrintOrderFilter.ownerID>(sender, filter, filter == null || filter.MyOwner == false);
			PXUIFieldAttribute.SetEnabled<POPrintOrderFilter.workGroupID>(sender, filter, filter == null || filter.MyWorkGroup == false);

			if (filter != null && !String.IsNullOrEmpty(filter.Action))
			{
				SetProcessTarget(filter);

				bool showPrintSettings = IsPrintingAllowed(filter);
				PXUIFieldAttribute.SetVisible<POPrintOrderFilter.printWithDeviceHub>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<POPrintOrderFilter.definePrinterManually>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<POPrintOrderFilter.printerID>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<POPrintOrderFilter.numberOfCopies>(sender, filter, showPrintSettings);

				if (PXContext.GetSlot<AUSchedule>() == null)
				{
					PXUIFieldAttribute.SetEnabled<POPrintOrderFilter.definePrinterManually>(sender, filter, filter.PrintWithDeviceHub == true);
					PXUIFieldAttribute.SetEnabled<POPrintOrderFilter.numberOfCopies>(sender, filter, filter.PrintWithDeviceHub == true);
					PXUIFieldAttribute.SetEnabled<POPrintOrderFilter.printerID>(sender, filter, filter.PrintWithDeviceHub == true && filter.DefinePrinterManually == true);
				}

				if (filter.PrintWithDeviceHub != true || filter.DefinePrinterManually != true)
				{
					filter.PrinterID = null;
				}
			}
		}

		public virtual bool IsPrintingAllowed(POPrintOrderFilter filter)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() 
				&& filter?.Action == WellKnownActions.POOrderScreen.PrintPurchaseOrder;
		}

		public virtual void POPrintOrderFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if ((!sender.ObjectsEqual<POPrintOrderFilter.action>(e.Row, e.OldRow) || !sender.ObjectsEqual<POPrintOrderFilter.definePrinterManually>(e.Row, e.OldRow) || !sender.ObjectsEqual<POPrintOrderFilter.printWithDeviceHub>(e.Row, e.OldRow)) 
				&& Filter.Current != null && PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && Filter.Current.PrintWithDeviceHub == true && Filter.Current.DefinePrinterManually == true
				&& (PXContext.GetSlot<AUSchedule>() == null || !(Filter.Current.PrinterID != null && ((POPrintOrderFilter)e.OldRow).PrinterID == null)))
			{
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(PONotificationSource.Vendor, POReports.PurchaseOrderReportID, Accessinfo.BranchID);
			}
		}

		protected virtual void POPrintOrderOwned_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.IsDirty = false;
		}

		protected virtual void POPrintOrderFilter_PrinterName_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POPrintOrderFilter row = (POPrintOrderFilter)e.Row;
			if (row != null)
			{
				if (!IsPrintingAllowed(row))
					e.NewValue = null;
			}
		}

		protected virtual void SetProcessTarget(POPrintOrderFilter orderFilter)
        {
			Dictionary<string, object> parameters = Filter.Cache.ToDictionary(orderFilter);
			Records.SetProcessWorkflowAction(orderFilter.Action, parameters);
		}

		public class WellKnownActions
		{
			public class POOrderScreen
			{
				public const string ScreenID = "PO301000";

				public const string PrintPurchaseOrder
					= ScreenID + "$" + nameof(POOrderEntry.printPurchaseOrder);

				public const string EmailPurchaseOrder
					= ScreenID + "$" + nameof(POOrderEntry.emailPurchaseOrder);

				public const string MarkAsDontPrint
					= ScreenID + "$" + nameof(POOrderEntry.markAsDontPrint);

				public const string MarkAsDontEmail
					= ScreenID + "$" + nameof(POOrderEntry.markAsDontEmail);
			}
		}
	}
}

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

using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.TM;

namespace PX.Objects.PM
{
	public class ProFormaProcess : PXGraph<ProFormaProcess>
	{
		public PXCancel<ProFormaFilter> Cancel;
		public PXFilter<ProFormaFilter> Filter;
		public PXAction<ProFormaFilter> viewDocumentRef;
		public PXAction<ProFormaFilter> viewDocumentProject;

		[PXFilterable]
		public PXFilteredProcessing<PMProforma, ProFormaFilter> Items; 

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocumentRef(PXAdapter adapter)
		{
			var proforma = Items.Current;
			if (proforma == null)
				return adapter.Get();

			var proformaGraph = CreateInstance<ProformaEntry>();
			PMProforma validProforma = proformaGraph.Document.Search<PMProforma.refNbr>(
				proforma.RefNbr,
				proforma.ARInvoiceDocType);

			if (validProforma == null)
			{
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExistOrNoRights, proforma.RefNbr));
			}

			proformaGraph.Document.Current = validProforma;
			throw new PXRedirectRequiredException(proformaGraph, true, "Order")
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocumentProject(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				var service = CreateInstance<ProjectAccountingService>();
				service.NavigateToProjectScreen(Items.Current.ProjectID, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public virtual IEnumerable items()
		{
			ProFormaFilter filter = Filter.Current;

			PXSelectBase<PMProforma> cmd =
				new PXSelectJoin<PMProforma,
				InnerJoinSingleTable<Customer, On<PMProforma.customerID, Equal<Customer.bAccountID>>>,
				Where<PMProforma.hold, Equal<False>,
					And<PMProforma.released, Equal<False>,
					And<PMProforma.status, Equal<ProformaStatus.open>,
					And<Match<Customer, Current<AccessInfo.userName>>>>>>>(this);

			cmd.WhereAnd<Where<PMProforma.invoiceDate, LessEqual<Current<ProFormaFilter.endDate>>>>();

			if (filter.BeginDate != null)
			{
				cmd.WhereAnd<Where<PMProforma.invoiceDate, GreaterEqual<Current<ProFormaFilter.beginDate>>>>();	
			}

			if (filter.OwnerID != null)
			{
				cmd.WhereAnd<Where<PMProforma.ownerID, Equal<Current<ProFormaFilter.currentOwnerID>>>>();
			}

			if (filter.WorkGroupID != null)
			{
				cmd.WhereAnd<Where<PMProforma.workgroupID, Equal<Current<ProFormaFilter.workGroupID>>>>();
			}

			if (Filter.Current.ShowAll == true)
			{
				cmd.WhereAnd<Where<PMProforma.hold, Equal <False>>> ();
			}

			int startRow = PXView.StartRow;
			int totalRows = 0;

			foreach (PXResult<PMProforma> res in cmd.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				PMProforma item = res;

				PMProforma cached = (PMProforma)Items.Cache.Locate(item);
				if (cached != null)
					item.Selected = cached.Selected;
				yield return item;

				PXView.StartRow = 0;
			}
		}

		public ProFormaProcess()
		{
			Items.SetProcessCaption(PM.Messages.Process);
			Items.SetProcessAllCaption(PM.Messages.ProcessAll);

			PXUIFieldAttribute.SetVisible<PMProforma.curyID>(Items.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
		}

		public virtual void ProFormaFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{

			ProFormaFilter filter = e.Row as ProFormaFilter;

			Items.SetProcessDelegate<ProformaEntry>(
				delegate (ProformaEntry engine, PMProforma item)
				{
					try
					{
						engine.Clear();
						engine.Document.Current = item;
						PMProforma.Events.Select(ev => ev.Release).FireOn(engine, item);
						engine.ReleaseDocument(item);
					}
					catch (Exception ex)
					{
						throw new PXSetPropertyException(ex.Message, PXErrorLevel.RowError);
					}
				});
		}

		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class ProFormaFilter : IBqlTable
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
			#region MyOwner
			public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
			protected Boolean? _MyOwner;
			[PXDBBool()]
			[PXUIField(DisplayName = "Me")]
			[PXDefault(false)]
			public virtual Boolean? MyOwner
			{
				get
				{
					return this._MyOwner;
				}
				set
				{
					this._MyOwner = value;
				}
			}
			#endregion
			#region MyWorkGroup
			public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
			protected Boolean? _MyWorkGroup;
			[PXDBBool()]
			[PXUIField(DisplayName = "My")]
			[PXDefault(false)]
			public virtual Boolean? MyWorkGroup
			{
				get
				{
					return this._MyWorkGroup;
				}
				set
				{
					this._MyWorkGroup = value;
				}
			}
			#endregion
			#region ShowAll
			public abstract class showAll : PX.Data.BQL.BqlBool.Field<showAll> { }
			protected Boolean? _ShowAll;
			[PXDBBool()]
			[PXUIField(DisplayName = "Show All")]
			[PXDefault(false)]
			public virtual Boolean? ShowAll
			{
				get
				{
					return this._ShowAll;
				}
				set
				{
					this._ShowAll = value;
				}
			}
			#endregion
			#region BeginDate
			public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }
			protected DateTime? _BeginDate;
			[PXDate()]
			[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
			[PXUnboundDefault]
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
			[PXDBDate()]
			[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
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
		}
	}
}

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
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL.DAC;
using PX.Objects.GL.Attributes;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System.Linq;

namespace PX.Objects.FA.Standalone
{
	public class FABookBalance : IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		[PXDBInt(IsKey = true)]
		public virtual int? AssetID
		{
			get;
			set;
		}
		#endregion
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		[PXDBInt(IsKey = true)]
		public virtual int? BookID
		{
			get;
			set;
		}
		#endregion
		#region LastDeprPeriod
		public abstract class lastDeprPeriod : PX.Data.BQL.BqlString.Field<lastDeprPeriod> { }
		[PXDBString(6, IsFixed = true, InputMask = "")]
		public virtual string LastDeprPeriod
		{
			get;
			set;
		}
		#endregion
		#region CurrDeprPeriod
		public abstract class currDeprPeriod : PX.Data.BQL.BqlString.Field<currDeprPeriod> { }
		[PXDBString(6, IsFixed = true)]
		public virtual string CurrDeprPeriod
		{
			get;
			set;
		}
		#endregion
	}
}

namespace PX.Objects.FA
{
	[PXProjection(
		typeof(Select2<FADetails,
			LeftJoin<FABookBalanceUpdateGL,
				On<FABookBalanceUpdateGL.assetID, Equal<FADetails.assetID>>,
			LeftJoin<FABookBalanceNonUpdateGL,
				On<FABookBalanceNonUpdateGL.assetID, Equal<FADetails.assetID>>,
			LeftJoin<FABookBalanceTransfer,
				On<FABookBalanceTransfer.assetID, Equal<FADetails.assetID>,
					And<FABookBalanceTransfer.bookID, Equal<IsNull<FABookBalanceUpdateGL.bookID, FABookBalanceNonUpdateGL.bookID>>>>>>>>),
		new Type[] { typeof(FADetails) })]
	[PXCacheName(Messages.FADetails)]
	public class FADetailsTransfer : FADetails
	{
		#region AssetID
		public new abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		/// <summary>
		/// A reference to <see cref="FixedAsset"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the fixed asset. 
		/// It is a required value. 
		/// By default, the value is set to the current fixed asset identifier.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public override int? AssetID { get; set; }
		#endregion
		#region TransferPeriodID
		public abstract class transferPeriodID : PX.Data.BQL.BqlString.Field<transferPeriodID> { }
		/// <summary>
		/// The identifier of the transfer period.
		/// This is an unbound service field that is used to pass the parameter to transfer processing.
		/// </summary>
		[PXDBString(6, IsFixed = true, BqlField = typeof(FABookBalanceTransfer.transferPeriodID))]
		[PXUIField(DisplayName = "Transfer Period", Enabled = false)]
		[FinPeriodIDFormatting]
		public virtual string TransferPeriodID { get; set; }
		#endregion
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		public new abstract class locationRevID : PX.Data.BQL.BqlInt.Field<locationRevID> { }
		#region CurrentCost
		public new abstract class currentCost : PX.Data.BQL.BqlDecimal.Field<currentCost> { }
		/// <summary>
		/// The cost of the fixed asset in the current depreciation period.
		/// </summary>
		/// <value>
		/// The value is read-only and is selected from the appropriate <see cref="FABookHistoryRecon.ytdAcquired"/> field.
		/// </value>
		[CM.PXDBBaseCury(BqlField = typeof(FADetails.currentCost))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Basis", Enabled = false)]
		public override Decimal? CurrentCost
		{
			get;
			set;
		}
		#endregion
		#region AccrualBalance
		public new abstract class accrualBalance : PX.Data.BQL.BqlDecimal.Field<accrualBalance> { }
		/// <summary>
		/// The reconciled part of the current cost of the fixed asset.
		/// </summary>
		/// <value>
		/// The value is read-only and is selected from the appropriate <see cref="FABookHistoryRecon.ytdReconciled"/> field.
		/// </value>
		[PXDBBaseCury(BqlField = typeof(FADetails.accrualBalance))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public override Decimal? AccrualBalance
		{
			get;
			set;
		}
		#endregion

		public new abstract class receiptDate : BqlDateTime.Field<receiptDate> { }
		public new abstract class tagNbr : BqlString.Field<tagNbr> { }
	}

	public class TransferProcess : PXGraph<TransferProcess>
	{
		public PXCancel<TransferFilter> Cancel;
		public PXFilter<TransferFilter> Filter;
		[PXFilterable]
		[PX.SM.PXViewDetailsButton(typeof(FixedAsset.assetCD), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
		public PXFilteredProcessingJoin<FixedAsset, TransferFilter, 
			LeftJoin<FADetailsTransfer, 
				On<FixedAsset.assetID, Equal<FADetailsTransfer.assetID>>,
			InnerJoin<Branch, On<FixedAsset.branchID, Equal<Branch.branchID>>,
			LeftJoin<Account, 
				On<FixedAsset.fAAccountID, Equal<Account.accountID>>, 
			LeftJoin<FALocationHistory, 
				On<FALocationHistory.assetID, Equal<FADetailsTransfer.assetID>, 
					And<FALocationHistory.revisionID, Equal<FADetailsTransfer.locationRevID>>>>>>>> Assets;

		public PXSelect<BAccount> Baccount;
		public PXSelect<Vendor> Vendor;
		public PXSelect<EPEmployee> Employee;
		public PXSelect<FixedAsset> AssetSelect;
		public PXSelect<FADetails> Details;
		public PXSelect<FABookBalance> BookBalance;
		public PXSelect<FARegister> Register;
		public PXSelect<FATran> FATransactions;
		public PXSelect<FALocationHistory> Lochist;
 
		public PXSetup<FASetup> fasetup; 

		public TransferProcess()
		{
			FASetup setup = fasetup.Current;

			if (fasetup.Current.UpdateGL != true)
			{
				throw new PXSetupNotEnteredException<FASetup>(Messages.OperationNotWorkInInitMode, PXUIFieldAttribute.GetDisplayName<FASetup.updateGL>(fasetup.Cache));
			}

			PXUIFieldAttribute.SetDisplayName<Account.accountClassID>(Caches[typeof(Account)], Messages.FixedAssetsAccountClass);

			if (fasetup.Current.AutoReleaseTransfer != true)
			{
				Assets.SetProcessCaption(Messages.Prepare);
				Assets.SetProcessAllCaption(Messages.PrepareAll);
			}
		}

		public override void Clear(PXClearOption option)
		{
			if (this.Caches.ContainsKey(typeof(FADetails)))
			{
				this.Caches[typeof(FADetails)].ClearQueryCache();
			}

			base.Clear(option);
		}

		protected virtual void FALocationHistory_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			AssetMaint.LiveUpdateMaskedSubs(this, Assets.Cache, (FALocationHistory)e.Row);
		}

		public static void Transfer(TransferFilter filter, List<FixedAsset> list)
		{
			TransferProcess graph = PXGraph.CreateInstance<TransferProcess>();
			graph.DoTransfer(filter, list);
		}

		protected virtual void DoTransfer(TransferFilter filter, List<FixedAsset> list)
		{
			DocumentList<FARegister> created = new DocumentList<FARegister>(this);

			foreach (FixedAsset asset in list)
			{
				try
				{
					this.Caches[typeof(FixedAsset)].Current = asset;
					FADetails det = PXSelect<FADetails, Where<FADetails.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectSingleBound(this, new object[] { asset });
					FALocationHistory location = PXSelect<FALocationHistory, Where<FALocationHistory.assetID, Equal<Current<FADetails.assetID>>, And<FALocationHistory.revisionID, Equal<Current<FADetails.locationRevID>>>>>.SelectSingleBound(this, new object[] { det });
					int? destClassID = filter.ClassTo ?? asset.ClassID;
					FAClass destClass = SelectFrom<FAClass>.Where<FAClass.assetID.IsEqual<@P.AsInt>>.View.Select(this, destClassID);
					int? destBranchID = filter.BranchTo ?? location.LocationID;
					string destDeptID = string.IsNullOrEmpty(filter.DepartmentTo) ? location.Department : filter.DepartmentTo;

					if (location.LocationID != destBranchID || location.Department != destDeptID || asset.ClassID != destClassID)
					{
						FADetails copy_det = (FADetails)Details.Cache.CreateCopy(det);
						FALocationHistory copy_loc = (FALocationHistory)Lochist.Cache.CreateCopy(location);

						copy_loc.RevisionID = ++copy_det.LocationRevID;
						copy_loc.ClassID = destClassID;

					if (copy_loc.LocationID != destBranchID)
					{
						copy_loc.BuildingID = null;
					}
						copy_loc.LocationID = destBranchID;
						copy_loc.Department = destDeptID;
						copy_loc.PeriodID = filter.PeriodID;
						copy_loc.TransactionDate = filter.TransferDate;
						copy_loc.Reason = filter.Reason;

						if (destClass.UnderConstruction == true)
						{
							if (det.DepreciateFromDate == null)
							{
								throw new PXSetPropertyException(PX.Data.ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<FADetails.depreciateFromDate>(Details.Cache));
							}
						}

						TransactionEntry.SegregateRegister(this, (int)destBranchID, FARegister.origin.Transfer, null, filter.TransferDate, "", created);

						Details.Update(copy_det);
						location = Lochist.Insert(copy_loc);

						if (asset.ClassID != destClassID)
						{
							bool hasBeenUnderConstruction = asset.UnderConstruction == true;
							asset.ClassID = destClassID;
							asset.UnderConstruction = destClass.UnderConstruction;

							if (hasBeenUnderConstruction && asset.UnderConstruction != true)
							{
								asset.AccumulatedDepreciationAccountID = destClass.AccumulatedDepreciationAccountID;
								asset.AccumulatedDepreciationSubID = destClass.AccumulatedDepreciationSubID;
								asset.DepreciatedExpenseAccountID = destClass.DepreciatedExpenseAccountID;
								asset.DepreciatedExpenseSubID = destClass.DepreciatedExpenseSubID;

								UpdateBookBalances(asset);
							}

							AssetSelect.Cache.Update(asset);
						}

						FARegister reg = Register.Current;
						AssetProcess.TransferAsset(this, asset, location, ref reg);
					}
				}
				catch(Exception e)
				{
					PXProcessing.SetError(list.IndexOf(asset), e);
					throw;
				}
			}
			if (Register.Current != null && created.Find(Register.Current) == null)
			{
				created.Add(Register.Current);
			}
			Actions.PressSave();
			if (fasetup.Current.AutoReleaseTransfer == true)
			{
				SelectTimeStamp();
				PXLongOperation.StartOperation(this, delegate{ AssetTranRelease.ReleaseDoc(created, false); });
			}
			else if (created.Count > 0)
			{
				AssetTranRelease graph = CreateInstance<AssetTranRelease>();
				AssetTranRelease.ReleaseFilter fltr = (AssetTranRelease.ReleaseFilter)graph.Filter.Cache.CreateCopy(graph.Filter.Current);
				fltr.Origin = FARegister.origin.Transfer;
				graph.Filter.Update(fltr);
				graph.SelectTimeStamp();

				Dictionary<string, string> parameters = new Dictionary<string, string>();

				for (int i = 0; i < created.Count; ++i)
				{
					FARegister reg = created[i];
				reg.Selected = true;
				graph.FADocumentList.Update(reg);
				graph.FADocumentList.Cache.SetStatus(reg, PXEntryStatus.Updated);
				graph.FADocumentList.Cache.IsDirty = false;

					parameters["FARegister.RefNbr" + i] = reg.RefNbr;
				}

				Organization organization = PXSelectReadonly<Organization,
					Where<Organization.organizationID, Equal<Required<Organization.organizationID>>>>.Select(this, filter.OrganizationID);

				parameters["OrgBAccountID"] = organization?.OrganizationCD;
				parameters["PeriodFrom"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.PeriodID);
				parameters["PeriodTo"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.PeriodID);
				parameters["Mode"] = "U";

				PXReportRequiredException reportex = new PXReportRequiredException(parameters, "FA642000", "Preview");
				throw new PXRedirectWithReportException(graph, reportex, "Release FA Transaction");
			}
		}

		private void UpdateBookBalances(FixedAsset asset)
		{
			foreach(PXResult<FABookSettings, FABookBalance> result in SelectFrom<FABookSettings>
				.LeftJoin<FABookBalance>
					.On<FABookBalance.bookID.IsEqual<FABookSettings.bookID>>
				.Where<FABookSettings.assetID.IsEqual<@P.AsInt>
					.And<FABookBalance.assetID.IsEqual<@P.AsInt>>>
				.View.Select(this, asset.ClassID, asset.AssetID))
			{
				FABookSettings settings = result;
				FABookBalance balance = result;

				if (balance?.AssetID != null && balance.DepreciationMethodID == null && balance.AveragingConvention == null)
				{
					balance.DepreciationMethodID = AssetMaint.GetDeprMethodOfAssetType(this, settings.DepreciationMethodID, balance);
					balance.AveragingConvention = settings.AveragingConvention;
					BookBalance.Update(balance);
				}
			}
		}

		public virtual IEnumerable assets()
		{
			TransferFilter filter = Filter.Current;
			PXSelectBase<FixedAsset> cmd = new PXSelectJoin<FixedAsset, 
				InnerJoin<FADetailsTransfer, 
					On<FixedAsset.assetID, Equal<FADetailsTransfer.assetID>>, 
				InnerJoin<Branch, On<FixedAsset.branchID, Equal<Branch.branchID>>,
				LeftJoin<Account, 
					On<FixedAsset.fAAccountID, Equal<Account.accountID>>, 
				LeftJoin<FALocationHistory, 
					On<FALocationHistory.assetID, Equal<FADetailsTransfer.assetID>, 
						And<FALocationHistory.revisionID, Equal<FADetailsTransfer.locationRevID>>>>>>>, 
				Where<FADetailsTransfer.status, NotEqual<FixedAssetStatus.hold>, 
					And<FADetailsTransfer.status, NotEqual<FixedAssetStatus.disposed>,
					And<FADetailsTransfer.status, NotEqual<FixedAssetStatus.reversed>>>>>(this);
			if(filter.PeriodID != null)
			{
				cmd.WhereAnd<Where<FADetailsTransfer.transferPeriodID, IsNull, Or<FADetailsTransfer.transferPeriodID, LessEqual<Current<TransferFilter.periodID>>>>>();
			}
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>() || filter.OrganizationID != null)
			{
				cmd.WhereAnd<Where<Branch.organizationID, Equal<Current<TransferFilter.organizationID>>>>();
			}
			if (filter.BranchFrom != null)
			{
				cmd.WhereAnd<Where<FixedAsset.branchID, Equal<Current<TransferFilter.branchFrom>>>>();
			}
			if(filter.DepartmentFrom != null)
			{
				cmd.WhereAnd<Where<FALocationHistory.department, Equal<Current<TransferFilter.departmentFrom>>>>();
			}
			if (filter.ClassFrom != null)
			{
				cmd.WhereAnd<Where<FixedAsset.classID, Equal<Current<TransferFilter.classFrom>>>>();
			}

			int startRow = PXView.StartRow;
			int totalRows = 0;
			List<PXFilterRow> newFilters = new List<PXFilterRow>();
			foreach (PXFilterRow f in PXView.Filters)
			{
				if (f.DataField.ToLower() == "status")
				{
					f.DataField = "FADetailsTransfer__Status";
				}
				newFilters.Add(f);
			}
			List<object> list = cmd.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, newFilters.ToArray(), ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		protected virtual void FixedAsset_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FixedAsset asset = (FixedAsset)e.Row;

			// AssetID can be <0 when the datasource inserts a temporary record on redirect from selector
			if (asset == null || asset.AssetID < 0) return;

			FADetailsTransfer det = PXSelect<FADetailsTransfer, Where<FADetailsTransfer.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectSingleBound(this, new object[] { asset });
			string existingError = PXUIFieldAttribute.GetErrorOnly<FixedAsset.selected>(sender, asset);
			if (string.IsNullOrEmpty(existingError))
			{
				PXUIFieldAttribute.SetEnabled<FixedAsset.selected>(sender, asset, true);
				sender.RaiseExceptionHandling<FixedAsset.selected>(asset, null, null);

				try
				{
					AssetProcess.ThrowDisabled_Transfer(this, Filter.Current, asset, det);
				}
				catch (PXException exc)
				{
					// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [we need to unselect such assets to not to process them, the checkbox is disabled and it is more user-friendly, when it is unselected with notification]
					sender.SetValue<FixedAsset.selected>(asset, false);
					PXUIFieldAttribute.SetEnabled<FixedAsset.selected>(sender, asset, false);
					sender.RaiseExceptionHandling<FixedAsset.selected>(asset, null, new PXSetPropertyException(exc.MessageNoNumber, PXErrorLevel.RowWarning));
				}
			}

			if (string.IsNullOrEmpty(det.TransferPeriodID) && asset.UnderConstruction != true)
			{
				PXUIFieldAttribute.SetEnabled<FixedAsset.selected>(sender, asset, false);
				sender.RaiseExceptionHandling<FADetailsTransfer.transferPeriodID>(asset, null, new PXSetPropertyException(Messages.NextPeriodNotGenerated));
			}
		}

		protected virtual void _(Events.FieldUpdated<TransferFilter.organizationID> e)
		{
			TransferFilter filter = e.Row as TransferFilter;
			if (filter == null) return;

			e.Cache.SetDefaultExt<TransferFilter.branchFrom>(filter);
			e.Cache.SetDefaultExt<TransferFilter.branchTo>(filter);
		}

		protected virtual void _(Events.FieldDefaulting<TransferFilter.branchFrom> e)
		{
			TransferFilter filter = e.Row as TransferFilter;
			if (filter == null) return;

			e.NewValue = GetDefaultBrunchID(filter.OrganizationID);
			if (e.NewValue == null)
			{
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldDefaulting<TransferFilter.branchTo> e)
		{
			TransferFilter filter = e.Row as TransferFilter;
			if (filter == null) return;

			e.NewValue = GetDefaultBrunchID(filter.OrganizationID);
			if (e.NewValue == null)
			{
				e.Cancel = true;
			}
		}

		private int? GetDefaultBrunchID(int? OrganizationID)
		{
			int? result = null;
			int[] branchIDs = PXAccess.GetChildBranchIDs(OrganizationID, false);
			int? currentBranchID = this.Accessinfo.BranchID;

			if (branchIDs.Length == 1)
			{
				result = branchIDs[0];
			}
			else if (branchIDs.Length > 1 && OrganizationID == PXAccess.GetParentOrganizationID(currentBranchID))
			{
				result = currentBranchID;
			}

			return result;
		}

		public void TransferFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			TransferFilter filter = (TransferFilter)e.Row;
			if (filter == null) return;

			Assets.SetProcessDelegate(delegate(List<FixedAsset> list) { Transfer(filter, list); });

			bool areErrorsOnForm = PXUIFieldAttribute.GetErrors(sender, filter, PXErrorLevel.Error, PXErrorLevel.RowError).Any();
			bool isProcessEnabled = !areErrorsOnForm
				&& string.IsNullOrEmpty(filter.PeriodID) == false
				&& (filter.BranchTo != null || filter.DepartmentTo != null);

			Assets.SetProcessEnabled(isProcessEnabled);
			Assets.SetProcessAllEnabled(isProcessEnabled);
		}

		public void TransferFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row == null) return;

			if (!sender.ObjectsEqual<TransferFilter.branchFrom, TransferFilter.departmentFrom, TransferFilter.classFrom>(e.Row, e.OldRow))
			{
				// reset Selected flag for the assets, that are not in the current list (restricted by filter)
				foreach (FixedAsset fa in Assets.Cache.Cached.Cast<FixedAsset>().Where(_ => _.Selected == true))
				{
					Assets.Cache.SetValue<FixedAsset.selected>(fa, false);
				}

				this.Caches<FixedAsset>().Clear();
				Assets.Cache.Clear();
			}
		}

		[Serializable]
		public partial class TransferFilter : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : IBqlField { }

			[Organization(PersistingCheck = PXPersistingCheck.Nothing, FieldClass = "MULTICOMPANY")]
			[PXUIRequired(typeof(Where<FeatureInstalled<FeaturesSet.multipleCalendarsSupport>>))]
			public virtual int? OrganizationID
			{
				get;
				set;
			}
			#endregion
			#region TransferDate
			public abstract class transferDate : PX.Data.BQL.BqlDateTime.Field<transferDate> { }
			protected DateTime? _TransferDate;
			[PXDBDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Transfer Date")]
			public virtual DateTime? TransferDate
			{
				get
				{
					return _TransferDate;
				}
				set
				{
					_TransferDate = value;
				}
			}
			#endregion
			#region PeriodID
			public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
			protected String _PeriodID;
			[PXUIField(DisplayName = "Transfer Period")]
			[FABookPeriodOpenInGLSelector(
				dateType: typeof(TransferFilter.transferDate),
				organizationSourceType: typeof(TransferFilter.organizationID),
				isBookRequired: false)]
			public virtual String PeriodID
			{
				get
				{
					return _PeriodID;
				}
				set
				{
					_PeriodID = value;
				}
			}
			#endregion
			#region ClassFrom
			public abstract class classFrom : PX.Data.BQL.BqlInt.Field<classFrom> { }
			protected int? _ClassFrom;
			[PXDBInt]
			[PXSelector(
				typeof(Search<FAClass.assetID, Where<FAClass.recordType, Equal<FARecordType.classType>>>),
				typeof(FAClass.assetCD), 
				typeof(FAClass.assetTypeID), 
				typeof(FAClass.description), 
				typeof(FAClass.usefulLife),
				SubstituteKey = typeof(FAClass.assetCD),
				DescriptionField = typeof(FAClass.description), 
				CacheGlobal = true)]
			[PXUIField(DisplayName = "Asset Class")]
			public virtual int? ClassFrom
			{
				get
				{
					return _ClassFrom;
				}
				set
				{
					_ClassFrom = value;
				}
			}
			#endregion
			#region BranchFrom
			public abstract class branchFrom : PX.Data.BQL.BqlInt.Field<branchFrom> { }
			protected int? _BranchFrom;
			[BranchOfOrganization(
				organizationFieldType: typeof(TransferFilter.organizationID),
				onlyActive: true,
				featureFieldType: typeof(FeaturesSet.multipleCalendarsSupport),
				IsDetail = false,
				PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual int? BranchFrom
			{
				get
				{
					return _BranchFrom;
				}
				set
				{
					_BranchFrom = value;
				}
			}
			#endregion
			#region DepartmentFrom
			public abstract class departmentFrom : PX.Data.BQL.BqlString.Field<departmentFrom> { }
			protected string _DepartmentFrom;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
			[PXUIField(DisplayName = "Department")]
			public virtual string DepartmentFrom
			{
				get
				{
					return _DepartmentFrom;
				}
				set
				{
					_DepartmentFrom = value;
				}
			}
			#endregion
			#region ClassTo
			public abstract class classTo : PX.Data.BQL.BqlInt.Field<classTo> { }
			protected int? _ClassTo;
			[PXDBInt]
			[PXSelector(typeof(Search<FAClass.assetID, Where<FAClass.recordType, Equal<FARecordType.classType>>>),
						typeof(FAClass.assetCD), typeof(FAClass.assetTypeID), typeof(FAClass.description), typeof(FAClass.usefulLife),
						SubstituteKey = typeof(FAClass.assetCD),
						DescriptionField = typeof(FAClass.description), CacheGlobal = true)]
			[PXUIField(DisplayName = "Asset Class")]
			public virtual int? ClassTo
			{
				get
				{
					return _ClassTo;
				}
				set
				{
					_ClassTo = value;
				}
			}
			#endregion
			#region BranchTo
			public abstract class branchTo : PX.Data.BQL.BqlInt.Field<branchTo> { }
			protected int? _BranchTo;
			[BranchOfOrganization(
				organizationFieldType: typeof(TransferFilter.organizationID),
				onlyActive: true,
				featureFieldType: typeof(FeaturesSet.multipleCalendarsSupport),
				IsDetail = false,
				PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual int? BranchTo
			{
				get
				{
					return _BranchTo;
				}
				set
				{
					_BranchTo = value;
				}
			}
			#endregion
			#region DepartmentTo
			public abstract class departmentTo : PX.Data.BQL.BqlString.Field<departmentTo> { }
			protected string _DepartmentTo;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
			[PXUIField(DisplayName = "Department")]
			public virtual string DepartmentTo
			{
				get
				{
					return _DepartmentTo;
				}
				set
				{
					_DepartmentTo = value;
				}
			}
			#endregion
			#region Reason
			public abstract class reason : PX.Data.BQL.BqlString.Field<reason> { }
			protected String _Reason;
			[PXDBString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "Reason")]
			public virtual String Reason
			{
				get
				{
					return this._Reason;
				}
				set
				{
					this._Reason = value;
				}
			}
			#endregion
		}
	}
}

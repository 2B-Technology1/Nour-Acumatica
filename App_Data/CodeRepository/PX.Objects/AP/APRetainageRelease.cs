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

using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using System;
using PX.Objects.GL.Attributes;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AP.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.AP
{
	[Serializable]
	public partial class APRetainageFilter : IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(false)]
		public int? OrganizationID { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

	    [Branch(PersistingCheck = PXPersistingCheck.Nothing)]
	    public virtual Int32? BranchID { get; set; }
		#endregion
		#region OrgBAccountID
		public abstract class orgBAccountID : IBqlField { }

		[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
		[PXUIRequired(typeof(Where<FeatureInstalled<FeaturesSet.multipleBaseCurrencies>>))]
		public int? OrgBAccountID { get; set; }
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate { get; set; }
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[APOpenPeriod(typeof(APRetainageFilter.docDate),
			organizationSourceType: typeof(APRetainageFilter.organizationID),
			useMasterOrganizationIDByDefault: true,
			ValidatePeriod = PeriodValidation.DefaultSelectUpdate)]
		[PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FinPeriodID { get; set; }
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[VendorActive(
			Visibility = PXUIVisibility.SelectorVisible,
			Required = false,
			DescriptionField = typeof(Vendor.acctName))]
		public virtual int? VendorID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[APActiveProject]
		public virtual Int32? ProjectID { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[APInvoiceType.RefNbr(typeof(Search5<Standalone.APRegisterAlias.refNbr,
			InnerJoin<APInvoice, On<APInvoice.docType, Equal<Standalone.APRegisterAlias.docType>,
				And<APInvoice.refNbr, Equal<Standalone.APRegisterAlias.refNbr>>>,
			InnerJoinSingleTable<Vendor, On<Standalone.APRegisterAlias.vendorID, Equal<Vendor.bAccountID>>,
			LeftJoin<APTran, On<Standalone.APRegisterAlias.paymentsByLinesAllowed, Equal<True>,
				And<APTran.tranType, Equal<APInvoice.docType>,
				And<APTran.refNbr, Equal<APInvoice.refNbr>,
				And<APTran.curyRetainageBal, NotEqual<decimal0>,
				And<APTran.curyRetainageAmt, NotEqual<decimal0>>>>>>>>>,
			Where<Standalone.APRegisterAlias.retainageApply, Equal<True>,
				And<Standalone.APRegisterAlias.released, Equal<True>,
				And2<Where<
					Standalone.APRegisterAlias.paymentsByLinesAllowed, NotEqual<True>,
					And<Standalone.APRegisterAlias.curyRetainageUnreleasedAmt, Greater<decimal0>,
					And<Standalone.APRegisterAlias.curyRetainageTotal, Greater<decimal0>>>>,
				Or<
					Standalone.APRegisterAlias.paymentsByLinesAllowed, Equal<True>,
					And<APTran.refNbr, IsNotNull>>>>>,
			Aggregate<
				GroupBy<Standalone.APRegisterAlias.docType,
				GroupBy<Standalone.APRegisterAlias.refNbr>>>,
			OrderBy<Desc<Standalone.APRegisterAlias.refNbr>>
			>))]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.Visible)]
		public virtual string RefNbr { get; set; }
		#endregion
		#region ShowBillsWithOpenBalance
		public abstract class showBillsWithOpenBalance : PX.Data.BQL.BqlBool.Field<showBillsWithOpenBalance> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Lines with Open Balance", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ShowBillsWithOpenBalance { get; set; }
		#endregion
	}

	[Serializable]
	[PXProjection(typeof(Select2<APInvoice,
		InnerJoin<APRegister, On<APRegister.docType, Equal<APInvoice.docType>,
			And<APRegister.refNbr, Equal<APInvoice.refNbr>>>,
		LeftJoin<APTran, On<APRegister.paymentsByLinesAllowed, Equal<True>,
			And<APTran.tranType, Equal<APInvoice.docType>,
			And<APTran.refNbr, Equal<APInvoice.refNbr>,
			And<APTran.curyRetainageBal, NotEqual<decimal0>,
			And<APTran.curyRetainageAmt, NotEqual<decimal0>>>>>>>>,
		Where2<
			Where<CurrentValue<APRetainageFilter.vendorID>, IsNull, Or<APRegister.vendorID, Equal<CurrentValue<APRetainageFilter.vendorID>>>>,
			And2<Where<CurrentValue<APRetainageFilter.projectID>, IsNull, Or<APRegister.projectID, Equal<CurrentValue<APRetainageFilter.projectID>>>>,
			And2<Where<CurrentValue<APRetainageFilter.showBillsWithOpenBalance>, Equal<True>,
				Or<Where<APRegister.curyDocBal, Equal<decimal0>,
				And<CurrentValue<APRetainageFilter.showBillsWithOpenBalance>, NotEqual<True>>>>>,
			And<APRegister.retainageApply, Equal<True>,
			And<APRegister.released, Equal<True>,
			And<APRegister.docDate, LessEqual<CurrentValue<APRetainageFilter.docDate>>,
			And2<Where<
				APRegister.refNbr, Equal<CurrentValue<APRetainageFilter.refNbr>>,
					Or<CurrentValue<APRetainageFilter.refNbr>, IsNull>>,
			And<Where<
				APRegister.paymentsByLinesAllowed, NotEqual<True>,
				And<APRegister.curyRetainageUnreleasedAmt, Greater<decimal0>,
					Or<APTran.refNbr, IsNotNull>>>>>>>>>>>,
		OrderBy<Asc<APRegister.refNbr>>>))]
	public partial class APInvoiceExt : APInvoice
	{
		#region Key fields

		#region DocType
		public new abstract class docType : IBqlField { }

		[PXDBString(3, 
			IsKey = true, 
			IsFixed = true, 
			BqlField = typeof(APInvoice.docType))]
		[APInvoiceType.List]
		[PXUIField(DisplayName = "Type")]
		public override string DocType
		{
			get
			{
				return _DocType;
			}
			set
			{
				_DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : IBqlField { }

		[PXDBString(15, 
			IsKey = true, 
			IsUnicode = true, 
			InputMask = ">CCCCCCCCCCCCCCC", 
			BqlField = typeof(APInvoice.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXSelector(typeof(APInvoiceExt.refNbr))]
		public override string RefNbr
		{
			get
			{
				return _RefNbr;
			}
			set
			{
				_RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : IBqlField { }

		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXFormula(typeof(IsNull<APInvoiceExt.aPTranLineNbr, int0>))]
		public virtual int? LineNbr
	{
			get;
			set;
		}
		#endregion

		#endregion

		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		/// <summary>
		/// Code of the <see cref="PX.Objects.CM.Currency">Currency</see> of the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Company.BaseCuryID">company's base currency</see>.
		/// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(APRegister.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, FieldClass = nameof(FeaturesSet.Multicurrency))]
		[PXDefault(typeof(Current<AccessInfo.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public override string CuryID
		{
			get;
			set;
		}

		#endregion
		#region DisplayProjectID
		public abstract class displayProjectID : IBqlField { }

		[PXInt]
		[PXUIField(DisplayName = "Project", Enabled = false)]
		[PXSelector(typeof(PMProject.contractID),
			SubstituteKey = typeof(PMProject.contractCD),
			ValidateValue = false)]
		[PXFormula(typeof(Switch<Case<Where<APInvoiceExt.paymentsByLinesAllowed, Equal<True>>, APInvoiceExt.aPTranProjectID>, APInvoiceExt.projectID>))]
		public virtual int? DisplayProjectID
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainageBal
		public abstract class curyRetainageBal : IBqlField { }

		[PXCurrency(typeof(APInvoiceExt.curyInfoID), typeof(APInvoiceExt.retainageBal), BaseCalc = false)]
		[PXFormula(typeof(Mult<
			IsNull<APInvoiceExt.aPTranCuryRetainageBal, APInvoiceExt.curyRetainageUnreleasedAmt>,
			SignAmount<APInvoiceExt.docType>>))]
		public virtual decimal? CuryRetainageBal
		{
			get;
			set;
		}
		#endregion
		#region RetainageBal
		public abstract class retainageBal : IBqlField { }

		[PXBaseCury]
		[PXFormula(typeof(Mult<
			IsNull<APInvoiceExt.aPTranRetainageBal, APInvoiceExt.retainageUnreleasedAmt>,
			SignAmount<APInvoiceExt.docType>>))]
		public virtual decimal? RetainageBal
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDocAmtWithRetainageTotal
		public new abstract class curyOrigDocAmtWithRetainageTotal : IBqlField { }

		[PXCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.origDocAmtWithRetainageTotal), BaseCalc = false)]
		[PXUIField(DisplayName = "Total Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Mult<
			IsNull<Add<APInvoiceExt.aPTranCuryOrigRetainageAmt, APInvoiceExt.aPTranCuryOrigTranAmt>, Add<APRegister.curyOrigDocAmt, APRegister.curyRetainageTotal>>,
			SignAmount<APInvoiceExt.docType>>))]
		public override decimal? CuryOrigDocAmtWithRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region OrigDocAmtWithRetainageTotal
		public new abstract class origDocAmtWithRetainageTotal : IBqlField { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Total Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXFormula(typeof(Mult<
			IsNull<Add<APInvoiceExt.aPTranOrigRetainageAmt, APInvoiceExt.aPTranOrigTranAmt>,
				Add<APRegister.curyOrigDocAmt, APRegister.curyRetainageTotal>>,
			SignAmount<APInvoiceExt.docType>>))]
		public override decimal? OrigDocAmtWithRetainageTotal
		{
			get;
			set;
		}
		#endregion

		#region RetainageReleasePct
		public abstract class retainageReleasePct : PX.Data.BQL.BqlDecimal.Field<retainageReleasePct> { }

		[UnboundRetainagePercent(
			typeof(True),
			typeof(decimal100),
			typeof(APInvoiceExt.curyRetainageBal),
			typeof(APInvoiceExt.curyRetainageReleasedAmt),
			typeof(APInvoiceExt.retainageReleasePct),
			DisplayName = "Percent to Release")]
		public virtual decimal? RetainageReleasePct
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainageReleasedAmt
		public abstract class curyRetainageReleasedAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageReleasedAmt> { }

		[UnboundRetainageAmount(
			typeof(APInvoiceExt.curyInfoID),
			typeof(APInvoiceExt.curyRetainageBal),
			typeof(APInvoiceExt.curyRetainageReleasedAmt),
			typeof(APInvoiceExt.retainageReleasedAmt),
			typeof(APInvoiceExt.retainageReleasePct),
			DisplayName = "Retainage to Release")]
		public virtual decimal? CuryRetainageReleasedAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageReleasedAmt

		public abstract class retainageReleasedAmt : PX.Data.BQL.BqlDecimal.Field<retainageReleasedAmt> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainageReleasedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainageUnreleasedCalcAmt
		public abstract class curyRetainageUnreleasedCalcAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageUnreleasedCalcAmt> { }

		[PXCurrency(typeof(APInvoiceExt.curyInfoID), typeof(APInvoiceExt.retainageUnreleasedCalcAmt))]
		[PXUIField(DisplayName = "Unreleased Retainage")]
		[PXFormula(typeof(Sub<APInvoiceExt.curyRetainageBal, APInvoiceExt.curyRetainageReleasedAmt>))]
		public virtual decimal? CuryRetainageUnreleasedCalcAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageUnreleasedCalcAmt
		public abstract class retainageUnreleasedCalcAmt : PX.Data.BQL.BqlDecimal.Field<retainageUnreleasedCalcAmt> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainageUnreleasedCalcAmt
		{
			get;
			set;
		}
		#endregion

		#region APTran fields

		#region APTranLineNbr
		public abstract class aPTranLineNbr : IBqlField { }

		[PXDBInt(BqlField = typeof(APTran.lineNbr))]
		public virtual int? APTranLineNbr
		{
			get;
			set;
		}
		#endregion
		#region APTranInventoryID
		public abstract class aPTranInventoryID : IBqlField { }

		[PXDBInt(BqlField = typeof(APTran.inventoryID))]
		[PXUIField(DisplayName = "Inventory ID",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXSelector(typeof(InventoryItem.inventoryID),
			SubstituteKey = typeof(InventoryItem.inventoryCD),
			ValidateValue = false)]
		public virtual int? APTranInventoryID
		{
			get;
			set;
		}
		#endregion
		#region APTranProjectID
		public abstract class aPTranProjectID : IBqlField { }

		[PXDBInt(BqlField = typeof(APTran.projectID))]
		[PXUIField(DisplayName = "Project",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXSelector(typeof(PMProject.contractID),
			SubstituteKey = typeof(PMProject.contractCD),
			ValidateValue = false)]
		public virtual int? APTranProjectID
		{
			get;
			set;
		}
		#endregion
		#region APTranTaskID
		public abstract class aPTranTaskID : IBqlField { }

		[PXDBInt(BqlField = typeof(APTran.taskID))]
		[PXUIField(DisplayName = "Project Task",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXSelector(typeof(PMTask.taskID),
			SubstituteKey = typeof(PMTask.taskCD),
			ValidateValue = false)]
		public virtual int? APTranTaskID
		{
			get;
			set;
		}
		#endregion
		#region APTranCostCodeID
		public abstract class aPTranCostCodeID : IBqlField { }

		[PXDBInt(BqlField = typeof(APTran.costCodeID))]
		[PXUIField(DisplayName = "Cost Code",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXSelector(typeof(PMCostCode.costCodeID),
			SubstituteKey = typeof(PMCostCode.costCodeCD),
			ValidateValue = false)]
		public virtual int? APTranCostCodeID
		{
			get;
			set;
		}
		#endregion
		#region APTranAccountID
		public abstract class aPTranAccountID : IBqlField { }

		[PXDBInt(BqlField = typeof(APTran.accountID))]
		[PXUIField(DisplayName = "Account",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXSelector(typeof(Account.accountID),
			SubstituteKey = typeof(Account.accountCD),
			ValidateValue = false)]
		public virtual int? APTranAccountID
		{
			get;
			set;
		}
		#endregion
		#region APTranCuryOrigRetainageAmt
		public abstract class aPTranCuryOrigRetainageAmt : IBqlField { }

		[PXDBDecimal(BqlField = typeof(APTran.curyOrigRetainageAmt))]
		public virtual decimal? APTranCuryOrigRetainageAmt
		{
			get;
			set;
		}
		#endregion
		#region APTranOrigRetainageAmt
		public abstract class aPTranOrigRetainageAmt : IBqlField { }

		[PXDBDecimal(BqlField = typeof(APTran.origRetainageAmt))]
		public virtual decimal? APTranOrigRetainageAmt
		{
			get;
			set;
		}
		#endregion
		#region APTranCuryRetainageBal
		public abstract class aPTranCuryRetainageBal : IBqlField { }

		[PXDBDecimal(BqlField = typeof(APTran.curyRetainageBal))]
		public virtual decimal? APTranCuryRetainageBal
		{
			get;
			set;
		}
		#endregion
		#region APTranRetainageBal
		public abstract class aPTranRetainageBal : IBqlField { }

		[PXDBDecimal(BqlField = typeof(APTran.retainageBal))]
		public virtual decimal? APTranRetainageBal
		{
			get;
			set;
		}
		#endregion
		#region APTranCuryOrigTranAmt
		public abstract class aPTranCuryOrigTranAmt : IBqlField { }

		[PXDBDecimal(BqlField = typeof(APTran.curyOrigTranAmt))]
		public virtual decimal? APTranCuryOrigTranAmt
		{
			get;
			set;
	}
		#endregion
		#region APTranOrigTranAmt
		public abstract class aPTranOrigTranAmt : IBqlField { }

		[PXDBDecimal(BqlField = typeof(APTran.origTranAmt))]
		public virtual decimal? APTranOrigTranAmt
		{
			get;
			set;
		}
		#endregion

		#endregion
	}

	[TableAndChartDashboardType]
	public class APRetainageRelease : PXGraph<APRetainageRelease>
	{
		public PXFilter<APRetainageFilter> Filter;
		public PXCancel<APRetainageFilter> Cancel;
		public PXSelect<APInvoice> APInvoiceDummy;

		[PXFilterable]
		public PXFilteredProcessing<APInvoiceExt, APRetainageFilter> DocumentList;

		public PXSetup<APSetup> APSetup;

		public PXAction<APRetainageFilter> viewDocument;
		[PXButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (DocumentList.Current != null)
			{
				PXRedirectHelper.TryRedirect(DocumentList.Cache, DocumentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		protected virtual IEnumerable documentList()
		{
			var docList = new SelectFrom<APInvoiceExt>.View(this);

			if (APSetup.Current.MigrationMode == true)
			{
				docList.WhereAnd<Where<APInvoiceExt.isMigratedRecord.IsEqual<True>>>();
			}

			if (Filter.Current.OrgBAccountID != null)
			{
				docList.WhereAnd<Where<APInvoiceExt.branchID, InsideBranchesOf<Current<APRetainageFilter.orgBAccountID>>>>();
			}

			foreach (APInvoiceExt doc in docList.Select())
			{
				APRetainageInvoice unreleasedDocument = PXSelectJoin<APRetainageInvoice,
					LeftJoin<APTran, On<APRetainageInvoice.paymentsByLinesAllowed, Equal<True>,
						And<APTran.tranType, Equal<APRetainageInvoice.docType>,
						And<APTran.refNbr, Equal<APRetainageInvoice.refNbr>,
						And<APTran.origLineNbr, Equal<Required<APTran.origLineNbr>>>>>>>,
					Where<APRetainageInvoice.isRetainageDocument, Equal<True>,
						And<APRetainageInvoice.origDocType, Equal<Required<APInvoice.docType>>,
						And<APRetainageInvoice.origRefNbr, Equal<Required<APInvoice.refNbr>>,
						And<APRetainageInvoice.released, NotEqual<True>,
						And<Where<APRetainageInvoice.paymentsByLinesAllowed, NotEqual<True>,
							Or<APTran.lineNbr, IsNotNull>>>>>>>>
					.SelectSingleBound(this, null, doc.APTranLineNbr, doc.DocType, doc.RefNbr);

				if (unreleasedDocument == null)
				{
					yield return doc;
				}
			}
		}

		public APRetainageRelease()
		{
			APSetup setup = APSetup.Current;

			bool isRequireSingleProjectPerDocument = APSetup.Current?.RequireSingleProjectPerDocument == true;

			PXUIFieldAttribute.SetVisible<APRetainageFilter.projectID>(Filter.Cache, null, isRequireSingleProjectPerDocument);
			PXUIFieldAttribute.SetVisible<APInvoiceExt.displayProjectID>(DocumentList.Cache, null, 
				isRequireSingleProjectPerDocument || PXAccess.FeatureInstalled<FeaturesSet.paymentsByLines>());
			PeriodValidation validationValue = this.IsContractBasedAPI ||
				this.IsImport || this.IsExport || this.UnattendedMode ? PeriodValidation.DefaultUpdate : PeriodValidation.DefaultSelectUpdate;
			OpenPeriodAttribute.SetValidatePeriod<APRetainageFilter.finPeriodID>(Filter.Cache, null, validationValue);
		}

		protected virtual void APRetainageFilter_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APRetainageFilter filter = e.Row as APRetainageFilter;
			if (filter == null) return;

			var document = PXSelectorAttribute.Select<APRetainageFilter.refNbr>(sender, filter, e.NewValue);
			if (document is null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void APRetainageFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			APRetainageFilter filter = e.Row as APRetainageFilter;
			if (filter == null) return;

			bool isAutoRelease = APSetup.Current?.RetainageBillsAutoRelease == true && APSetup.Current?.MigrationMode != true;

			DocumentList.SetProcessDelegate(delegate (List<APInvoiceExt> list)
			{
				APInvoiceEntry graph = CreateInstance<APInvoiceEntry>();
				APInvoiceEntryRetainage retainageExt = graph.GetExtension<APInvoiceEntryRetainage>();

				RetainageOptions retainageOptions = new RetainageOptions
				{
					DocDate = filter.DocDate,
					MasterFinPeriodID = FinPeriodIDAttribute.CalcMasterPeriodID<APRetainageFilter.finPeriodID>(graph.Caches[typeof(APRetainageFilter)], filter)
				};

				retainageExt.ReleaseRetainageProc(list, retainageOptions, isAutoRelease);
			});
		}

		protected virtual void APInvoiceExt_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			APInvoiceExt invoice = e.Row as APInvoiceExt;
			if (invoice == null) return;

			PXUIFieldAttribute.SetEnabled(sender, invoice, false);
			PXUIFieldAttribute.SetEnabled<APInvoiceExt.selected>(sender, invoice, true);
			PXUIFieldAttribute.SetEnabled<APInvoiceExt.retainageReleasePct>(sender, invoice, true);
			PXUIFieldAttribute.SetEnabled<APInvoiceExt.curyRetainageReleasedAmt>(sender, invoice, true);

			if (invoice.Selected ?? true)
			{
				Dictionary<String, String> errors = PXUIFieldAttribute.GetErrors(sender, invoice, PXErrorLevel.Error);
				if (errors.Count > 0)
				{
					invoice.Selected = false;
					DocumentList.Cache.SetStatus(invoice, PXEntryStatus.Updated);
					sender.RaiseExceptionHandling<APInvoiceExt.selected>(
						invoice,
						null,
						new PXSetPropertyException(Messages.ErrorRaised, PXErrorLevel.RowError));

					PXUIFieldAttribute.SetEnabled<APInvoiceExt.selected>(sender, invoice, false);
				}
			}
		}

		public override bool IsDirty => false;
	}
}

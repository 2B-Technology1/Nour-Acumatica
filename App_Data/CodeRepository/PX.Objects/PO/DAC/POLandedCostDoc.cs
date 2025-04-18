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
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.WorkflowAPI;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PO.LandedCosts;
using PX.Objects.PO.LandedCosts.Attributes;
using CRLocation = PX.Objects.CR.Standalone.Location;


namespace PX.Objects.PO
{
	[Serializable]
	[PXCacheName(Messages.POLandedCostDoc)]
	[PXPrimaryGraph(typeof(POLandedCostDocEntry))]
	[PXGroupMask(typeof(InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POLandedCostDoc.vendorID>, And<Match<Vendor, Current<AccessInfo.userName>>>>>))]
	public partial class POLandedCostDoc : PX.Data.IBqlTable, IAssign
	{
		#region Keys
		public class PK : PrimaryKeyOf<POLandedCostDoc>.By<docType, refNbr>
		{
			public static POLandedCostDoc Find(PXGraph graph, string docType, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, docType, refNbr, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<POLandedCostDoc>.By<branchID> { }
			public class Vendor : BAccount.PK.ForeignKeyOf<POLandedCostDoc>.By<vendorID> { }
			public class VandorLocation : Location.PK.ForeignKeyOf<POLandedCostDoc>.By<vendorID, vendorLocationID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<POLandedCostDoc>.By<curyID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<POLandedCostDoc>.By<curyInfoID> { }
			public class Terms : CS.Terms.PK.ForeignKeyOf<POLandedCostDoc>.By<termsID> { }
			public class TaxZone : TX.TaxZone.PK.ForeignKeyOf<POLandedCostDoc>.By<taxZoneID> { }
			public class PayToVendor : AP.Vendor.PK.ForeignKeyOf<POLandedCostDoc>.By<payToVendorID> { }
			public class Workgroup : TM.EPCompanyTree.PK.ForeignKeyOf<POLandedCostDoc>.By<workgroupID> { }
			public class Owner : CR.Contact.PK.ForeignKeyOf<POLandedCostDoc>.By<ownerID> { }
			//todo FinancialPeriod : FinPeriod.PK.ForeignKeyOf<POLandedCostDoc>.By<finPeriodID> { }
			//todo TransactionPeriod : FinPeriod.PK.ForeignKeyOf<POLandedCostDoc>.By<tranPeriodID> { }
		}
		#endregion
		#region Events
		public class Events : PXEntityEvent<POLandedCostDoc>.Container<Events>
		{
			public PXEntityEvent<POLandedCostDoc> InventoryAdjustmentCreated;
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		/// <summary>
		/// Type of the document.
		/// </summary>
		/// <value>
		/// Possible values are: "LC" - Landed Cost, "LCC" - Correction, "LCR" - Reversal.
		/// </value>
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault(POLandedCostDocType.LandedCost)]
		[POLandedCostDocType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public virtual String DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <summary>
		/// Reference number of the document.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<POLandedCostDoc.refNbr, Where<POLandedCostDoc.docType, Equal<Optional<POLandedCostDoc.docType>>>>),
			Filterable = true)]
		[PXFieldDescription]
		[LandedCostDocNumbering]
		public virtual String RefNbr
		{
			get;
			set; 
		}

		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// The identifier of the <see cref="Branch">branch</see> to which the document belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Branch(typeof(Coalesce<
			Search<Location.vBranchID, Where<Location.bAccountID, Equal<Current<vendorID>>, And<Location.locationID, Equal<Current<vendorLocationID>>>>>,
			Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false)]
		public virtual Int32? BranchID
		{
			get;
			set;
		}
		#endregion

		#region OpenDoc
		public abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		protected Boolean? _OpenDoc;

		/// <summary>
		/// When set to <c>true</c> indicates that the document is open.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Open", Visible = false)]
		public virtual Boolean? OpenDoc
		{
			get
			{
				return this._OpenDoc;
			}
			set
			{
				this._OpenDoc = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;

		/// <summary>
		/// When set to <c>true</c> indicates that the document was released.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Visible = false, Enabled = false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;

		/// <summary>
		/// When set to <c>true</c> indicates that the document is on hold and thus cannot be released.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Enabled = false)]
		[PXDefault(true, typeof(POSetup.holdLandedCosts))]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// Status of the document. The field is calculated based on the values of status flag. It can't be changed directly.
		/// The fields tht determine status of a document are: <see cref="POLandedCostDocStatus.Hold"/>, <see cref="POLandedCostDocStatus.Balanced"/>, <see cref="POLandedCostDocStatus.Released"/>.
		/// </summary>
		/// <value>
		/// Possible values are: 
		/// <c>"H"</c> - Hold, <c>"B"</c> - Balanced, <c>"R"</c> - Released.
		/// Defaults to Hold.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(POLandedCostDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[POLandedCostDocStatus.List]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }

		/// <summary>
		/// When <c>true</c>, indicates that the amount of tax calculated with the external External Tax Provider is up to date.
		/// If this field equals <c>false</c>, the document was updated since last synchronization with the Tax Engine
		/// and taxes might need recalculation.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax Is Up to Date", Enabled = false)]
		public virtual Boolean? IsTaxValid
		{
			get;
			set;
		}
		#endregion
		#region NonTaxable
		public abstract class nonTaxable : PX.Data.BQL.BqlBool.Field<nonTaxable> { }
		/// <summary>
		/// Get or set NonTaxable that mark current document does not impose sales taxes.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Non-Taxable", Enabled = false)]
		public virtual Boolean? NonTaxable
		{
			get;
			set;
		}
		#endregion

		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		protected DateTime? _DocDate;

		/// <summary>
		/// Date of the document.
		/// </summary>
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        /// <summary>
        /// <see cref="FinPeriod">Financial Period</see> of the document.
        /// </summary>
        /// <value>
        /// Defaults to the period, to which the <see cref="POLandedCostDoc.DocDate"/> belongs, but can be overriden by user.
        /// </value>
        [POOpenPeriod(
			sourceType: typeof(POLandedCostDoc.docDate),
			branchSourceType: typeof(POLandedCostDoc.branchID),
			masterFinPeriodIDType: typeof(POLandedCostDoc.tranPeriodID),
			IsHeader = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }

        /// <summary>
        /// <see cref="FinPeriod">Financial Period</see> of the document.
        /// </summary>
        /// <value>
        /// Determined by the <see cref="POLandedCostDoc.DocDate">date of the document</see>. Unlike <see cref="POLandedCostDoc.FinPeriodID"/>
        /// the value of this field can't be overriden by user.
        /// </value>
        [PeriodID]
		[PXUIField(DisplayName = "Transaction Period")]
		public virtual String TranPeriodID
		{
			get;
			set;
		}
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID>
		{
			public class PreventEditBAccountVOrgBAccountID<TGraph> :
				PreventEditBAccountRestrictToBase<BAccount.vOrgBAccountID, TGraph, POLandedCostDoc, 
					SelectFrom<POLandedCostDoc>
					.Where<POLandedCostDoc.vendorID.IsEqual<BAccount.bAccountID.FromCurrent>>>
				where TGraph : PXGraph
			{
				protected override string GetErrorMessage(BAccount baccount, POLandedCostDoc document, string documentBaseCurrency)
				{
					return PXMessages.LocalizeFormatNoPrefix(Messages.CannotChangeRestricToIfLCExists,
							documentBaseCurrency, baccount.AcctCD, document.RefNbr);
				}
			}

			public class PreventEditBAccountVOrgBAccountIDOnVendorMaint : PreventEditBAccountVOrgBAccountID<VendorMaint>
			{
				public static bool IsActive()
					=> PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
			}

			public class PreventEditBAccountVOrgBAccountIDOnCustomerMaint : PreventEditBAccountVOrgBAccountID<CustomerMaint>
			{
				public static bool IsActive()
					=> PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
			}
		}
		/// <summary>
		/// Identifier of the <see cref="Vendor"/>, whom the document belongs to.
		/// </summary>
		[LandedCostVendorActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
		[PXDefault]
		[PXForeignReference(typeof(FK.Vendor))]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }

		/// <summary>
		/// Identifier of the <see cref="Location">Location</see> of the <see cref="Vendor">Vendor</see>, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Location.LocationID"/> field. Defaults to vendor's <see cref="Vendor.DefLocationID">default location</see>.
		/// </value>
		[LocationActive(
			typeof(Where<Location.bAccountID, Equal<Optional<vendorID>>,
				And<MatchWithBranch<Location.vBranchID>>>),
			DescriptionField = typeof(Location.descr),
			Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Coalesce<Search2<Vendor.defLocationID,
				InnerJoin<CRLocation, 
					On<CRLocation.locationID, Equal<Vendor.defLocationID>, 
					And<CRLocation.bAccountID, Equal<Vendor.bAccountID>>>>,
				Where<Vendor.bAccountID, Equal<Current<vendorID>>,
					And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>,
			Search<CRLocation.locationID,
				Where<CRLocation.bAccountID, Equal<Current<vendorID>>,
					And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>))]
		/*[PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
				InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
				Where<BAccountR.bAccountID, Equal<Current<POLandedCostDoc.vendorID>>,
					And<CRLocation.isActive, Equal<True>,
						And<MatchWithBranch<CRLocation.vBranchID>>>>>,
			Search<CRLocation.locationID,
				Where<CRLocation.bAccountID, Equal<Current<POLandedCostDoc.vendorID>>,
					And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>))]*/
		[PXForeignReference(typeof(CompositeKey<Field<vendorID>.IsRelatedTo<Location.bAccountID>,Field<vendorLocationID>.IsRelatedTo<Location.locationID>>))]

		public virtual Int32? VendorLocationID
		{
			get;
			set;
		}
		#endregion

		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(curyInfoID), typeof(POLandedCostDoc.taxTotal))]
		[PXUIField(DisplayName = "Tax Total", Enabled = false)]
		public virtual Decimal? CuryTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal
		{
			get;
			set;
		}
		#endregion

		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

		/// <summary>
		/// Counter of the document lines, used <i>internally</i> to assign numbers to newly created lines.
		/// It is not recommended to rely on this fields to determine the exact count of lines, because it might not reflect the latter under various conditions.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		/// <summary>
		/// Code of the <see cref="PX.Objects.CM.Currency">Currency</see> of the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Company.BaseCuryID">company's base currency</see>.
		/// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Vendor.curyID>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		/// <summary>
		/// The identifier of the <see cref="CurrencyInfo">CurrencyInfo</see> object associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CurrencyInfoID"/> field.
		/// </value>
		[PXDBLong()]
		[CurrencyInfo]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CreateBill
		public abstract class createBill : PX.Data.BQL.BqlBool.Field<createBill> { }
		/// <summary>
		/// Get or set CreateBill that mark current document create bill on release.
		/// </summary>
		[PXDBBool]
		[PXDefault(typeof(POSetup.autoCreateLCAP))]
		[PXUIField(DisplayName = "Create Bill", Enabled = true)]
		public virtual Boolean? CreateBill
		{
			get;
			set;
		}
		#endregion
		#region VendorRefNbr
		public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }

		[PXDBString(40, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vendor Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		[POLandedCostDocVendorRefNbr]
		public virtual String VendorRefNbr
		{
			get;
			set;
		}
		#endregion

		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }

		/// <summary>
		/// The document total presented in the currency of the document. (See <see cref="CuryID"/>)
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(lineTotal))]
		[PXUIField(DisplayName = "Amount Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal
		{
			get;
			set;
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }

		/// <summary>
		/// The document total presented in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryDocTotal
		public abstract class curyDocTotal : PX.Data.BQL.BqlDecimal.Field<curyDocTotal> { }

		/// <summary>
		/// The document total presented in the currency of the document. (See <see cref="CuryID"/>)
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(docTotal))]
		[PXUIField(DisplayName = "Document Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryDocTotal
		{
			get;
			set;
		}
		#endregion
		#region DocTotal
		public abstract class docTotal : PX.Data.BQL.BqlDecimal.Field<docTotal> { }

		/// <summary>
		/// The document total presented in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DocTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryAllocatedTotal
		public abstract class curyAllocatedTotal : PX.Data.BQL.BqlDecimal.Field<curyAllocatedTotal> { }

		/// <summary>
		/// The total allocated amount of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(curyInfoID), typeof(allocatedTotal), BaseCalc = false)]
		[PXUIField(DisplayName = "Total Allocated Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryAllocatedTotal
		{
			get;
			set;
		}
		#endregion
		#region AllocatedTotal
		public abstract class allocatedTotal : PX.Data.BQL.BqlDecimal.Field<allocatedTotal> { }

		/// <summary>
		/// The total allocated amount of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AllocatedTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryControlTotal
		public abstract class curyControlTotal : PX.Data.BQL.BqlDecimal.Field<curyControlTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(controlTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Total")]
		public virtual Decimal? CuryControlTotal
		{
			get;
			set;
		}
		#endregion
		#region ControlTotal
		public abstract class controlTotal : PX.Data.BQL.BqlDecimal.Field<controlTotal> { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ControlTotal
		{
			get;
			set;
		}
		#endregion

		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXDefault(
			typeof(Search<Vendor.termsID,
				Where2<FeatureInstalled<FeaturesSet.vendorRelations>,
					And<Vendor.bAccountID, Equal<Current<payToVendorID>>,
						Or2<Not<FeatureInstalled<FeaturesSet.vendorRelations>>,
							And<Vendor.bAccountID, Equal<Current<vendorID>>>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[Terms(typeof(billDate), typeof(dueDate), typeof(discDate), typeof(curyLineTotal), typeof(curyDiscAmt), typeof(curyTaxTotal))]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.vendor>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		public virtual string TermsID
		{
			get;
			set;
		}
		#endregion
		#region BillDate
		public abstract class billDate : PX.Data.BQL.BqlDateTime.Field<billDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "Bill Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(docDate))]
		[PXDefault(typeof(docDate), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? BillDate
		{
			get;
			set;
		}
		#endregion
		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DueDate
		{
			get;
			set;
		}
		#endregion
		#region DiscDate
		public abstract class discDate : PX.Data.BQL.BqlDateTime.Field<discDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DiscDate
		{
			get;
			set;
		}
		#endregion
		#region CuryDiscAmt
		public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(curyInfoID), typeof(discAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region DiscAmt
		public abstract class discAmt : PX.Data.BQL.BqlDecimal.Field<discAmt> { }
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Location.vTaxZoneID, Where<Location.bAccountID, Equal<Current<vendorID>>, And<Location.locationID, Equal<Current<vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vendor Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TX.TaxZone.taxZoneID), DescriptionField = typeof(TX.TaxZone.descr), Filterable = true)]
		[PXRestrictor(typeof(Where<TX.TaxZone.isManualVATZone, Equal<False>>), TX.Messages.CantUseManualVAT)]
		public virtual String TaxZoneID
		{
			get;
			set;
		}
		#endregion
		#region PayToVendorID
		public abstract class payToVendorID : PX.Data.BQL.BqlInt.Field<payToVendorID> { }

		/// <summary>
		/// A reference to the <see cref="Vendor"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the vendor, whom the AP bill will belong to. 
		/// </value>
		[PXFormula(typeof(Validate<curyID>))]
		[POLandedCostPayToVendor(CacheGlobal = true, Filterable = true)]
		[PXDefault]
		[PXForeignReference(typeof(FK.PayToVendor))]
		public virtual int? PayToVendorID
		{
			get;
			set;
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		[PXDBInt]
		[PXDefault(typeof(Vendor.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PX.TM.PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
		public virtual int? WorkgroupID
		{
			get;
			set;
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		protected int? _OwnerID;
		[PXDefault(typeof(Vendor.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PX.TM.Owner(typeof(workgroupID), Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? OwnerID
		{
			get;
			set;
		}
		#endregion
		#region IAssign Members

		int? PX.Data.EP.IAssign.WorkgroupID
		{
			get
			{
				return WorkgroupID;
			}
			set
			{
				WorkgroupID = value;
			}
		}

		int? PX.Data.EP.IAssign.OwnerID
		{
			get
			{
				return OwnerID;
			}
			set
			{
				OwnerID = value;
			}
		}

		#endregion

		#region APDocCreated
		public abstract class aPDocCreated : PX.Data.BQL.BqlBool.Field<aPDocCreated> { }

		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? APDocCreated
		{
			get;
			set;
		}
		#endregion
		#region INDocCreated
		public abstract class iNDocCreated : PX.Data.BQL.BqlBool.Field<iNDocCreated> { }

		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? INDocCreated
		{
			get;
			set;
		}
		#endregion

		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }

		/// <summary>
		/// The part of the document total that is exempt from VAT. 
		/// This total is calculated as a sum of the taxable amounts for the <see cref="PX.Objects.TX.Tax">taxes</see>
		/// of <see cref="PX.Objects.TX.Tax.TaxType">type</see> VAT, which are marked as <see cref="PX.Objects.TX.Tax.ExemptTax">exempt</see> 
		/// and are neither <see cref="PX.Objects.TX.Tax.StatisticalTax">statistical</see> nor <see cref="PX.Objects.TX.Tax.ReverseTax">reverse</see>.
		/// (Presented in the currency of the document, see <see cref="CuryID"/>)
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(vatExemptTotal))]
		[PXUIField(DisplayName = "VAT Exempt Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatExemptTotal
		{
			get;
			set;
		}
		#endregion
		#region VatExemptTaxTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }

		/// <summary>
		/// The part of the document total that is exempt from VAT. 
		/// This total is calculated as a sum of the taxable amounts for the <see cref="PX.Objects.TX.Tax">taxes</see>
		/// of <see cref="PX.Objects.TX.Tax.TaxType">type</see> VAT, which are marked as <see cref="PX.Objects.TX.Tax.ExemptTax">exempt</see> 
		/// and are neither <see cref="PX.Objects.TX.Tax.StatisticalTax">statistical</see> nor <see cref="PX.Objects.TX.Tax.ReverseTax">reverse</see>.
		/// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatExemptTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }

		/// <summary>
		/// The part of the document total, which is subject to VAT.
		/// This total is calculated as a sum of the taxable amounts for the <see cref="PX.Objects.TX.Tax">taxes</see>
		/// of <see cref="PX.Objects.TX.Tax.TaxType">type</see> VAT, which are neither <see cref="PX.Objects.TX.Tax.ExemptTax">exempt</see>, 
		/// nor <see cref="PX.Objects.TX.Tax.StatisticalTax">statistical</see>, nor <see cref="PX.Objects.TX.Tax.ReverseTax">reverse</see>.
		/// (Presented in the currency of the document, see <see cref="CuryID"/>)
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(vatTaxableTotal))]
		[PXUIField(DisplayName = "VAT Taxable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatTaxableTotal
		{
			get;
			set;
		}
		#endregion
		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }

		/// <summary>
		/// The part of the document total, which is subject to VAT.
		/// This total is calculated as a sum of the taxable amounts for the <see cref="PX.Objects.TX.Tax">taxes</see>
		/// of <see cref="PX.Objects.TX.Tax.TaxType">type</see> VAT, which are neither <see cref="PX.Objects.TX.Tax.ExemptTax">exempt</see>, 
		/// nor <see cref="PX.Objects.TX.Tax.StatisticalTax">statistical</see>, nor <see cref="PX.Objects.TX.Tax.ReverseTax">reverse</see>.
		/// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatTaxableTotal
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXSearchable(SM.SearchCategory.PO, "{0}: {1} - {3}", new Type[] { typeof(POLandedCostDoc.docType), typeof(POLandedCostDoc.refNbr), typeof(POLandedCostDoc.vendorID), typeof(Vendor.acctName) },
		   new Type[] { typeof(POLandedCostDoc.vendorRefNbr) },
		   NumberFields = new Type[] { typeof(POLandedCostDoc.refNbr) },
		   Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(POLandedCostDoc.docDate), typeof(POLandedCostDoc.status), typeof(POLandedCostDoc.vendorRefNbr) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(POLandedCostDoc.curyDocTotal) },
		   MatchWithJoin = typeof(InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POLandedCostDoc.vendorID>>>),
		   SelectForFastIndexing = typeof(Select2<POLandedCostDoc, InnerJoin<Vendor, On<POLandedCostDoc.vendorID, Equal<Vendor.bAccountID>>>>)
		)]
		[PXNote(ShowInReferenceSelector = true, Selector = typeof(
			Search2<
				POLandedCostDoc.refNbr,
			LeftJoinSingleTable<Vendor,
				On<Vendor.bAccountID, Equal<POLandedCostDoc.vendorID>>>,
			Where<
				Vendor.bAccountID, IsNull,
				Or<Match<Vendor, Current<AccessInfo.userName>>>>,
			OrderBy<
				Desc<POLandedCostDoc.refNbr>>>))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}

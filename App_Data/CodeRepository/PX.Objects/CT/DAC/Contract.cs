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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.TM;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.Common.Discount;
using PX.Data.EP;
using System.Collections;
using PX.Data.WorkflowAPI;
using PX.Objects.TX;
using PX.Objects.EP;

namespace PX.Objects.CT
{
	
	[System.SerializableAttribute()]

	[PXPrimaryGraph(new Type[] {
		typeof(TemplateMaint),
		typeof(ContractMaint),
		typeof(PM.TemplateMaint),
		typeof(ProjectEntry)
	},
		new Type[] {
		typeof(Select<Contract,
			Where<Contract.contractID, Equal<Current<Contract.contractID>>,
			And<Contract.baseType, Equal<CTPRType.contractTemplate>>>>),
		typeof(Select<Contract, 
			Where<Contract.contractID, Equal<Current<Contract.contractID>>, 
			And<Contract.baseType, Equal<CTPRType.contract>>>>),
		typeof(Select<PMProject,
			Where<PMProject.contractID, Equal<Current<PMProject.contractID>>,
			And<PMProject.baseType, Equal<CTPRType.projectTemplate>,
			And<PMProject.nonProject, Equal<False>>>>>),
		typeof(Select<PMProject, 
			Where<PMProject.contractID, Equal<Current<PMProject.contractID>>, 
			And<PMProject.baseType, Equal<CTPRType.project>,
			And<PMProject.nonProject, Equal<False>>>>>)
		})]
	[PXCacheName(Messages.CTContract)]
	public partial class Contract : PX.Data.IBqlTable
	{
		[Obsolete(Common.InternalMessages.ClassIsObsoleteAndWillBeRemoved2019R2)]
		public class ContractBaseType : CTPRType.contract { }
		#region Events
		public class Events : PXEntityEvent<Contract>.Container<Events>
		{
			public PXEntityEvent<Contract> SetupContract;
			public PXEntityEvent<Contract> ActivateContract;
			public PXEntityEvent<Contract> ExpireContract;
			public PXEntityEvent<Contract> CancelContract;
			public PXEntityEvent<Contract> UpgradeContract;
		}
		#endregion

		#region Keys
		public class PK : PrimaryKeyOf<Contract>.By<contractID>
		{
			public static Contract Find(PXGraph graph, int? contractID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, contractID, options);
		}
		public class UK : PrimaryKeyOf<Contract>.By<baseType, contractCD>
		{
			public static Contract Find(PXGraph graph, string baseType, string contractCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, baseType, contractCD, options);
		}
		public static class FK
		{
			public class Customer : AR.Customer.PK.ForeignKeyOf<Contract>.By<customerID> { }
			public class Location : CR.Location.PK.ForeignKeyOf<Contract>.By<customerID, locationID> { }

			public class Currency : CM.Currency.PK.ForeignKeyOf<Contract>.By<curyID> { }
			public class RateType : CM.CurrencyRateType.PK.ForeignKeyOf<Contract>.By<rateTypeID> { }
			public class CSCalendar : CS.CSCalendar.PK.ForeignKeyOf<Contract>.By<calendarID> { }

			public class DefaultSaleAccount : GL.Account.PK.ForeignKeyOf<Contract>.By<defaultSalesAccountID> { }
			public class DefaultSalesSubaccount : GL.Sub.PK.ForeignKeyOf<Contract>.By<defaultSalesSubID> { }
			public class DefaultExpenseAccount : GL.Account.PK.ForeignKeyOf<Contract>.By<defaultExpenseAccountID> { }
			public class DefaultExpenseSubaccount : GL.Sub.PK.ForeignKeyOf<Contract>.By<defaultExpenseSubID> { }

			public class AccrualAccount : GL.Account.PK.ForeignKeyOf<Contract>.By<defaultAccrualAccountID> { }
			public class AccrualSubaccount : GL.Sub.PK.ForeignKeyOf<Contract>.By<defaultAccrualSubID> { }

			public class Branch : GL.Branch.PK.ForeignKeyOf<Contract>.By<defaultBranchID> { }
			public class SalesPerson : AR.SalesPerson.PK.ForeignKeyOf<Contract>.By<salesPersonID> { }
			public class Terms : CS.Terms.PK.ForeignKeyOf<Contract>.By<termsID> { }
			public class PromoCode : AR.ARDiscount.PK.ForeignKeyOf<Contract>.By<discountID> { }
			public class ProjectManager : EP.EPEmployee.PK.ForeignKeyOf<Contract>.By<approverID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		protected Int32? _ContractID;
		[PXDBIdentity()]
		[PXUIField(DisplayName = "Contract ID")]
		public virtual Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion

		#region BaseType
		public abstract class baseType : PX.Data.BQL.BqlString.Field<baseType> { }

		[PXUIField(DisplayName = "Entity Type", Enabled = false)]
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[CTPRType.List]
		[PXDefault(CTPRType.Contract)]
		public virtual string BaseType
		{
			get;
			set;
		}
		#endregion
		#region ContractCD
		public abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }
		protected String _ContractCD;
		[PXDimensionSelector(ContractAttribute.DimensionName,
			typeof(Search2<Contract.contractCD, 
				InnerJoin<ContractBillingSchedule, 
					On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>, 
				LeftJoin<Customer, 
					On<Customer.bAccountID, Equal<Contract.customerID>>>>,
				Where<Contract.baseType, Equal<CTPRType.contract>>>),
			typeof(Contract.contractCD),
			typeof(Contract.contractCD), typeof(Contract.customerID), typeof(Customer.acctName), typeof(Contract.locationID), typeof(Contract.description), typeof(Contract.status), typeof(Contract.expireDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), DescriptionField = typeof(Contract.description), Filterable = true)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Contract ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String ContractCD
		{
			get
			{
				return this._ContractCD;
			}
			set
			{
				if (this._ContractCD != value)
					_contractInfo = null;
				this._ContractCD = value;
			}
		}
		#endregion
		#region TemplateID
		public abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }
		protected Int32? _TemplateID;
		[PXDefault]
		[ContractTemplate(Required = true)]
		[PXRestrictor(typeof(Where<ContractTemplate.status, Equal<Contract.status.active>>), Messages.TemplateIsNotActivated, typeof(ContractTemplate.contractCD))]
		[PXRestrictor(typeof(Where<ContractTemplate.effectiveFrom, LessEqual<Current<AccessInfo.businessDate>>, Or<ContractTemplate.effectiveFrom, IsNull>>), Messages.TemplateIsNotStarted)]
		[PXRestrictor(typeof(Where<ContractTemplate.discontinueAfter, GreaterEqual<Current<AccessInfo.businessDate>>, Or<ContractTemplate.discontinueAfter, IsNull>>), Messages.TemplateIsExpired)]
		public virtual Int32? TemplateID
		{
			get
			{
				return this._TemplateID;
			}
			set
			{
				this._TemplateID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBLocalizableString(255, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				if (this._Description != value)
					_contractInfo = null;
				this._Description = value;
			}
		}
		#endregion
		#region ContractInfo

		public abstract class contractInfo : PX.Data.BQL.BqlString.Field<contractInfo> { }

		public String _contractInfo;
		[PXString]
		[PXUIField(Visible = false)]
		public virtual String ContractInfo
		{
			get
			{
				if (_contractInfo == null)
					_contractInfo = string.Format("{0} - {1}", ContractCD, Description);
				return _contractInfo;
			}
		}

		#endregion
		#region OriginalContractID
		public abstract class originalContractID : PX.Data.BQL.BqlInt.Field<originalContractID> { }
		protected Int32? _OriginalContractID;
		[Contract(DisplayName="Contract", Enabled=false)]
		public virtual Int32? OriginalContractID
		{
			get
			{
				return this._OriginalContractID;
			}
			set
			{
				this._OriginalContractID = value;
			}
		}
		#endregion
		#region MasterContractID
		public abstract class masterContractID : PX.Data.BQL.BqlInt.Field<masterContractID> { }
		protected Int32? _MasterContractID;
		[PXDBInt()]
		[PXSelector(typeof(Contract.contractID))]
		[PXUIField(DisplayName = "Master Contract")]
		public virtual Int32? MasterContractID
		{
			get
			{
				return this._MasterContractID;
			}
			set
			{
				this._MasterContractID = value;
			}
		}
		#endregion
		#region CaseItemID
		public abstract class caseItemID : PX.Data.BQL.BqlInt.Field<caseItemID> { }
		protected Int32? _CaseItemID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Case Count Item")]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, Equal<False>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXForeignReference(typeof(Field<caseItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? CaseItemID
		{
			get
			{
				return this._CaseItemID;
			}
			set
			{
				this._CaseItemID = value;
			}
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Renewable, Expiring, Unlimited },
					new string[] { Messages.Renewable, Messages.Expiring, Messages.Unlimited }) { }
			}
			public const string Renewable = "R";
			public const string Expiring = "E";
			public const string Unlimited = "U";

			public class renewable : PX.Data.BQL.BqlString.Constant<renewable>
			{
				public renewable() : base(Renewable) { }
			}

			public class expiring : PX.Data.BQL.BqlString.Constant<expiring>
			{
				public expiring() : base(Expiring) { }
			}

			public class unlimited : PX.Data.BQL.BqlString.Constant<unlimited>
			{
				public unlimited() : base(Unlimited) { }
			}
		}
		protected String _Type;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Contract Type")]
		[type.List]
		[PXDefault(type.Renewable)]
		public virtual String Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region ClassType
		public abstract class classType : PX.Data.BQL.BqlString.Field<classType> { }
		protected String _ClassType;
		[PXDBString(1, IsFixed = true)]
		public virtual String ClassType
		{
			get
			{
				return this._ClassType;
			}
			set
			{
				this._ClassType = value;
			}
		}
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		//[PXDefault]
		[CustomerActive(typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>),
			DescriptionField = typeof(Customer.acctName), 
			Visibility=PXUIVisibility.SelectorVisible, Filterable = true)]		
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

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<Contract.customerID>>>), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		[PXDefault(typeof(Search<BAccount.defLocationID, Where<BAccount.bAccountID, Equal<Current<Contract.customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<Contract.customerID>.IsRelatedTo<Location.bAccountID>,
				Field<Contract.locationID>.IsRelatedTo<Location.locationID>
			>))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region RateTableID
		public abstract class rateTableID : PX.Data.BQL.BqlString.Field<rateTableID> { }
		protected String _RateTableID;
		[PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
		public virtual String RateTableID
		{
			get
			{
				return this._RateTableID;
			}
			set
			{
				this._RateTableID = value;
			}
		}
		#endregion
		#region Balance
		public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
		protected decimal? _Balance;
		[PXBaseCury]
		[PXUIField(DisplayName = "Balance", Enabled = false)]
		public decimal? Balance
		{
			get
			{
				return this._Balance;
			}
			set
			{
				this._Balance = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Draft, InApproval, Active, Expired, Canceled, InUpgrade, PendingActivation },
					new string[] { Messages.Draft, Messages.InApproval, Messages.Active, Messages.Expired, Messages.Canceled, Messages.InUpgrade, Messages.PendingActivation }) {}
			}
			public const string Draft = "D";
			public const string InApproval = "I";
			public const string Active = "A";
			public const string Expired = "E";
			public const string Canceled = "C";
			public const string Completed = "F";
			public const string InUpgrade = "U";
			public const string PendingActivation = "P";
			public const string OnHold = "H";

			public class draft : PX.Data.BQL.BqlString.Constant<draft>
			{
				public draft() : base(Draft) {}
			}
			public class inApproval : PX.Data.BQL.BqlString.Constant<inApproval>
			{
				public inApproval() : base(InApproval) {}
			}
			public class active : PX.Data.BQL.BqlString.Constant<active>
			{
				public active() : base(Active) {}
			}
			public class expired : PX.Data.BQL.BqlString.Constant<expired>
			{
				public expired() : base(Expired) {}
			}
			public class canceled : PX.Data.BQL.BqlString.Constant<canceled>
			{
				public canceled() : base(Canceled) {}
			}
			public class completed : PX.Data.BQL.BqlString.Constant<completed>
			{
				public completed() : base(Completed) {}
			}
			public class inUpgrade : PX.Data.BQL.BqlString.Constant<inUpgrade>
			{
				public inUpgrade() : base(InUpgrade) {}
			}
			public class pendingActivation : PX.Data.BQL.BqlString.Constant<pendingActivation>
			{
				public pendingActivation() : base(PendingActivation) {}
			}

			public class onHold : PX.Data.BQL.BqlString.Constant<onHold>
			{
				public onHold() : base(OnHold) {; }
			}
		}
		protected string _Status;
		[PXDBString(1, IsFixed = true)]
		[status.List]
		[PXDefault(status.Draft)]
		[PXUIField(DisplayName = "Status", Required=true, Visibility=PXUIVisibility.SelectorVisible, Enabled=false)]
		public virtual string Status
		{
			get
			{
				return _Status;
			}
			set
			{
				_Status = value;
			}
		}
		#endregion
		#region Duration
		public abstract class duration : PX.Data.BQL.BqlInt.Field<duration> { }
		protected Int32? _Duration;
		[PXDBInt(MinValue = 1, MaxValue = 1000)]
		[PXUIField(DisplayName = "Duration")]
		[PXDefault(1)]
		public virtual Int32? Duration
		{
			get
			{
				return this._Duration;
			}
			set
			{
				this._Duration = value;
			}
		}
		#endregion
		#region DurationType
		public abstract class durationType : PX.Data.BQL.BqlString.Field<durationType>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Annual, Quarterly, Monthly, Custom },
					new string[] { Messages.Annual, Messages.Quarterly, Messages.Monthly, Messages.Custom }) { ; }
			}
			public const string Monthly = "M";
			public const string Quarterly = "Q";
			public const string Annual = "A";
			public const string Custom = "C";

			public class annual : PX.Data.BQL.BqlString.Constant<annual>
			{
				public annual() : base(Annual) { }
			}
			public class monthly : PX.Data.BQL.BqlString.Constant<monthly>
			{
				public monthly() : base(Monthly) { }
			}
			public class quarterly : PX.Data.BQL.BqlString.Constant<quarterly>
			{
				public quarterly() : base(Quarterly) { }
			}
			public class custom : PX.Data.BQL.BqlString.Constant<custom>
			{
				public custom() : base(Custom) { }
			}
		}
		protected string _DurationType;

		[PXDBString(1, IsFixed = true)]
		[durationType.List]
		[PXDefault(durationType.Annual)]
		[PXUIField(DisplayName = "Duration Unit")]
//		[PXFormula(typeof(Selector<templateID, ContractTemplate.durationType>))]
		public virtual string DurationType
		{
			get
			{
				return this._DurationType;
			}
			set
			{
				this._DurationType = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXDBDate]
		[PXUIField(DisplayName = "Setup Date", Required=true)]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region ActivationDate
		public abstract class activationDate : PX.Data.BQL.BqlDateTime.Field<activationDate> { }
		protected DateTime? _ActivationDate;
		[PXDefault(typeof(startDate), PersistingCheck=PXPersistingCheck.Nothing)]
		[PXDBDate]
		[PXFormula(typeof(Switch<
				Case<Where<Contract.startDate, Greater<Current<Contract.activationDate>>>, Current<Contract.startDate>>,
			Contract.activationDate>))]
		[PXUIField(DisplayName = "Activation Date")]
		public virtual DateTime? ActivationDate
		{
			get
			{
				return this._ActivationDate;
			}
			set
			{
				this._ActivationDate = value;
			}
		}
		#endregion
		#region RenewalBillingStartDate
		public abstract class renewalBillingStartDate : PX.Data.BQL.BqlDateTime.Field<renewalBillingStartDate> { }
		[PXDBDate]
		[PXFormula(typeof(Switch<
			Case<Where<scheduleStartsOn, Equal<scheduleStartsOn.setupDate>>, startDate,
			Case<Where<scheduleStartsOn, Equal<scheduleStartsOn.activationDate>>, activationDate>>>))]
		public virtual DateTime? RenewalBillingStartDate { get; set; }
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[PXDBDate]
		[PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXVerifyEndDate(typeof(Contract.startDate), AllowAutoChange = false)]
		[PXUIEnabled(typeof(Where<status, NotEqual<status.expired>, And<status, NotEqual<status.canceled>, And<type, NotEqual<type.unlimited>>>>))]
		[PXFormula(typeof(ContractExpirationDate<type, durationType, renewalBillingStartDate, duration>))]
		/*
		[PXFormula(typeof(IIf<Where<renewalBillingStartDate, IsNotNull>, ContractExpirationDate<type, durationType, renewalBillingStartDate, duration>, Switch<
			Case<Where<scheduleStartsOn, Equal<scheduleStartsOn.setupDate>>, ContractExpirationDate<type, durationType, startDate, duration>,
			Case<Where<scheduleStartsOn, Equal<scheduleStartsOn.activationDate>>, ContractExpirationDate<type, durationType, activationDate, duration>>>>>))]
		 */
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region TerminationDate
		public abstract class terminationDate : PX.Data.BQL.BqlDateTime.Field<terminationDate> { }
		protected DateTime? _TerminationDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Termination Date", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
		public virtual DateTime? TerminationDate
		{
			get
			{
				return this._TerminationDate;
			}
			set
			{
				this._TerminationDate = value;
			}
		}
		#endregion

		#region GracePeriod
		public abstract class gracePeriod : PX.Data.BQL.BqlInt.Field<gracePeriod> { }
		protected Int32? _GracePeriod;

		/// <summary>
		/// Period in days Contract is serviced even after it has expired. Warning is shown whenever user
		/// selects the contract that falls in this period.
		/// </summary>
		[PXDBInt(MinValue=0, MaxValue=365)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Grace Period")]
		[PXUIEnabled(typeof(Where<type, Equal<type.renewable>, And<Where<EntryStatus, Equal<EntryStatus.inserted>, Or<status, NotEqual<status.canceled>>>>>))]
		public virtual int? GracePeriod
		{
			get
			{
				return _GracePeriod;
			}
			set
			{
				_GracePeriod = value;
			}
		}
		#endregion
		#region GraceDate
		public abstract class graceDate : PX.Data.BQL.BqlDateTime.Field<graceDate> { }

		/// <summary>
		/// End Date of Grace Period.
		/// </summary>
		[PXDBCalced(typeof(Add<Contract.expireDate, Contract.gracePeriod>), typeof(DateTime))]
		public virtual DateTime? GraceDate { get; set; }
		#endregion
		#region AutoRenew
		public abstract class autoRenew : PX.Data.BQL.BqlBool.Field<autoRenew> { }
		protected bool? _AutoRenew;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Mass Renewal")]
		public virtual bool? AutoRenew
		{
			get
			{
				return _AutoRenew;
			}
			set
			{
				_AutoRenew = value;
			}
		}
		#endregion
		#region AutoRenewDays
		public abstract class autoRenewDays : PX.Data.BQL.BqlInt.Field<autoRenewDays> { }
		protected Int32? _AutoRenewDays;
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Renewal Point")]
		public virtual Int32? AutoRenewDays
		{
			get
			{
				return this._AutoRenewDays;
			}
			set
			{
				this._AutoRenewDays = value;
			}
		}
		#endregion
		#region IsTemplate
		[Obsolete(Common.InternalMessages.PropertyIsObsoleteAndWillBeRemoved2019R2)]
		public abstract class isTemplate : PX.Data.BQL.BqlBool.Field<isTemplate> { }
		[Obsolete(Common.InternalMessages.PropertyIsObsoleteAndWillBeRemoved2019R2)]
		[PXDBCalced(typeof(Switch<Case<Where<baseType, Equal<CTPRType.contractTemplate>, Or<baseType, Equal<CTPRType.projectTemplate>>>, True>, False>), typeof(bool))]
		public virtual bool? IsTemplate
		{
			get
			{
				return CTPRType.IsTemplate(BaseType);
			}
			set { }
		}
		#endregion
		#region StrIsTemplate
		public abstract class strIsTemplate : PX.Data.BQL.BqlString.Field<strIsTemplate>
		{
            public const string Contract = "Contract";
            public const string Template = "Template";
        }

        [PXString]
		[PXUIFieldAttribute(DisplayName = "Type")]
        [PXStringList(new string[] { strIsTemplate.Contract, strIsTemplate.Template },
                        new string[] { strIsTemplate.Contract, strIsTemplate.Template })]
        public virtual string StrIsTemplate
		{
			get
			{
				return CTPRType.IsTemplate(BaseType) == true
			                            ? strIsTemplate.Template
			                            : strIsTemplate.Contract;
			}
			
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDefault]
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID))]
		[PXUIField(DisplayName = "Currency")]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region RateTypeID
		public abstract class rateTypeID : PX.Data.BQL.BqlString.Field<rateTypeID> { }
		protected String _RateTypeID;
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(PX.Objects.CM.CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Rate Type", Required=true)]
		[PXDefault(typeof(Search<CM.CMSetup.aRRateTypeDflt>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String RateTypeID
		{
			get
			{
				return this._RateTypeID;
			}
			set
			{
				this._RateTypeID = value;
			}
		}
		#endregion
		#region AllowOverrideCury
		public abstract class allowOverrideCury : PX.Data.BQL.BqlBool.Field<allowOverrideCury> { }
		protected Boolean? _AllowOverrideCury;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Currency Override")]
		public virtual Boolean? AllowOverrideCury
		{
			get
			{
				return this._AllowOverrideCury;
			}
			set
			{
				this._AllowOverrideCury = value;
			}
		}
		#endregion
		#region AllowOverrideRate
		public abstract class allowOverrideRate : PX.Data.BQL.BqlBool.Field<allowOverrideRate> { }
		protected Boolean? _AllowOverrideRate;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Rate Override")]
		public virtual Boolean? AllowOverrideRate
		{
			get
			{
				return this._AllowOverrideRate;
			}
			set
			{
				this._AllowOverrideRate = value;
			}
		}
		#endregion
		#region CalendarID
		public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }
		protected String _CalendarID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Calendar")]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual String CalendarID
		{
			get
			{
				return this._CalendarID;
			}
			set
			{
				this._CalendarID = value;
			}
		}
		#endregion
		#region CreateProforma
		public abstract class createProforma : PX.Data.BQL.BqlBool.Field<createProforma> { }
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Create Pro Forma Invoice on Billing")]
		public virtual Boolean? CreateProforma
		{
			get; set;
		}
		#endregion
		#region AutomaticReleaseAR
		public abstract class automaticReleaseAR : PX.Data.BQL.BqlBool.Field<automaticReleaseAR> { }
		protected Boolean? _AutomaticReleaseAR;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Release AR Documents", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? AutomaticReleaseAR
		{
			get
			{
				return this._AutomaticReleaseAR;
			}
			set
			{
				this._AutomaticReleaseAR = value;
			}
		}
		#endregion
		#region Refundable
		public abstract class refundable : PX.Data.BQL.BqlBool.Field<refundable> { }
		protected Boolean? _Refundable;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck=PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Refundable")]
		public virtual Boolean? Refundable
		{
			get
			{
				return this._Refundable;
			}
			set
			{
				this._Refundable = value;
			}
		}
		#endregion
		#region RefundPeriod
		public abstract class refundPeriod : PX.Data.BQL.BqlInt.Field<refundPeriod> { }
		protected Int32? _RefundPeriod;
		[PXDBInt(MinValue = 0)]
		[PXDefault(0,PersistingCheck=PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Refund Period")]
		public virtual Int32? RefundPeriod
		{
			get
			{
				return this._RefundPeriod;
			}
			set
			{
				this._RefundPeriod = value;
			}
		}
		#endregion
		#region EffectiveFrom
		public abstract class effectiveFrom : PX.Data.BQL.BqlDateTime.Field<effectiveFrom> { }
		protected DateTime? _EffectiveFrom;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective From")]
		public virtual DateTime? EffectiveFrom
		{
			get
			{
				return this._EffectiveFrom;
			}
			set
			{
				this._EffectiveFrom = value;
			}
		}
		#endregion
		#region DiscontinueAfter
		public abstract class discontinueAfter : PX.Data.BQL.BqlDateTime.Field<discontinueAfter> { }
		protected DateTime? _DiscontinueAfter;
		[PXDBDate()]
		[PXUIField(DisplayName = "Discontinue After")]
		public virtual DateTime? DiscontinueAfter
		{
			get
			{
				return this._DiscontinueAfter;
			}
			set
			{
				this._DiscontinueAfter = value;
			}
		}
		#endregion
		#region DiscountID
		public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
		protected String _DiscountID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Promo Code")]
		[PromoDiscIDSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, Equal<DiscountType.LineDiscount>>>))]
		public virtual String DiscountID
		{
			get
			{
				return this._DiscountID;
			}
			set
			{
				this._DiscountID = value;
			}
		}
		public class PromoDiscIDSelectorAttribute : PXCustomSelectorAttribute
		{
			protected BqlCommand _select;
			public PromoDiscIDSelectorAttribute(Type type)
				: base(type)
			{
				this._ViewName = "_SODiscount_LinePromo_";
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				_select = BqlCommand.CreateInstance(typeof(Select5<ARDiscount,
					InnerJoin<DiscountSequence, On<ARDiscount.discountID, Equal<DiscountSequence.discountID>>,
					LeftJoin<DiscountCustomer, On<DiscountCustomer.discountID, Equal<DiscountSequence.discountID>, And<DiscountCustomer.discountSequenceID, Equal<DiscountSequence.discountSequenceID>>>,
					LeftJoin<DiscountItem, On<DiscountItem.discountID, Equal<DiscountSequence.discountID>, And<DiscountItem.discountSequenceID, Equal<DiscountSequence.discountSequenceID>>>,
					LeftJoin<DiscountCustomerPriceClass, On<DiscountCustomerPriceClass.discountID, Equal<DiscountSequence.discountID>, And<DiscountCustomerPriceClass.discountSequenceID, Equal<DiscountSequence.discountSequenceID>>>,
					LeftJoin<DiscountInventoryPriceClass, On<DiscountInventoryPriceClass.discountID, Equal<DiscountSequence.discountID>, And<DiscountInventoryPriceClass.discountSequenceID, Equal<DiscountSequence.discountSequenceID>>>>>>>>,
					Where2<
						Where<ARDiscount.applicableTo, NotEqual<DiscountTarget.customer>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.customerAndInventory>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.customerAndInventoryPrice>, Or<DiscountCustomer.customerID, Equal<Current<Contract.customerID>>>>>>,
					//And2<Where<SODiscount.applicableTo, NotEqual<DiscountTarget.inventory>, And<SODiscount.applicableTo, NotEqual<DiscountTarget.customerAndInventory>, And<SODiscount.applicableTo, NotEqual<DiscountTarget.customerPriceAndInventory>, Or<SODiscountItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>>>,
					//And2<Where<SODiscount.applicableTo, NotEqual<DiscountTarget.customerAndInventoryPrice>, And<SODiscount.applicableTo, NotEqual<DiscountTarget.customerPriceAndInventoryPrice>, And<SODiscount.applicableTo, NotEqual<DiscountTarget.inventoryPrice>, Or<SODiscountInventoryPriceClass.inventoryPriceClassID, Equal<Current<InventoryItem.priceClassID>>>>>>,
						And2<Where<ARDiscount.applicableTo, NotEqual<DiscountTarget.customerPrice>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.customerPriceAndInventory>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.customerPriceAndInventoryPrice>, Or<DiscountCustomerPriceClass.customerPriceClassID, Equal<Current<CR.Location.cPriceClassID>>>>>>,
						And<ARDiscount.applicableTo, NotEqual<DiscountTarget.branch>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouse>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndCustomer>, 
						And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndCustomerPrice>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndInventory>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndInventoryPrice>,
						And<DiscountSequence.isActive, Equal<True>,
						And<ARDiscount.type, Equal<DiscountType.LineDiscount>,
						And<Where<DiscountSequence.isPromotion, Equal<False>, Or<Current<Contract.startDate>, Between<DiscountSequence.startDate, DiscountSequence.endDate>>>>>>>>>>>>>>,
					Aggregate<GroupBy<ARDiscount.discountID>>>));
			}

			public virtual IEnumerable GetRecords()
			{
				Location item = PXSelect<Location, Where<Location.bAccountID, Equal<Current<Contract.customerID>>, And<Location.locationID, Equal<Current<Contract.locationID>>>>>.Select(_Graph);

				PXView view = _Graph.TypedViews.GetView(_select, true);
				return view.SelectMultiBound(new object[] { item });
			}

			public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (!string.IsNullOrEmpty((string)e.NewValue))
				{
					if (PXSelect<ARDiscount, Where<ARDiscount.discountID, Equal<Required<ARDiscount.discountID>>, And<ARDiscount.type, Equal<DiscountType.LineDiscount>>>>.Select(sender.Graph, e.NewValue).Count == 0)
					{
						throw new PXSetPropertyException(ErrorMessages.ElementDoesntExist, e.NewValue);
					}
				}
			}
		}
		#endregion
		#region DetailedBilling
		public abstract class detailedBilling : PX.Data.BQL.BqlInt.Field<detailedBilling>
		{
			public const int Summary = 0;
			public const int Detail = 1; 
		}
		protected Int32? _DetailedBilling;
		[PXDBInt()]
		[PXDefault(detailedBilling.Summary)]
		[PXIntList(new int[] {detailedBilling.Summary, detailedBilling.Detail}, new string[] { Messages.Summary, Messages.Detail })]
		[PXUIField(DisplayName = "Billing Format")]
		public virtual Int32? DetailedBilling
		{
			get
			{
				return this._DetailedBilling;
			}
			set
			{
				this._DetailedBilling = value;
			}
		}
		#endregion
		#region AllowOverride
		public abstract class allowOverride : PX.Data.BQL.BqlBool.Field<allowOverride> { }
		protected Boolean? _AllowOverride;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Template Item Override")]
		public virtual Boolean? AllowOverride
		{
			get
			{
				return _AllowOverride;
			}
			set
			{
				_AllowOverride = value;
			}
		}
		#endregion
		#region RefreshOnRenewal
		public abstract class refreshOnRenewal : PX.Data.BQL.BqlBool.Field<refreshOnRenewal> { }
		protected Boolean? _RefreshOnRenewal;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Refresh Items from Template on Renewal")]
		public virtual Boolean? RefreshOnRenewal
		{
			get
			{
				return this._RefreshOnRenewal;
			}
			set
			{
				this._RefreshOnRenewal = value;
			}
		}
		#endregion
		#region IsContinuous
		public abstract class isContinuous : PX.Data.BQL.BqlBool.Field<isContinuous> { }
		protected Boolean? _IsContinuous;
		[PXDBBool()]
		[PXUIField(DisplayName = "Shift Expire Date on Renew", Visibility = PXUIVisibility.Invisible)]
		[PXDefault(true)]
		public virtual Boolean? IsContinuous
		{
			get
			{
				return this._IsContinuous;
			}
			set
			{
				this._IsContinuous = value;
			}
		}
		#endregion
		#region DefaultSalesAccountID
		public abstract class defaultSalesAccountID : PX.Data.BQL.BqlInt.Field<defaultSalesAccountID> { }

		/// <summary>
		/// The identifier of the <see cref="Account">default sales account</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Default Sales Account", AvoidControlAccounts = true)]
		public virtual Int32? DefaultSalesAccountID
		{
			get;
			set;
		}
		#endregion
		#region DefaultSalesSubID
		public abstract class defaultSalesSubID : PX.Data.BQL.BqlInt.Field<defaultSalesSubID> { }
		[SubAccount(DisplayName = "Default Sales Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DefaultSalesSubID
		{
			get;
			set;
		}
		#endregion
		#region DefaultExpenseAccountID
		public abstract class defaultExpenseAccountID : PX.Data.BQL.BqlInt.Field<defaultExpenseAccountID> { }

		/// <summary>
		/// The identifier of the <see cref="Account">default cost account</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Default Cost Account", AvoidControlAccounts = true)]
		public virtual Int32? DefaultExpenseAccountID
		{
			get;
			set;
		}
		#endregion
		#region DefaultExpenseSubID
		public abstract class defaultExpenseSubID : PX.Data.BQL.BqlInt.Field<defaultExpenseSubID> { }
		[SubAccount(DisplayName = "Default Cost Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DefaultExpenseSubID
		{
			get;
			set;
		}
		#endregion
		#region DefaultAccrualAccountID
		public abstract class defaultAccrualAccountID : PX.Data.BQL.BqlInt.Field<defaultAccrualAccountID> { }
		protected Int32? _DefaultAccrualAccountID;

		/// <summary>
		/// The identifier of the <see cref="Account">default accrual account</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Accrual Account", AvoidControlAccounts = true)]
		public virtual Int32? DefaultAccrualAccountID
		{
			get
			{
				return this._DefaultAccrualAccountID;
			}
			set
			{
				this._DefaultAccrualAccountID = value;
			}
		}
		#endregion
		#region DefaultSubID
		public abstract class defaultAccrualSubID : PX.Data.BQL.BqlInt.Field<defaultAccrualSubID> { }
		protected Int32? _DefaultAccrualSubID;
		[SubAccount(DisplayName = "Accrual Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DefaultAccrualSubID
		{
			get
			{
				return this._DefaultAccrualSubID;
			}
			set
			{
				this._DefaultAccrualSubID = value;
			}
		}
		#endregion
		#region DefaultBranchID
		public abstract class defaultBranchID : PX.Data.BQL.BqlInt.Field<defaultBranchID> { }
		protected Int32? _DefaultBranchID;

		/// <summary>
		/// This field is used in the <see cref="PMProject"/> class.
		/// The identifier of the <see cref="Branch"/> associated with the project.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Branch(DisplayName = "Branch", IsDetail = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? DefaultBranchID
		{
			get
			{
				return this._DefaultBranchID;
			}
			set
			{
				this._DefaultBranchID = value;
			}
		}
		#endregion
		#region QuoteNbr
		public abstract class quoteNbr : PX.Data.BQL.BqlString.Field<quoteNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXSelector(typeof(Search2<PMQuote.quoteNbr,
					LeftJoin<BAccount, On<BAccount.bAccountID, Equal<PMQuote.bAccountID>>,
					LeftJoin<Contact, On<Contact.contactID, Equal<PMQuote.contactID>>>>,
				Where<PMQuote.quoteType, Equal<CRQuoteTypeAttribute.project>>,
				OrderBy<Desc<PMQuote.quoteNbr>>>),
			new[] {
				typeof(PMQuote.quoteNbr),
				typeof(PMQuote.status),
				typeof(PMQuote.subject),
				typeof(BAccount.acctCD),
				typeof(PMQuote.documentDate),
				typeof(PMQuote.expirationDate)
			 },
			Filterable = true)]
		[PXUIField(DisplayName = "Quote Ref. Nbr.", FieldClass = nameof(FeaturesSet.ProjectQuotes))]
		[PXFieldDescription]
		public virtual String QuoteNbr { get; set; }
		#endregion
		#region RestrictToEmployeeList
		public abstract class restrictToEmployeeList : PX.Data.BQL.BqlBool.Field<restrictToEmployeeList> { }
		protected Boolean? _RestrictToEmployeeList;

		/// <summary>
		/// This field is used in the <see cref="PMProject"/> class.
		/// Specifies (if set to <see langword="true" />) that only the employees associated with the current
		/// <see cref="PMProject">project</see> can create activities and documents that is associated with the current project.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Restrict Employees")]
		public virtual Boolean? RestrictToEmployeeList
		{
			get
			{
				return this._RestrictToEmployeeList;
			}
			set
			{
				this._RestrictToEmployeeList = value;
			}
		}
		#endregion
		#region RestrictToResourceList
		public abstract class restrictToResourceList : PX.Data.BQL.BqlBool.Field<restrictToResourceList> { }
		protected Boolean? _RestrictToResourceList;

		/// <summary>
		/// This field is used in the <see cref="PMProject"/> class.
		/// Specifies (if set to <see langword="true" />) that equipment time cards can be associated with the
		/// current <see cref="PMProject">project</see> for only the equipment that is associated with the current project.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Restrict Equipment")]
		public virtual Boolean? RestrictToResourceList
		{
			get
			{
				return this._RestrictToResourceList;
			}
			set
			{
				this._RestrictToResourceList = value;
			}
		}
		#endregion
		#region ApproverID
		public abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }
		protected Int32? _ApproverID;
		[PXDBInt]
		[EP.PXEPEmployeeSelector]
		[PXUIField(DisplayName = "Project Manager", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? ApproverID
		{
			get
			{
				return this._ApproverID;
			}
			set
			{
				this._ApproverID = value;
			}
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;

		/// <summary>
		/// The workgroup that is responsible for the document.
		/// </summary>
		[PXInt]
		[PXFormula(typeof(Selector<Contract.customerID, BAccount.workgroupID>))]
		public virtual int? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		protected int? _OwnerID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Owner(typeof(Contract.workgroupID), Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[SalesPerson(DescriptionField = typeof(SalesPerson.descr), DisplayName = "Sales Person")]
		[PXForeignReference(typeof(Field<Contract.salesPersonID>.IsRelatedTo<SalesPerson.salesPersonID>))]
		public virtual Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion
		#region ScheduleStartsOn
		public abstract class scheduleStartsOn : PX.Data.BQL.BqlString.Field<scheduleStartsOn>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { SetupDate, ActivationDate },
					new string[] { Messages.SetupDate, Messages.ActivationDate }) { ; }
			}

			public const string SetupDate = "S";
			public const string ActivationDate = "A";

			public class setupDate : PX.Data.BQL.BqlString.Constant<setupDate>
			{
				public setupDate() : base(SetupDate) {}
			}
			public class activationDate : PX.Data.BQL.BqlString.Constant<activationDate>
			{
				public activationDate() : base(ActivationDate) {}
			}
		}
		protected String _ScheduleStartsOn;
		[PXDefault(scheduleStartsOn.ActivationDate, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(1, IsFixed = true)]
		[scheduleStartsOn.List]
		[PXUIField(DisplayName = "Billing Schedule Starts On")]
		public virtual String ScheduleStartsOn
		{
			get
			{
				return this._ScheduleStartsOn;
			}
			set
			{
				this._ScheduleStartsOn = value;
			}
		}
		#endregion

		#region AccountingMode

		public abstract class accountingMode : Data.BQL.BqlBool.Field<accountingMode> { }

		[PXDBString(1)]
		[PXDefault("P")]
		public virtual string AccountingMode { get; set; }

		#endregion
		#region PrepaymentEnabled
		public abstract class prepaymentEnabled : PX.Data.BQL.BqlBool.Field<prepaymentEnabled> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Prepaid Amount", Visible = false)]
		public virtual Boolean? PrepaymentEnabled
		{
			get; set;
		}
		#endregion
		#region PrepaymentDefCode
		public abstract class prepaymentDefCode : PX.Data.BQL.BqlString.Field<prepaymentDefCode> { }
		[PXSelector(typeof(Search<DR.DRDeferredCode.deferredCodeID, Where<DR.DRDeferredCode.accountType, Equal<DR.DeferredAccountType.income>>>), DescriptionField=typeof(DR.DRDeferredCode.description))]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Deferral Code", FieldClass = "DEFFERED", Visible = false)]
		public virtual String PrepaymentDefCode
		{
			get;
			set;
		}
		#endregion
		#region LimitsEnabled
		public abstract class limitsEnabled : PX.Data.BQL.BqlBool.Field<limitsEnabled> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use T&M Revenue Budget Limits")]
		public virtual Boolean? LimitsEnabled
		{
			get; set;
		}
		#endregion
		#region ChangeOrderWorkflow
		public abstract class changeOrderWorkflow : PX.Data.BQL.BqlBool.Field<changeOrderWorkflow> { }
		[PXDBBool()]
		[PXDefault(typeof(Search<FeaturesSet.changeOrder>))]
		[PXUIField(DisplayName = "Change Order Workflow", FieldClass = PM.PMChangeOrder.FieldClass)]
		public virtual Boolean? ChangeOrderWorkflow
		{
			get; set;
		}
		#endregion
		#region LockCommitments
		public abstract class lockCommitments : PX.Data.BQL.BqlBool.Field<lockCommitments> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? LockCommitments
		{
			get; set;
		}
		#endregion
		#region BudgetMetricsEnabled
		public abstract class budgetMetricsEnabled : PX.Data.BQL.BqlBool.Field<budgetMetricsEnabled> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Track Production Data")]
		public virtual Boolean? BudgetMetricsEnabled
		{
			get; set;
		}
		#endregion
		#region CertifiedJob
		public abstract class certifiedJob : PX.Data.BQL.BqlBool.Field<certifiedJob> { }

		/// <summary>
		/// This field is used in the <see cref="PMProject"/> class.
		/// Specifies (if set to <see langword="true" />) that the job performed in the scope of the <see cref="PMProject">project</see>
		/// is a certified job performed for the government, such as a hospital construction project.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Certified Job", FieldClass = nameof(FeaturesSet.Construction))]
		public virtual Boolean? CertifiedJob
		{
			get; set;
		}
		#endregion
		#region IncludeQtyInAIA
		public abstract class includeQtyInAIA : PX.Data.BQL.BqlBool.Field<includeQtyInAIA> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Show Quantity in AIA Report", FieldClass = nameof(FeaturesSet.Construction))]
		public bool? IncludeQtyInAIA
		{
			get; set;
		}
		#endregion
		#region AIALevel
		public abstract class aIALevel : PX.Data.BQL.BqlString.Field<aIALevel>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Summary, Detail },
					new string[] { Messages.Summary, Messages.Detail })
				{ }
			}
			public const string Summary = "S";
			public const string Detail = "D";
		
			public class summary : PX.Data.BQL.BqlString.Constant<summary>
			{
				public summary() : base(Summary) { }
			}
			public class detail : PX.Data.BQL.BqlString.Constant<detail>
			{
				public detail() : base(Detail) { }
			}			
		}
		[PXDBString(1, IsFixed = true)]
		[aIALevel.List]
		[PXDefault(aIALevel.Summary)]
		[PXUIField(DisplayName = "AIA Level", FieldClass = nameof(FeaturesSet.Construction))]
		public virtual string AIALevel
		{
			get;
			set;
		}
		#endregion
		#region BillingID
		public abstract class billingID : PX.Data.BQL.BqlString.Field<billingID> { }
		protected String _BillingID;
		[PXDBString(15, IsUnicode = true)]
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
		#region AllocationID
		public abstract class allocationID : PX.Data.BQL.BqlString.Field<allocationID> { }
		protected String _AllocationID;
		[PXDBString(15, IsUnicode = true)]
		public virtual String AllocationID
		{
			get
			{
				return this._AllocationID;
			}
			set
			{
				this._AllocationID = value;
			}
		}
		#endregion
		#region ContractAccountGroup
		public abstract class contractAccountGroup : PX.Data.BQL.BqlInt.Field<contractAccountGroup> { }
		protected int? _ContractAccountGroup;
		[PXSelector(typeof(Search<PMAccountGroup.groupID, Where<PMAccountGroup.type, Equal<PMAccountType.offBalance>>>), SubstituteKey=typeof(PMAccountGroup.groupCD))]
		[PXUIField(DisplayName = "Account Group")]
		[PXDBInt]
		public virtual int? ContractAccountGroup
		{
			get
			{
				return this._ContractAccountGroup;
			}
			set
			{
				this._ContractAccountGroup = value;
			}
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		
		/// <summary>
		/// The identifier of the <see cref="Terms">Credit Terms</see> object associated with the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Customer.TermsID">credit terms</see> that are selected for the <see cref="CustomerID">customer</see>.
		/// Corresponds to the <see cref="Terms.TermsID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<Contract.customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms")]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		public virtual String TermsID
		{
			get;
			set;
		}
		#endregion
		#region LastChangeOrderNumber
		public abstract class lastChangeOrderNumber : PX.Data.BQL.BqlString.Field<lastChangeOrderNumber> { }
		[PXDBString(PMChangeOrder.projectNbr.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Last Revenue Change Nbr.", FieldClass = PMChangeOrder.FieldClass)]
		public virtual String LastChangeOrderNumber
		{
			get;
			set;
		}
		#endregion
		#region LastProformaNumber
		public abstract class lastProformaNumber : PX.Data.BQL.BqlString.Field<lastProformaNumber> { }

		/// <summary>
		/// This field is used in the <see cref="PMProject"/> class.
		/// The application number that has been assigned to the last <see cref="PMProforma">pro forma invoice</see>
		/// with progress billing lines prepared for the current <see cref="PMProject">project</see>.
		/// </summary>
		[PXDBString(PMProforma.projectNbr.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Last Application Nbr.", FieldClass = nameof(FeaturesSet.Construction))]
		public virtual String LastProformaNumber
		{
			get;
			set;
		}
		#endregion
		#region RetainagePct
		public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct> { }
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion
		#region RetainageMaxPct
		/// <exclude/>
		public abstract class retainageMaxPct : PX.Data.BQL.BqlDecimal.Field<retainageMaxPct>
		{
		}
		/// <summary>
		/// Retainage Cap %
		/// </summary>
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cap (%)", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? RetainageMaxPct
		{
			get;
			set;
		}
		#endregion
		#region RetainageMode
		/// <exclude/>
		public abstract class retainageMode : PX.Data.BQL.BqlString.Field<retainageMode> { }
		/// <summary>
		/// Retainage Mode
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[RetainageModes.List]
		[PXDefault(RetainageModes.Normal)]
		[PXUIField(DisplayName = "Retainage Mode", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual String RetainageMode { get; set; }
		#endregion
		#region IncludeCO
		/// <exclude/>
		public abstract class includeCO : PX.Data.BQL.BqlBool.Field<includeCO> { }
		/// <summary>
		/// Include Change Orders in Contract Total
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include CO", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Boolean? IncludeCO
		{
			get;
			set;
		}
		#endregion
		#region SteppedRetainage
		/// <exclude/>
		public abstract class steppedRetainage : PX.Data.BQL.BqlBool.Field<steppedRetainage> { }
		/// <summary>
		/// Stepped Retainage
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Steps", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Boolean? SteppedRetainage
		{
			get;
			set;
		}
		#endregion
		#region SiteAddress
		public abstract class siteAddress : PX.Data.BQL.BqlString.Field<siteAddress> { }
		[Obsolete]
		[PXDBLocalizableString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Site Address", FieldClass = nameof(FeaturesSet.Construction))]
		public virtual String SiteAddress
		{
			get;
			set;
		}
		#endregion

		#region PendingSetup
		public abstract class pendingSetup : PX.Data.BQL.BqlDecimal.Field<pendingSetup> { }
		protected Decimal? _PendingSetup;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName="Pending Setup", Enabled=false)]
		public virtual Decimal? PendingSetup
		{
			get
			{
				return this._PendingSetup;
			}
			set
			{
				this._PendingSetup = value;
			}
		}
		#endregion
		#region PendingRecurring
		public abstract class pendingRecurring : PX.Data.BQL.BqlDecimal.Field<pendingRecurring> { }
		protected Decimal? _PendingRecurring;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Pending Recurring",Enabled=false)]
		public virtual Decimal? PendingRecurring
		{
			get
			{
				return this._PendingRecurring;
			}
			set
			{
				this._PendingRecurring = value;
			}
		}
		#endregion
		#region PendingRenewal
		public abstract class pendingRenewal : PX.Data.BQL.BqlDecimal.Field<pendingRenewal> { }
		protected Decimal? _PendingRenewal;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Pending Renewal", Enabled=false)]
		public virtual Decimal? PendingRenewal
		{
			get
			{
				return this._PendingRenewal;
			}
			set
			{
				this._PendingRenewal = value;
			}
		}
		#endregion
		#region TotalPending
		public abstract class totalPending : PX.Data.BQL.BqlDecimal.Field<totalPending> { }
		protected Decimal? _TotalPending;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Add<Contract.pendingRecurring, Add<Contract.pendingRenewal, Contract.pendingSetup>>))]
		[PXUIField(DisplayName = "Total Pending", Enabled=false)]
		public virtual Decimal? TotalPending
		{
			get
			{
				return this._TotalPending;
			}
			set
			{
				this._TotalPending = value;
			}
		}
		#endregion

		#region CurrentSetup
		public abstract class currentSetup : PX.Data.BQL.BqlDecimal.Field<currentSetup> { }
		protected Decimal? _CurrentSetup;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Current Setup",Enabled=false)]
		public virtual Decimal? CurrentSetup
		{
			get
			{
				return this._CurrentSetup;
			}
			set
			{
				this._CurrentSetup = value;
			}
		}
		#endregion
		#region CurrentRecurring
		public abstract class currentRecurring : PX.Data.BQL.BqlDecimal.Field<currentRecurring> { }
		protected Decimal? _CurrentRecurring;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Current Recurring", Enabled=false)]
		public virtual Decimal? CurrentRecurring
		{
			get
			{
				return this._CurrentRecurring;
			}
			set
			{
				this._CurrentRecurring = value;
			}
		}
		#endregion
		#region CurrentRenewal
		public abstract class currentRenewal : PX.Data.BQL.BqlDecimal.Field<currentRenewal> { }
		protected Decimal? _CurrentRenewal;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Current Renewal",Enabled=false)]
		public virtual Decimal? CurrentRenewal
		{
			get
			{
				return this._CurrentRenewal;
			}
			set
			{
				this._CurrentRenewal = value;
			}
		}
		#endregion

		#region TotalsCalculated
		public abstract class totalsCalculated : PX.Data.BQL.BqlInt.Field<totalsCalculated> { }
		[PXInt()]
		public virtual int? TotalsCalculated
		{
			get;
			set;
		}
		#endregion
		#region TotalRecurring
		public abstract class totalRecurring : PX.Data.BQL.BqlDecimal.Field<totalRecurring> { }
		protected Decimal? _TotalRecurring;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Recurring Total", Enabled = false)]
		public virtual Decimal? TotalRecurring
		{
			get
			{
				return this._TotalRecurring;
			}
			set
			{
				this._TotalRecurring = value;
			}
		}
		#endregion
		#region TotalUsage
		public abstract class totalUsage : PX.Data.BQL.BqlDecimal.Field<totalUsage> { }
		protected Decimal? _TotalUsage;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Extra Usage Total", Enabled = false)]
		public virtual Decimal? TotalUsage
		{
			get
			{
				return this._TotalUsage;
			}
			set
			{
				this._TotalUsage = value;
			}
		}
		#endregion
		#region TotalDue
		public abstract class totalDue : PX.Data.BQL.BqlDecimal.Field<totalDue> { }
		protected Decimal? _TotalDue;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Total Due", Enabled = false)]
		public virtual Decimal? TotalDue
		{
			get
			{
				return this._TotalDue;
			}
			set
			{
				this._TotalDue = value;
			}
		}
		#endregion
		
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		[PXNoUpdate]
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
		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
		protected Boolean? _Approved;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Approved", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Boolean? Approved
		{
			get
			{
				return this._Approved;
			}
			set
			{
				this._Approved = value;
			}
		}
		#endregion
		#region Rejected
		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
		protected bool? _Rejected = false;
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Reject", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual bool? Rejected
		{
			get
			{
				return _Rejected;
			}
			set
			{
				_Rejected = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected Boolean? _IsActive;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region IsCompleted
		public abstract class isCompleted : PX.Data.BQL.BqlBool.Field<isCompleted> { }
		protected Boolean? _IsCompleted;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsCompleted
		{
			get
			{
				return this._IsCompleted;
			}
			set
			{
				this._IsCompleted = value;
			}
		}
		#endregion
		#region IsCancelled
		public abstract class isCancelled : PX.Data.BQL.BqlBool.Field<isCancelled> { }
		protected Boolean? _IsCancelled;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsCancelled
		{
			get
			{
				return this._IsCancelled;
			}
			set
			{
				this._IsCancelled = value;
			}
		}
		#endregion
		#region IsPendingUpdate
		public abstract class isPendingUpdate : PX.Data.BQL.BqlBool.Field<isPendingUpdate> { }
		protected Boolean? _IsPendingUpdate;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsPendingUpdate
		{
			get
			{
				return this._IsPendingUpdate;
			}
			set
			{
				this._IsPendingUpdate = value;
			}
		}
		#endregion
		#region AutoAllocate
		public abstract class autoAllocate : PX.Data.BQL.BqlBool.Field<autoAllocate> { }
		protected Boolean? _AutoAllocate;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? AutoAllocate
		{
			get
			{
				return this._AutoAllocate;
			}
			set
			{
				this._AutoAllocate = value;
			}
		}
		#endregion
		#region IsLastActionUndoable
		public abstract class isLastActionUndoable : PX.Data.BQL.BqlBool.Field<isLastActionUndoable> { }
		protected Boolean? _IsLastActionUndoable;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsLastActionUndoable
		{
			get
			{
				return this._IsLastActionUndoable;
			}
			set
			{
				this._IsLastActionUndoable = value;
			}
		}
		#endregion

		#region VisibleInGL
		public abstract class visibleInGL : PX.Data.BQL.BqlBool.Field<visibleInGL> { }
		protected Boolean? _VisibleInGL;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInGL
		{
			get
			{
				return this._VisibleInGL;
			}
			set
			{
				this._VisibleInGL = value;
			}
		}
		#endregion
		#region VisibleInAP
		public abstract class visibleInAP : PX.Data.BQL.BqlBool.Field<visibleInAP> { }
		protected Boolean? _VisibleInAP;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInAP
		{
			get
			{
				return this._VisibleInAP;
			}
			set
			{
				this._VisibleInAP = value;
			}
		}
		#endregion
		#region VisibleInAR
		public abstract class visibleInAR : PX.Data.BQL.BqlBool.Field<visibleInAR> { }
		protected Boolean? _VisibleInAR;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInAR
		{
			get
			{
				return this._VisibleInAR;
			}
			set
			{
				this._VisibleInAR = value;
			}
		}
		#endregion
		#region VisibleInSO
		public abstract class visibleInSO : PX.Data.BQL.BqlBool.Field<visibleInSO> { }
		protected Boolean? _VisibleInSO;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInSO
		{
			get
			{
				return this._VisibleInSO;
			}
			set
			{
				this._VisibleInSO = value;
			}
		}
		#endregion
		#region VisibleInPO
		public abstract class visibleInPO : PX.Data.BQL.BqlBool.Field<visibleInPO> { }
		protected Boolean? _VisibleInPO;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInPO
		{
			get
			{
				return this._VisibleInPO;
			}
			set
			{
				this._VisibleInPO = value;
			}
		}
		#endregion
		
		#region VisibleInTA
		public abstract class visibleInTA : PX.Data.BQL.BqlBool.Field<visibleInTA> { }
		protected Boolean? _VisibleInTA;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInTA
		{
			get
			{
				return this._VisibleInTA;
			}
			set
			{
				this._VisibleInTA = value;
			}
		}
		#endregion
		#region VisibleInEA
		public abstract class visibleInEA : PX.Data.BQL.BqlBool.Field<visibleInEA> { }
		protected Boolean? _VisibleInEA;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInEA
		{
			get
			{
				return this._VisibleInEA;
			}
			set
			{
				this._VisibleInEA = value;
			}
		}
		#endregion
		#region VisibleInIN
		public abstract class visibleInIN : PX.Data.BQL.BqlBool.Field<visibleInIN> { }
		protected Boolean? _VisibleInIN;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInIN
		{
			get
			{
				return this._VisibleInIN;
			}
			set
			{
				this._VisibleInIN = value;
			}
		}
		#endregion
		#region VisibleInCA
		public abstract class visibleInCA : PX.Data.BQL.BqlBool.Field<visibleInCA> { }
		protected Boolean? _VisibleInCA;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "CA")]
		public virtual Boolean? VisibleInCA
		{
			get
			{
				return this._VisibleInCA;
			}
			set
			{
				this._VisibleInCA = value;
			}
		}
		#endregion
		#region VisibleInCR
		public abstract class visibleInCR : PX.Data.BQL.BqlBool.Field<visibleInCR> { }
		protected Boolean? _VisibleInCR;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "CRM")]
		public virtual Boolean? VisibleInCR
		{
			get
			{
				return this._VisibleInCR;
			}
			set
			{
				this._VisibleInCR = value;
			}
		}
		#endregion
		#region NonProject
		public abstract class nonProject : PX.Data.BQL.BqlBool.Field<nonProject> { }
		protected Boolean? _NonProject;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? NonProject
		{
			get
			{
				return this._NonProject;
			}
			set
			{
				this._NonProject = value;
			}
		}
		#endregion
		#region RevID
		public abstract class revID : PX.Data.BQL.BqlInt.Field<revID> { }
		[PXDBInt(MinValue = 1)]
		[PXDefault(1, PersistingCheck = PXPersistingCheck.Null)]
		public virtual int? RevID { get; set; }
		#endregion
		#region LastActiveRevID
		public abstract class lastActiveRevID : PX.Data.BQL.BqlInt.Field<lastActiveRevID> { }
		[PXDBInt(MinValue = 1)]
		public virtual int? LastActiveRevID { get; set; }
		#endregion
		#region LineCtr
		public abstract class lineCtr : PX.Data.BQL.BqlInt.Field<lineCtr> { }
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? LineCtr { get; set; }
		#endregion
		#region BillingLineCntr
		public abstract class billingLineCntr : PX.Data.BQL.BqlInt.Field<billingLineCntr> { }
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? BillingLineCntr { get; set; }
		#endregion
		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.AR, Messages.ContractSearchTitle, new Type[] { typeof(Contract.contractCD), typeof(Contract.customerID), typeof(BAccount.acctName) },
		   new Type[] { typeof(Contract.description) },
		   NumberFields = new Type[] { typeof(Contract.contractCD) },
		   Line1Format = "{0}{1}{2}", Line1Fields = new Type[] { typeof(Contract.templateID), typeof(Contract.status), typeof(Contract.type) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(Contract.description) },
		   WhereConstraint = typeof(Where<Current<Contract.baseType>, Equal<CTPRType.contract>,
									   Or<Current<Contract.baseType>, Equal<CTPRType.contractTemplate>>>),
		   MatchWithJoin = typeof(LeftJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>)
		)]
		[PXNote(DescriptionField = typeof(Contract.contractCD))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion

		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;
		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
		#endregion

		#region Labels
		#region DaysBeforeExpiration
		public abstract class daysBeforeExpiration : PX.Data.BQL.BqlString.Field<daysBeforeExpiration> { }
		protected String _DaysBeforeExpiration;
		[PXString(IsUnicode = true)]
		[PXUIField]
		public virtual string DaysBeforeExpiration
		{
			get
            {
                return PXLocalizer.Localize(Messages.Labels_DaysBeforeExpiration, typeof(Messages).FullName);
            }
		}
		#endregion
		#region Days
		public abstract class days : PX.Data.BQL.BqlString.Field<days> { }
		protected String _Days;
		[PXString(IsUnicode = true)]
		[PXUIField]
		public virtual string Days
		{
			get
            {
                return PXLocalizer.Localize(Messages.Labels_Days, typeof(Messages).FullName);
            }
		}
		#endregion
		#region Min
		public abstract class min : PX.Data.BQL.BqlString.Field<min> { }
		protected String _Min;
		[PXString]
		[PXUIField]
		public virtual string Min
		{
			get
            {
                return PXLocalizer.Localize(Messages.Labels_Min, typeof(Messages).FullName);
            }
		}
		#endregion
		#endregion

		#region ServiceActivate
		public abstract class serviceActivate : PX.Data.BQL.BqlBool.Field<serviceActivate> { }
		protected Boolean? _ServiceActivate;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? ServiceActivate
		{
			get
			{
				return this._ServiceActivate;
			}
			set
			{
				this._ServiceActivate = value;
			}
		}
		#endregion
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		/// <summary>
		/// The string size is not restricted because the field is mapped to an unrestricted field with <see cref="PXDimensionSelectorAttribute"/>.
		/// </summary>
		[PXString]
		[PXFormula(typeof(Selector<Contract.templateID, ContractTemplate.contractCD>))]
		[PXSelector(typeof(Search<ContractTemplate.contractCD, Where<ContractTemplate.baseType, Equal<CTPRType.contractTemplate>>>))]
		public virtual string ClassID { get; set; }
		#endregion
		#region Attributes
		public abstract class attributes : BqlAttributes.Field<attributes> { }
		[CRAttributesField(typeof(Contract.templateID))]
		public virtual string[] Attributes { get; set; }
		#endregion

		#region DropshipExpenseAccountSource
		public abstract class dropshipExpenseAccountSource : PX.Data.BQL.BqlString.Field<dropshipExpenseAccountSource> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(DropshipExpenseAccountSourceOption.PostingClassOrItem)]
		public virtual String DropshipExpenseAccountSource
		{
			get;
			set;
		}
		#endregion
		#region DropshipExpenseSubMask
		public abstract class dropshipExpenseSubMask : PX.Data.BQL.BqlString.Field<dropshipExpenseSubMask> { }
		[PXDefault(typeof(PMSetup.dropshipExpenseSubMask), PersistingCheck = PXPersistingCheck.Nothing)]
		[DropshipExpenseSubAccountMaskAttribute()]
		public virtual String DropshipExpenseSubMask
		{
			get;
			set;
		}
		#endregion
		#region DropshipReceiptProcessing
		public abstract class dropshipReceiptProcessing : PX.Data.BQL.BqlString.Field<dropshipReceiptProcessing> { }
		[PXDBString(1)]
		[PXDefault(DropshipReceiptProcessingOption.GenerateReceipt)]
		public virtual String DropshipReceiptProcessing
		{
			get;
			set;
		}
		#endregion
		#region DropshipExpenseRecording
		public abstract class dropshipExpenseRecording : PX.Data.BQL.BqlString.Field<dropshipExpenseRecording> { }
		[PXDBString(1)]
		[PXDefault(DropshipExpenseRecordingOption.OnBillRelease)]
		public virtual String DropshipExpenseRecording
		{
			get;
			set;
		}
		#endregion

		#region CostTaxZoneID
		public abstract class costTaxZoneID : PX.Data.BQL.BqlString.Field<costTaxZoneID> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual String CostTaxZoneID
		{
			get;
			set;
		}
		#endregion
		#region RevenueTaxZoneID
		public abstract class revenueTaxZoneID : PX.Data.BQL.BqlString.Field<revenueTaxZoneID> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual String RevenueTaxZoneID
		{
			get;
			set;
		}
		#endregion
	}


	public static class CTRoundingTarget
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { None, Case, Activity },
				new string[] { Messages.None, Messages.Case, Messages.Activity }) { ; }
		}

		public const string None = "N";
		public const string Case = "C";
		public const string Activity = "A";
	}
}

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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.TM;
using System;
using Branch = PX.Objects.GL.Branch;

namespace PX.Objects.EP
{
	/// <summary>
	/// Represents an Employee of the organization utilizing Acumatica ERP.
	/// </summary>
	/// <remarks>
	/// An employee is a person working for the organization that utilizes Acumatica ERP.
	/// The records of this type are created and edited on the <i>Employees (EP203000)</i> form,
	/// which correspond to the <see cref="EmployeeMaint"/> graph.
	/// EPEmployee is a child of <see cref="BAccount"/> class.
	/// </remarks>
	[System.SerializableAttribute()]
	[PXTable(typeof(PX.Objects.CR.BAccount.bAccountID))]
	[PXCacheName(Messages.Employee)]
	[PXPrimaryGraph(typeof(EmployeeMaint))]
	public partial class EPEmployee : Vendor
	{
		#region Keys
		/// <summary>
		/// Primary Key
		/// </summary>
		public new class PK : PrimaryKeyOf<EPEmployee>.By<bAccountID>
		{
			public static EPEmployee Find(PXGraph graph, int? bAccountID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bAccountID, options);
		}
		/// <summary>
		/// Unique Key
		/// </summary>
		public new class UK : PrimaryKeyOf<EPEmployee>.By<acctCD>
		{
			public static EPEmployee Find(PXGraph graph, string acctCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, acctCD, options);
		}
		/// <summary>
		/// Foreign Keys
		/// </summary>
		public new static class FK
		{
			/// <summary>
			/// Customer Class
			/// </summary>
			public class Class : CR.CRCustomerClass.PK.ForeignKeyOf<EPEmployee>.By<classID> { }
			/// <summary>
			/// Branch or location
			/// </summary>
			public class ParentBusinessAccount : CR.BAccount.PK.ForeignKeyOf<EPEmployee>.By<parentBAccountID> { }

			/// <summary>
			/// Address
			/// </summary>
			public class Address : CR.Address.PK.ForeignKeyOf<EPEmployee>.By<defAddressID> { }
			/// <summary>
			/// Contact
			/// </summary>
			public class ContactInfo : CR.Contact.PK.ForeignKeyOf<EPEmployee>.By<defContactID> { }
			/// <summary>
			/// Default Location
			/// </summary>
			public class DefaultLocation : CR.Location.PK.ForeignKeyOf<EPEmployee>.By<bAccountID, defLocationID> { }
			/// <summary>
			/// Primary Contact
			/// </summary>
			public class PrimaryContact : CR.Contact.PK.ForeignKeyOf<EPEmployee>.By<primaryContactID> { }
			/// <summary>
			/// Department
			/// </summary>
			public class Department : EP.EPDepartment.PK.ForeignKeyOf<EPEmployee>.By<departmentID> { }
			/// <summary>
			/// The employee's supervisor to whom the reports are sent
			/// </summary>
			public class ReportsTo : EP.EPEmployee.PK.ForeignKeyOf<EPEmployee>.By<supervisorID> { }
			/// <summary>
			/// Sales Person
			/// </summary>
			public class SalesPerson : AR.SalesPerson.PK.ForeignKeyOf<EPEmployee>.By<salesPersonID> { }
			/// <summary>
			/// Labor Item
			/// </summary>
			public class LabourItem : IN.InventoryItem.PK.ForeignKeyOf<EPEmployee>.By<labourItemID> { }

			/// <summary>
			/// Account for the sales records 
			/// </summary>
			public class SalesAccount : GL.Account.PK.ForeignKeyOf<EPEmployee>.By<salesAcctID> { }
			/// <summary>
			/// Subaccount for the sales records 
			/// </summary>
			public class SalesSubaccount : GL.Sub.PK.ForeignKeyOf<EPEmployee>.By<salesSubID> { }

			/// <summary>
			/// The account used to record the cash discount amounts received from the vendor due to credit terms.
			/// </summary>
			public class CashDiscountAccount : GL.Account.PK.ForeignKeyOf<EPEmployee>.By<discTakenAcctID> { }
			/// <summary>
			/// The subaccount used to record the cash discount amounts received from the vendor due to credit terms.
			/// </summary>
			public class CashDiscountSubaccount : GL.Sub.PK.ForeignKeyOf<EPEmployee>.By<discTakenSubID> { }

			/// <summary>
			/// Account to record compensations
			/// </summary>
			public class ExpenseAccount : GL.Account.PK.ForeignKeyOf<EPEmployee>.By<expenseAcctID> { }
			/// <summary>
			/// Subaccount to record compensations
			/// </summary>
			public class ExpenseSubaccount : GL.Sub.PK.ForeignKeyOf<EPEmployee>.By<expenseSubID> { }

			/// <summary>
			/// Account to record prepayments paid to the employee
			/// </summary>
			public class PrepaymentAccount : GL.Account.PK.ForeignKeyOf<EPEmployee>.By<prepaymentAcctID> { }
			/// <summary>
			/// Subaccount to record prepayments paid to the employee
			/// </summary>
			public class PrepaymentSubaccount : GL.Sub.PK.ForeignKeyOf<EPEmployee>.By<prepaymentSubID> { }

			/// <summary>
			/// Vendor's owner
			/// </summary>
			public class Owner : CR.Contact.PK.ForeignKeyOf<EPEmployee>.By<ownerID> { }
			/// <summary>
			/// Workgroup
			/// </summary>
			public class Workgroup : TM.EPCompanyTree.PK.ForeignKeyOf<EPEmployee>.By<workgroupID> { }

			/// <summary>
			/// Login information
			/// </summary>
			public class User : PX.SM.Users.PK.ForeignKeyOf<EPEmployee>.By<userID> { }
		}
		#endregion

		#region BaseCuryID
		public new abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }

		/// <summary>
		/// The employee's base <see cref="Currency"/>, which is the base currency of the branch selected in the Branch box.
		/// </summary>
		/// <value>
		/// This field corresponds to <see cref="Organization.BaseCuryID"/>.
		/// </value>
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Search<CM.CurrencyList.curyID>))]
		[PXDefault(typeof(Switch<
			Case<Where<isBranch, Equal<True>>, Null>,
			Current<AccessInfo.baseCuryID>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Base Currency ID", Visible = false)]
		public override String BaseCuryID { get; set; }
		#endregion
		#region VStatus
		public new abstract class vStatus : Vendor.vStatus { }

		/// <summary>
		/// The status of the employee.
		/// </summary>
		/// <value>
		/// The possible values of the field are listed in
		/// the <see cref="VendorStatus"/> class. The set of these values can be changed and extended by using the workflow engine.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(VendorStatus.Active)]
		[VendorStatus.List]
		public override String VStatus { get; set; }
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion
		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		/// <summary>
		/// The human-readable identifier of the employee that is
		/// specified by the user or defined by the EMPLOYEE auto-numbering sequence during the
		/// creation of the employee. This field is a natural key, as opposed
		/// to the surrogate key <see cref="BAccountID"/>.
		/// </summary>
		[EmployeeRaw]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask="")]
		[PXDefault()]
		[PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public override String AcctCD
		{
			get
			{
				return this._AcctCD;
			}
			set
			{
				this._AcctCD = value;
			}
		}
		#endregion
		#region ParentBAccountID
		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		/// <summary>
		/// Represents the branch of your organization where the employee works.
		/// </summary>
		[PXDBInt()]
		[PXDefault(typeof(Search2<Branch.bAccountID,
			InnerJoin<BAccountR,
						On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>,
			Where<Branch.active, Equal<True>, And<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>))]
		[PXUIField(DisplayName = "Branch")]
		[PXDimensionSelector("BIZACCT", typeof(Search2<Branch.bAccountID,
			InnerJoin<BAccountR,
						On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>,
			Where<Branch.active, Equal<True>, And<MatchWithBranch<Branch.branchID>>>>), typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
		public override Int32? ParentBAccountID
		{
			get
			{
				return this._ParentBAccountID;
			}
			set
			{
				this._ParentBAccountID = value;
			}
		}
		#endregion
		#region DefContactID
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		/// <summary>
		/// The identifier of the <see cref="CR.Contact"/> object linked with the current employee as Contact Info.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CR.Contact.ContactID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXUIField(DisplayName = "Default Contact")]
		[PXSelector(typeof(Search<Contact.contactID, Where<Contact.bAccountID, Equal<Current<EPEmployee.parentBAccountID>>>>))]
		public override Int32? DefContactID
		{
			get
			{
				return this._DefContactID;
			}
			set
			{
				this._DefContactID = value;
			}
		}
		#endregion
		#region DefAddressID
		public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		/// <summary>
		/// The identifier of the <see cref="CR.Address"/> object linked with the current employee as Address Info.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CR.Address.AddressID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		[PXUIField(DisplayName = "Default Address")]
		[PXSelector(typeof(Search<Address.addressID, Where<Address.bAccountID, Equal<Current<EPEmployee.parentBAccountID>>>>))]
		public override Int32? DefAddressID
		{
			get
			{
				return this._DefAddressID;
			}
			set
			{
				this._DefAddressID = value;
			}
		}

		#endregion
		#region Type
		/// <summary>
		/// A field inherited from <see cref="BAccount"/> represents the type of the business account.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="BAccountType"/> class.
		/// For Employee the only possible value is <see cref="BAccountType.EmployeeType"/>.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(BAccountType.EmployeeType)]
		[PXUIField(DisplayName = "Type")]
		[BAccountType.List()]
		public override String Type
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
		#region AcctName
				public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		/// <summary>
		/// The employee name, which is usually a concatenation of the
		/// <see cref="Contact.FirstName">first</see> and <see cref="Contact.LastName">last name</see>
		/// of the appropriate contact.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Employee Name", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public override string AcctName
		{
			get
			{
				return base.AcctName;
			}
			set
			{
				base.AcctName = value;
			}
		}
		#endregion
		#region AcctReferenceNbr
		/// <summary>
		/// The external reference number of the employee.</summary>
		/// <remarks>It can be an additional number of the employee used in external integration.
		/// </remarks>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Employee Ref. No.", Visibility = PXUIVisibility.Visible)]
		public override string AcctReferenceNbr
		{
			get
			{
				return base.AcctReferenceNbr;
			}
			set
			{
				base.AcctReferenceNbr = value;
			}
		}
		#endregion
		#region VendorClassID
		/// <summary>
		/// The identifier of the <see cref="EPEmployeeClass">employee class</see> 
		/// that the employee belongs to.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPEmployeeClass.VendorClassID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault]
		[PXUIField(DisplayName = "Employee Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(EPEmployeeClass.vendorClassID),DescriptionField = typeof(EPEmployeeClass.descr))]
		public override String VendorClassID
		{
			get
			{
				return this._VendorClassID;
			}
			set
			{
				this._VendorClassID = value;
			}
		}
		#endregion
		#region Attributes
		/// <summary>
		/// The attributes list available for the current employee.
		/// </summary>
		[CRAttributesField(typeof (EPEmployee.vendorClassID), typeof (BAccount.noteID))]
	    public override string[] Attributes { get; set; }

	    #endregion
		
		#region DepartmentID
		public abstract class departmentID : PX.Data.BQL.BqlString.Field<departmentID> { }
		protected String _DepartmentID;
		/// <summary>
		/// Identifier of the <see cref="EPDepartment">employee department</see> 
		/// that the employee belongs to.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault()]
		[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
		[PXUIField(DisplayName = "Department", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String DepartmentID
		{
			get
			{
				return this._DepartmentID;
			}
			set
			{
				this._DepartmentID = value;
			}
		}
		 #endregion
		#region DefLocationID
		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		/// <summary>
		/// The identifier of the <see cref="Location"/> object linked with the employee and marked as default.
		/// The fields from the linked location are shown on the <b>Financial Settings</b> tab.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="Location.LocationID"/> field.
		/// </value>
		/// <remarks>
		/// Also, the <see cref="Location.BAccountID">Location.BAccountID</see> value must be equal to
		/// the <see cref="BAccount.BAccountID">BAccount.BAccountID</see> value of the current employee.
		/// </remarks>
		[PXDefault()]
		[PXDBInt()]
		[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.SelectorVisible)]
		[DefLocationID(typeof(Search<Location.locationID, Where<Location.bAccountID, Equal<Current<EPEmployee.bAccountID>>>>), SubstituteKey=typeof(Location.locationCD), DescriptionField = typeof(Location.descr))]
		[PXDBChildIdentity(typeof(Location.locationID))]
		public override int? DefLocationID
		{
			get
			{
				return base.DefLocationID;
			}
			set
			{
				base.DefLocationID = value;
			}
		}
		#endregion
		#region SupervisorID
		public abstract class supervisorID : PX.Data.BQL.BqlInt.Field<supervisorID> { }
		protected Int32? _SupervisorID;
		/// <summary>
		/// The identifier of the <see cref="EPEmployee">employee</see> to whom the current employee sends reports.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[PXDBInt()]
		[PXEPEPEmployeeSelector]
		[PXUIField(DisplayName = "Reports to", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? SupervisorID
		{
			get
			{
				return this._SupervisorID;
			}
			set
			{
				this._SupervisorID = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		/// <summary>
		/// The identifier of the <see cref="SalesPerson">sales person</see> to whom the current employee matches.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="SalesPerson.SalesPersonID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDimensionSelector("SALESPER", typeof(Search5<SalesPerson.salesPersonID,
												LeftJoin<EPEmployee,On<SalesPerson.salesPersonID,Equal<EPEmployee.salesPersonID>>>,
											   Where<EPEmployee.bAccountID, IsNull,
											   Or<EPEmployee.bAccountID, Equal<Current<EPEmployee.bAccountID>>>>, 
											   Aggregate<GroupBy<SalesPerson.salesPersonID>>>), 
											   typeof(SalesPerson.salesPersonCD), 
											   typeof(SalesPerson.salesPersonCD),
											   typeof(SalesPerson.descr))]
		[PXUIField(DisplayName = "Salesperson", Visibility = PXUIVisibility.Visible)]
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
		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }
		protected Int32? _LabourItemID;
		/// <summary>
		/// The identifier of the labor item for the current employee.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
		/// </value>
		/// <remarks>
		/// The labor item is a non-stock item (of the Labor type) associated with the employee and used as a source of expense accounts for transactions associated with projects or contracts.
		/// </remarks>
		[PXDBInt()]
		[PXUIField(DisplayName = "Labor Item")]
		[PXDimensionSelector(InventoryAttribute.DimensionName, 
			typeof(Search<InventoryItem.inventoryID, 
				Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, 
					And<Match<Current<AccessInfo.userName>>>>>), 
			typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr) )]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? LabourItemID
		{
			get
			{
				return this._LabourItemID;
			}
			set
			{
				this._LabourItemID = value;
			}
		}
		#endregion
		#region ShiftID
		public abstract class shiftID : PX.Data.BQL.BqlInt.Field<shiftID> { }
		/// <summary>
		/// The identifier of the shift code that the system inserts by default for any new time activity or earning record entered for the employee.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="EPShiftCode.shiftID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Shift Code", FieldClass = nameof(FeaturesSet.ShiftDifferential))]
		[PXSelector(typeof(SearchFor<EPShiftCode.shiftID>.Where<EPShiftCode.isManufacturingShift.IsEqual<False>>),
			SubstituteKey = typeof(EPShiftCode.shiftCD),
			DescriptionField = typeof(EPShiftCode.description))]
		[PXRestrictor(typeof(Where<EPShiftCode.isActive.IsEqual<True>>), Messages.ShiftCodeNotActive)]
		public virtual int? ShiftID { get; set; }
		#endregion ShiftID
		#region UnionID
		public abstract class unionID : PX.Data.BQL.BqlString.Field<unionID> { }
		/// <summary>
		/// The local identifier of the union associated with the employee.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="PM.PMUnion.unionID"/> field.
		/// </value>
		[PXRestrictor(typeof(Where<PM.PMUnion.isActive, Equal<True>>), PM.Messages.InactiveUnion, typeof(PM.PMUnion.unionID))]
		[PXSelector(typeof(Search<PM.PMUnion.unionID>))]
		[PXDBString(PM.PMUnion.unionID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Union Local ID", FieldClass = nameof(FeaturesSet.Construction))]
		public virtual String UnionID
		{
			get;
			set;
		}
		#endregion

		#region VendorClassID
		public new abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		#endregion
		#region ClassID
		public new abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		#endregion

		#region UserID
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		/// <summary>
		/// The identifier of the <see cref="PX.SM.Users">Users</see> to be used for the employee to sign into the system.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="PX.SM.Users.PKID">Users.PKID</see> field.
		/// </value>
		[PXDBGuid]
		[PXUIField(DisplayName = "Employee Login", Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(FK.User))]
		public virtual Guid? UserID { get; set; }
		#endregion
		
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;
		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Account">account</see> to be used to record sales made by the employee, if applicable.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXDefault(typeof(Select<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployee.vendorClassID>>>>), SourceField = typeof(EPEmployeeClass.salesAcctID))]
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;
		/// <summary>
		/// The identifier of the corresponding <see cref="PX.Objects.GL.Sub">subaccount</see> to be used to record sales made by the employee.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.GL.Sub.SubID">AccountID</see> field.
		/// </value>
		[PXDefault(typeof(Select<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployee.vendorClassID>>>>), SourceField = typeof(EPEmployeeClass.salesSubID))]
		[SubAccount(typeof(EPEmployee.salesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region DiscTakenAcctID
		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Account">account</see> used to record the cash discount amounts received from the vendor due to credit terms.
		/// Inherited from <see cref="Vendor"/> class.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Cash Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Select<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployee.vendorClassID>>>>), SourceField = typeof(EPEmployeeClass.discTakenAcctID))]
		public override Int32? DiscTakenAcctID
		{
			get
			{
				return this._DiscTakenAcctID;
			}
			set
			{
				this._DiscTakenAcctID = value;
			}
		}
		#endregion
		#region DiscTakenSubID
		/// <summary>
		/// The identifier of the corresponding <see cref="PX.Objects.GL.Sub">subaccount</see> used to record the cash discount amounts received from the vendor due to credit terms.
		/// Inherited from <see cref="Vendor"/> class.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.GL.Sub.SubID">AccountID</see> field.
		/// </value>
		[SubAccount(typeof(Vendor.discTakenAcctID), DisplayName = "Cash Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Select<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployee.vendorClassID>>>>), SourceField = typeof(EPEmployeeClass.discTakenSubID))]
		public override Int32? DiscTakenSubID
		{
			get
			{
				return this._DiscTakenSubID;
			}
			set
			{
				this._DiscTakenSubID = value;
			}
		}
		#endregion				
		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		protected Int32? _ExpenseAcctID;
		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Account">account</see> that will be used to record compensation amounts paid to the employee.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Select<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployee.vendorClassID>>>>), SourceField = typeof(EPEmployeeClass.expenseAcctID))]
		public virtual Int32? ExpenseAcctID
		{
			get
			{
				return this._ExpenseAcctID;
			}
			set
			{
				this._ExpenseAcctID = value;
			}
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		protected Int32? _ExpenseSubID;
		/// <summary>
		/// The identifier of the corresponding <see cref="PX.Objects.GL.Sub">subaccount</see> that will be used to record compensation amounts paid to the employee.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.GL.Sub.SubID">AccountID</see> field.
		/// </value>
		[SubAccount(typeof(EPEmployee.expenseAcctID), DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Select<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployee.vendorClassID>>>>), SourceField = typeof(EPEmployeeClass.expenseSubID))]
		public virtual Int32? ExpenseSubID
		{
			get
			{
				return this._ExpenseSubID;
			}
			set
			{
				this._ExpenseSubID = value;
			}
		}
		#endregion
		#region PrepaymentAcctID
		public new abstract class prepaymentAcctID : PX.Data.BQL.BqlInt.Field<prepaymentAcctID> { }
		/// <summary>
		/// Identifier of the AP <see cref="PX.Objects.GL.Account">account</see> to be used to record prepayments paid to the employee.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Prepayment Account", DescriptionField = typeof(Account.description), ControlAccountForModule = GL.ControlAccountModule.AP)]
		[PXDefault(typeof(Select<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployee.vendorClassID>>>>), SourceField = typeof(EPEmployeeClass.prepaymentAcctID), PersistingCheck = PXPersistingCheck.Nothing)]
		public override Int32? PrepaymentAcctID
		{
			get
			{
				return this._PrepaymentAcctID;
			}
			set
			{
				this._PrepaymentAcctID = value;
			}
		}
		#endregion
		#region PrepaymentSubID
		public new abstract class prepaymentSubID : PX.Data.BQL.BqlInt.Field<prepaymentSubID> { }
		/// <summary>
		/// The identifier of the corresponding <see cref="PX.Objects.GL.Sub">subaccount</see> to be used to record prepayments paid to the employee.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.GL.Sub.SubID">AccountID</see> field.
		/// </value>
		[SubAccount(typeof(EPEmployee.prepaymentAcctID), DisplayName = "Prepayment Sub.", DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Select<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployee.vendorClassID>>>>), SourceField = typeof(EPEmployeeClass.prepaymentSubID), PersistingCheck = PXPersistingCheck.Nothing)]
		public override Int32? PrepaymentSubID
		{
			get
			{
				return this._PrepaymentSubID;
			}
			set
			{
				this._PrepaymentSubID = value;
			}
		}
		#endregion

		
		#region CalendarID
		public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }
		protected String _CalendarID;
		/// <summary>
		/// The identifier of the <see cref="PX.Objects.CS.CSCalendar">calendar</see> that records the working hours of the employee and the time zone of the employee.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.CS.CSCalendar.CalendarID" /> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployee.vendorClassID>>>>), SourceField = typeof(EPEmployeeClass.calendarID))]
		[PXUIField(DisplayName = "Calendar", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region DefaultWorkgroupID
		public abstract class defaultWorkgroupID : PX.Data.BQL.BqlInt.Field<defaultWorkgroupID> { }
		protected int? _DefaultWorkgroupID;
		/// <summary>
		/// The identifier of the <see cref="PX.TM.EPCompanyTree">workgroup</see> that the system inserts by default
		/// for each new record entered on the <b>Details</b> tab of the <i>Employee Time Card (EP305000)</i> form for this employee.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.TM.EPCompanyTree.workGroupID" /> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Default Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, IsWorkgroupOrSubgroupOfContact<Current<EPEmployee.defContactID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual int? DefaultWorkgroupID
		{
			get
			{
				return this._DefaultWorkgroupID;
			}
			set
			{
				this._DefaultWorkgroupID = value;
			}
		}
		#endregion
		#region PositionLineCntr
		public abstract class positionLineCntr : PX.Data.BQL.BqlInt.Field<positionLineCntr> { }
		protected int? _PositionLineCntr;
		/// <exclude/>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? PositionLineCntr
		{
			get
			{
				return this._PositionLineCntr;
			}
			set
			{
				this._PositionLineCntr = value;
			}
		}
		#endregion	
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		/// <inheritdoc/>
		[PXSearchable(SM.SearchCategory.OS, Messages.SearchableTitleEmployee, new Type[] { typeof(EPEmployee.acctCD), typeof(EPEmployee.acctName) },
		   new Type[] { typeof(EPEmployee.defContactID), typeof(Contact.eMail) },
		   NumberFields = new Type[] { typeof(EPEmployee.acctCD) },
			 Line1Format = "{1}{2}", Line1Fields = new Type[] { typeof(EPEmployee.defContactID), typeof(Contact.eMail), typeof(Contact.phone1) },
			 Line2Format = "{1}", Line2Fields = new Type[] { typeof(EPEmployee.departmentID), typeof(EPDepartment.description) },
			SelectForFastIndexing = typeof(Select2<EPEmployee, InnerJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>>)
		 )]
		[PXUniqueNote(
			Selector = typeof(Search2<EPEmployee.acctCD,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<EPEmployee.bAccountID>>>,
				Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>),
			DescriptionField = typeof(EPEmployee.acctCD),
			ShowInReferenceSelector = true,
            PopupTextEnabled = true)]
		public override Guid? NoteID
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

		#region RouteEmails

		public abstract class routeEmails : PX.Data.BQL.BqlBool.Field<routeEmails> { }

		/// <summary>
		/// Specifies whether the emails addressed to this employee should be routed
		/// from an email account to the employee's email address if the processing of incoming mail is enabled
		/// for the email account and the <b>Route Employee Emails</b> check box is selected
		/// on the <i>Email Accounts (SM204002)</i> form. For details, see <i>Incoming Mail Processing</i>.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Route Emails")]
		public virtual Boolean? RouteEmails { get; set; }

		#endregion
		#region TimeCardRequired

		public abstract class timeCardRequired : PX.Data.BQL.BqlBool.Field<timeCardRequired> { }

		/// <summary>
		/// Specifies whether time cards are required for this employee.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Time Card Is Required")]
		[PXDefault(false)]
		public virtual Boolean? TimeCardRequired { get; set; }

		#endregion
        #region HoursValidation
        public abstract class hoursValidation : PX.Data.BQL.BqlString.Field<hoursValidation> { }
        protected String _HoursValidation;
		/// <summary>
		/// The extent of validation of regular work hours for this employee.
		/// </summary>
		/// <value>
		/// The default value is set to the <see cref="EPEmployeeClass.hoursValidation">regular hours validation</see> for the selected <see cref="VendorClassID">vendor class</see>.
		/// </value>
		[PXDBString(1)]
        [PXUIField(DisplayName = "Regular Hours Validation")]
        [HoursValidationOption.List]
		[PXDefault(typeof(Select<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Current<EPEmployee.vendorClassID>>>>), SourceField = typeof(EPEmployeeClass.hoursValidation), Constant = HoursValidationOption.Validate)]
        public virtual String HoursValidation
        {
            get
            {
                return this._HoursValidation;
            }
            set
            {
                this._HoursValidation = value;
            }
        }
		#endregion
		#region ReceiptAndClaimTaxZoneID
		public abstract class receiptAndClaimTaxZoneID : PX.Data.BQL.BqlString.Field<receiptAndClaimTaxZoneID> { }
		/// <summary>
		/// The identifier of the Receipt and Claim tax zone.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		public virtual string ReceiptAndClaimTaxZoneID
		{
			get;
			set;
		}
		#endregion
	}

	[Serializable]
	[PXCacheName(CR.Messages.Employee)]
    [PXHidden]
	public sealed class EPEmployeeSimple : EPEmployee
	{
		#region BAccountID

		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		#endregion

		#region DefContactID

		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }

		#endregion

		#region UserID

		public new abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }

		#endregion
	}
    

	#region ApproverEmployee

	[Serializable]
    [PXHidden]
	public partial class ApproverEmployee : EPEmployee
	{
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		public new abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }

		#region AcctCD

		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

		/// <inheritdoc/>
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Assignee ID")]
		public override string AcctCD
		{
			get
			{
				return base.AcctCD;
			}
			set
			{
				base.AcctCD = value;
			}
		}

		#endregion

		#region AcctName

		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

		/// <inheritdoc/>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Assigned To")]
		public override string AcctName
		{
			get
			{
				return base.AcctName;
			}
			set
			{
				base.AcctName = value;
			}
		}

		#endregion
	}

	#endregion

	#region ApprovedByEmployee

	[Serializable]
    [PXHidden]
	public partial class ApprovedByEmployee : EPEmployee
	{
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		public new abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }

		#region AcctCD

		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

		/// <inheritdoc/>
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Approved by (ID)")]
		public override string AcctCD
		{
			get
			{
				return base.AcctCD;
			}
			set
			{
				base.AcctCD = value;
			}
		}

		#endregion

		#region AcctName

		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

		/// <inheritdoc/>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Approved By")]
		public override string AcctName
		{
			get
			{
				return base.AcctName;
			}
			set
			{
				base.AcctName = value;
			}
		}

		#endregion
	}

	#endregion
}

namespace PX.Objects.EP.Simple
{
	/// <summary>
	/// Represents a simple version of an Employee class.
	/// </summary>
	[System.SerializableAttribute()]
    [PXHidden]
    public partial class EPEmployee : IBqlTable
    {
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        protected Int32? _BAccountID;
		/// <summary>
		/// The identifier of the employee.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
        [PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Int32? BAccountID
        {
            get
            {
                return this._BAccountID;
            }
            set
            {
                this._BAccountID = value;
            }
        }
        #endregion

        #region UserID
        public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		/// <summary>
		/// The identifier of the <see cref="PX.SM.Users">Users</see> to be used for the employee to sign into the system.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="PX.SM.Users.PKID">Users.PKID</see> field.
		/// </value>
        [PXDBGuid]
        [PXUIField(DisplayName = "Employee Login", Visibility = PXUIVisibility.Visible)]
        public virtual Guid? UserID { get; set; }
        #endregion
    }
	
}

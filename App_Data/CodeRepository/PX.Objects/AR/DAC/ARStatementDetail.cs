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

using PX.Objects.GL;
using PX.Objects.CM;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AR
{
	/// <summary>
	/// A document that has been included in a <see cref="ARStatement">customer statement</see>.
	/// The entities of this type are created on the Prepare Statements (AR503000) processing form and
	/// can be seen on the Customer Statement (AR641500) and Customer Statement MC (AR642000) reports.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.ARStatementDetail)]
	public partial class ARStatementDetail : IBqlTable
	{
		#region Keys
		// <exclude/>
		public class PK : PrimaryKeyOf<ARStatementDetail>.By<branchID, customerID, curyID, statementDate, docType, refNbr, refNoteID>
		{
			public static ARStatementDetail Find(PXGraph graph, Int32? branchID, Int32? customerID, String curyID, DateTime? statementDate, String docType, String refNbr, Guid? RefNoteID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, branchID, customerID, curyID, statementDate, docType, refNbr, RefNoteID, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<ARStatementDetail>.By<branchID> { }
			public class Customer : AR.Customer.PK.ForeignKeyOf<ARStatementDetail>.By<customerID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<ARStatementDetail>.By<curyID> { }
			public class Statement : AR.ARStatement.PK.ForeignKeyOf<ARStatementDetail>.By<branchID, customerID, curyID, statementDate> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <summary>
		/// The integer identifier of the <see cref="Branch"/> of the <see cref="ARStatement">
		/// Customer Statement</see>, to which the detail belongs. This field is part of the compound 
		/// key of the statement detail, and part of the foreign key referencing the 
		/// <see cref="ARStatement">Customer Statement</see> record. 
		/// Corresponds to the <see cref="ARStatement.BranchID"/> field.
		/// </summary>
		[Branch]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		/// <summary>
		/// The integer identifier of the <see cref="Customer"/> of the <see cref="ARStatement">
		/// Customer Statement</see>, to which the detail belongs. This field is part of the compound
		/// key of the statement detail, and part of the foreign key referencing the
		/// <see cref="ARStatement">Customer Statement</see> record.
		/// Corresponds to the <see cref="ARStatement.CustomerID"/> field.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(ARStatement.customerID))]
		[PXUIField(DisplayName = "Customer ID")]
		[PXParent(typeof(Select<
			ARStatement,
			Where<
				ARStatement.customerID, Equal<Current<ARStatementDetail.customerID>>,
				And<ARStatement.statementDate, Equal<Current<ARStatementDetail.statementDate>>,
				And<ARStatement.curyID, Equal<Current<ARStatementDetail.curyID>>>>>>))]
		public virtual int? CustomerID
		{
			get;
			set;
		}
		#endregion
		#region StatementDate
		public abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
		/// <summary>
		/// The date of the <see cref="ARStatement">Customer Statement</see>, to which
		/// the detail belongs. This field is part of the compound key of the statement
		/// detail, and part of the foreign key referencing the <see cref="ARStatement">
		/// Customer Statement</see> record.
		/// Corresponds to the <see cref="ARStatement.StatementDate"/> field.
		/// </summary>
		[PXDBDate(IsKey = true)]
		[PXDefault(typeof(ARStatement.statementDate))]
		[PXUIField(DisplayName = "Statement Date")]
		public virtual DateTime? StatementDate
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		/// <summary>
		/// The currency of the <see cref="ARStatement">Customer Statement</see>, to
		/// which the detail belongs. This field is part of the compound key of the
		/// statement detail, and part of the foreign key referencing the <see cref="ARStatement">
		/// Customer Statement</see> record.
		/// Corresponds to the <see cref="ARStatement.StatementDate"/> field.
		/// </summary>
		[PXDBString(5, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(ARStatement.curyID))]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency ID")]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		/// <summary>
		/// The type of the document, for which the statement detail is created.
		/// This field is part of the compound key of the statement detail,
		/// and part of the foreign key referencing the <see cref="ARRegister"/>
		/// document. Corresponds to the <see cref="ARRegister.DocType"/> field.
		/// </summary>
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "DocType")]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		/// <summary>
		/// The reference number of the document, for which the statement
		/// detail is created. This field is part of the compound key of
		/// the statement detail, and part of the foreign key referencing
		/// the <see cref="ARRegister"/> document. Corresponds to the
		/// <see cref="ARRegister.RefNbr"/> field.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Ref. Nbr.")]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region DocBalance
		public abstract class docBalance : PX.Data.BQL.BqlDecimal.Field<docBalance> { }
		/// <summary>
		/// Indicates the balance, in base currency, that the document
		/// has on the statement date.
		/// </summary>
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Doc. Balance")]
		public virtual decimal? DocBalance
		{
			get;
			set;
		}
		#endregion
		#region CuryDocBalance
		public abstract class curyDocBalance : PX.Data.BQL.BqlDecimal.Field<curyDocBalance> { }
		/// <summary>
		/// Indicates the balance, in document currency, that the document
		/// has on the statement date.
		/// </summary>
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Cury. Doc. Balance")]
		public virtual decimal? CuryDocBalance
		{
			get;
			set;
		}
		#endregion
		#region StatementType
		public abstract class statementType : PX.Data.BQL.BqlString.Field<statementType> { }
		/// <summary>
		/// The type of the customer statement.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Statement Type")]
		public virtual string StatementType
		{
			get;
			set;
		}
		#endregion
		#region BegBalance
		public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
		/// <summary>
		/// The beginning balance of the customer statement in the base currency. Only for the Balance Brought Forvard type.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beg. Balance")]
		public virtual decimal? BegBalance
		{
			get;
			set;
		}
		#endregion
		#region CuryBegBalance
		public abstract class curyBegBalance : PX.Data.BQL.BqlDecimal.Field<curyBegBalance> { }
		/// <summary>
		/// The beginning balance of the customer statement in the foreign currency. Only for the Balance Brought Forvard type.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Beg. Balance")]
		public virtual decimal? CuryBegBalance
		{
			get;
			set;
		}
		#endregion
		#region AgeBalance00
		public abstract class ageBalance00 : PX.Data.BQL.BqlDecimal.Field<ageBalance00> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 00 in the base currency.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Age00 Balance")]
		public virtual decimal? AgeBalance00
		{
			get;
			set;
		}
		#endregion
		#region CuryAgeBalance00
		public abstract class curyAgeBalance00 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance00> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 00 in the foreign currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cury. Age00 Balance")]
		public virtual decimal? CuryAgeBalance00
		{
			get;
			set;
		}
		#endregion
		#region AgeBalance01
		public abstract class ageBalance01 : PX.Data.BQL.BqlDecimal.Field<ageBalance01> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 01 in the base currency.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Age01 Balance")]
		public virtual Decimal? AgeBalance01
		{
			get;
			set;
		}
		#endregion
		#region CuryAgeBalance01
		public abstract class curyAgeBalance01 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance01> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 01 in the foreign currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cury. Age01 Balance")]
		public virtual decimal? CuryAgeBalance01
		{
			get;
			set;
		}
		#endregion
		#region AgeBalance02
		public abstract class ageBalance02 : PX.Data.BQL.BqlDecimal.Field<ageBalance02> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 02 in the base currency.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Age02 Balance")]
		public virtual decimal? AgeBalance02
		{
			get;
			set;
		}
		#endregion
		#region CuryAgeBalance02
		public abstract class curyAgeBalance02 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance02> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 03 in the foreign currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cury. Age02 Balance")]
		public virtual decimal? CuryAgeBalance02
		{
			get;
			set;
		}
		#endregion
		#region AgeBalance03
		public abstract class ageBalance03 : PX.Data.BQL.BqlDecimal.Field<ageBalance03> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 03 in the base currency.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cury. Age03 Balance")]
		public virtual decimal? AgeBalance03
		{
			get;
			set;
		}
		#endregion
		#region CuryAgeBalance03
		public abstract class curyAgeBalance03 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance03> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 03 in the foreign currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cury. Age03 Balance")]
		public virtual decimal? CuryAgeBalance03
		{
			get;
			set;
		}
		#endregion
		#region AgeBalance04
		public abstract class ageBalance04 : PX.Data.BQL.BqlDecimal.Field<ageBalance04> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 04 in the base currency.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Age04 Balance")]
		public virtual decimal? AgeBalance04
		{
			get;
			set;
		}
		#endregion
		#region CuryAgeBalance04
		public abstract class curyAgeBalance04 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance04> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 04 in the foreign currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cury. Age04 Balance")]
		public virtual Decimal? CuryAgeBalance04
		{
			get;
			set;
		}
		#endregion

		#region IsOpen
		public abstract class isOpen : PX.Data.BQL.BqlBool.Field<isOpen> { }
		/// <summary>
		/// If set to <c>true</c>, indicates that the document
		/// is open on the statement date.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		public virtual bool? IsOpen
		{
			get;
			set;
		}
		#endregion
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlInt.Field<refNoteID>
		{
		}

		[PXDBGuid(IsKey = true)]
		public virtual Guid? RefNoteID { get; set; }
		#endregion

		#region TranPostID
		public abstract class tranPostID : PX.Data.BQL.BqlInt.Field<tranPostID>
		{
		}

		[PXDBInt]
		public virtual int? TranPostID { get; set; }
		#endregion
	}
}

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
using PX.Objects.CM;

namespace PX.Objects.AR
{
	/// <summary>
	/// List of invoises in Dunning Letter
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.ARDunningLetterDetail)]
	public partial class ARDunningLetterDetail : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<ARDunningLetterDetail>.By<dunningLetterID, docType, refNbr>
		{
			public static ARDunningLetterDetail Find(PXGraph graph, Int32? dunningLetterID, String docType, String refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, dunningLetterID, docType, refNbr, options);
		}
		public static class FK
		{
			public class Customer : AR.Customer.PK.ForeignKeyOf<ARDunningLetterDetail>.By<bAccountID> { }
		}
		#endregion

		#region DunningLetterID
		public abstract class dunningLetterID : PX.Data.BQL.BqlInt.Field<dunningLetterID> { }
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(ARDunningLetter.dunningLetterID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Enabled = false)]
		[PXParent(typeof(Select<ARDunningLetter, Where<ARDunningLetter.dunningLetterID, Equal<Current<ARDunningLetterDetail.dunningLetterID>>>>))]
		public virtual Int32? DunningLetterID
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsFixed = true, IsKey = true)]
		[ARDocType.List()]
		[PXUIField(DisplayName = "Type")]
		[PXDefault()]
		public virtual String DocType
		{
			get;
			set;
		}
		#endregion
		#region PrintDocType
		public abstract class printDocType : PX.Data.BQL.BqlString.Field<printDocType> { }

		/// <summary>
		/// The type of the document for printing, which is used in reports.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ARDocType.PrintListAttribute"/>.
		/// </value>
		[PXString(3, IsFixed = true)]
		[ARDocType.PrintList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual String PrintDocType
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return DocType;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXDefault()]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		[PXDefault]
		[PXUIField(DisplayName = "Customer", IsReadOnly = true, Visible = false)]
		[Customer(DescriptionField = typeof(Customer.acctName))]
		public virtual int? BAccountID { get; set; }
		#endregion
		#region DunningLetterBAccountID
		public abstract class dunningLetterBAccountID : PX.Data.BQL.BqlInt.Field<dunningLetterBAccountID> { }

		[PXDefault]
		[PXDBInt]
		public virtual int? DunningLetterBAccountID { get; set; }
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion
		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "Due Date")]
		public virtual DateTime? DueDate
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		[PXDBCury(typeof(ARDunningLetterDetail.curyID))]
		public virtual Decimal? CuryOrigDocAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		[PXDBCury(typeof(ARDunningLetterDetail.curyID))]
		public virtual Decimal? CuryDocBal
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		[PXDBString(5, IsUnicode = true)]
		[PXDefault("")]
		[PXSelector(typeof(PX.Objects.CM.Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency ID")]
		public virtual String CuryID
		{
			get;
			set;
		}
		#endregion
		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Original Document Amount")]
		public virtual Decimal? OrigDocAmt
		{
			get;
			set;
		}
		#endregion
		#region DocBal
		public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Outstanding Balance")]
		public virtual Decimal? DocBal
		{
			get;
			set;
		}
		#endregion
		#region Overdue
		public abstract class overdue : PX.Data.BQL.BqlBool.Field<overdue> { }
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? Overdue
		{
			get;
			set;
		}
		#endregion
		#region OverdueBal
		public abstract class overdueBal : PX.Data.BQL.BqlDecimal.Field<overdueBal> { }
		[PXBaseCury]
		[PXDBCalced(typeof(Switch<Case<Where<overdue, Equal<True>>, docBal>, CS.decimal0>), typeof(decimal))]
		[PXUIField(DisplayName = "Overdue Balance")]
		public virtual Decimal? OverdueBal
		{
			get;
			set;
		}
		#endregion
		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		[PXDBBool()]
		[PXDBDefault(typeof(ARDunningLetter.voided))]
		public virtual Boolean? Voided
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		[PXDBBool()]
		[PXDBDefault(typeof(ARDunningLetter.released))]
		public virtual Boolean? Released
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region DunningLetterLevel
		public abstract class dunningLetterLevel : PX.Data.BQL.BqlInt.Field<dunningLetterLevel> { }
		[PXDBInt()]
		[PXDefault()]
		[PXUIField(DisplayName = "Dunning Level")]
		public virtual Int32? DunningLetterLevel
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(Messages.ARDunningLetterDetail)]
	public partial class ARDunningLetterDetailReport : ARDunningLetterDetail
	{
		#region SortDate
		/// <summary>
		/// Read-only field. Equals DueDate or DocDate if DueDate is null. Needed for sorting in reports.
		/// </summary>
		public abstract class sortDate : PX.Data.BQL.BqlDateTime.Field<sortDate> { }
		[PXDate()]
		[PXDBCalced(typeof(Switch<
				Case<Where<ARDunningLetterDetail.dueDate, IsNull>, ARDunningLetterDetail.docDate>,
				ARDunningLetterDetail.dueDate>), typeof(DateTime))]
		public virtual DateTime? SortDate
		{ get; set; }
		#endregion
	}
}

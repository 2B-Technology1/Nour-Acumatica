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
using PX.Objects.AP;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.IN;
using Amount = PX.Objects.AR.ARReleaseProcess.Amount;

namespace PX.Objects.PO
{
	[Serializable]
	[PXCacheName(Messages.POAccrualSplit)]
	public class POAccrualSplit : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POAccrualSplit>.By<aPDocType, aPRefNbr, aPLineNbr, pOReceiptType, pOReceiptNbr, pOReceiptLineNbr>
		{
			public static POAccrualSplit Find(PXGraph graph, string aPDocType, string aPRefNbr, int? aPLineNbr, string pOReceiptType, string pOReceiptNbr, int? pOReceiptLineNbr, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, aPDocType, aPRefNbr, aPLineNbr, pOReceiptType, pOReceiptNbr, pOReceiptLineNbr, options);
		}
		public static class FK
		{
			public class Receipt : POReceipt.PK.ForeignKeyOf<POAccrualSplit>.By<pOReceiptType, pOReceiptNbr> { }
			public class ReceiptLine : POReceiptLine.PK.ForeignKeyOf<POAccrualSplit>.By<pOReceiptType, pOReceiptNbr, pOReceiptLineNbr> { }
			public class AccrualStatus : POAccrualStatus.PK.ForeignKeyOf<POAccrualSplit>.By<refNoteID, lineNbr, type> { }
			public class APInvoice : AP.APInvoice.PK.ForeignKeyOf<POAccrualSplit>.By<aPDocType, aPRefNbr> { }
			public class APTran : AP.APTran.PK.ForeignKeyOf<POAccrualSplit>.By<aPDocType, aPRefNbr, aPLineNbr> { }
			//todo public class UnitOfMeasure : INUnit.UK.ByGlobal.ForeignKeyOf<POAccrualSplit>.By<uOM> { }
		}
		#endregion
		
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		[PXDBGuid]
		[PXDefault]
		public virtual Guid? RefNoteID
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt]
		[PXDefault]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[POAccrualType.List]
		public virtual string Type
		{
			get;
			set;
		}
		#endregion

		#region APDocType
		public abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }
		[APDocType.List]
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault]
		public virtual string APDocType
		{
			get;
			set;
		}
		#endregion
		#region APRefNbr
		public abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public virtual string APRefNbr
		{
			get;
			set;
		}
		#endregion
		#region APLineNbr
		public abstract class aPLineNbr : PX.Data.BQL.BqlInt.Field<aPLineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? APLineNbr
		{
			get;
			set;
		}
		#endregion

		#region POReceiptType
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault]
		public virtual string POReceiptType { get; set; }
		public abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }
		#endregion
		#region POReceiptNbr
		public abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public virtual string POReceiptNbr
		{
			get;
			set;
		}
		#endregion
		#region POReceiptLineNbr
		public abstract class pOReceiptLineNbr : PX.Data.BQL.BqlInt.Field<pOReceiptLineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? POReceiptLineNbr
		{
			get;
			set;
		}
		#endregion

		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string UOM
		{
			get;
			set;
		}
		#endregion
		#region AccruedQty
		public abstract class accruedQty : PX.Data.BQL.BqlDecimal.Field<accruedQty> { }
		[PXDBDecimal(6)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? AccruedQty
		{
			get;
			set;
		}
		#endregion
		#region BaseAccruedQty
		public abstract class baseAccruedQty : PX.Data.BQL.BqlDecimal.Field<baseAccruedQty> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BaseAccruedQty
		{
			get;
			set;
		}
		#endregion
		#region AccruedCost
		public abstract class accruedCost : PX.Data.BQL.BqlDecimal.Field<accruedCost> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AccruedCost
		{
			get;
			set;
		}
		#endregion
		#region PPVAmt
		public abstract class pPVAmt : PX.Data.BQL.BqlDecimal.Field<pPVAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? PPVAmt
		{
			get;
			set;
		}
		#endregion
		#region IsReversed
		public abstract class isReversed : PX.Data.BQL.BqlBool.Field<isReversed> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsReversed
		{
			get;
			set;
		}
		#endregion
		#region TaxAccruedCost
		public abstract class taxAccruedCost : PX.Data.BQL.BqlDecimal.Field<taxAccruedCost> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxAccruedCost
		{
			get;
			set;
		}
		#endregion

		#region FinPeriodID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [FinPeriodIDAttribute appends PXDBStringAttribute]
		[FinPeriodID]
		[PXDefault]
		public virtual string FinPeriodID { get; set; }
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID
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

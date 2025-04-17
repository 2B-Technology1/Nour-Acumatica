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

namespace PX.Objects.PO
{
	[Serializable]
	[PXCacheName(Messages.POLandedCostReceipt)]
	public class POLandedCostReceipt : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POLandedCostReceipt>.By<lCDocType, lCRefNbr, pOReceiptType, pOReceiptNbr>
		{
			public static POLandedCostReceipt Find(PXGraph graph, string lCDocType, string lCRefNbr, string pOReceiptType, string pOReceiptNbr, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, lCDocType, lCRefNbr, pOReceiptType, pOReceiptNbr, options);
		}
		public static class FK
		{
			public class LandedCostDocument : POLandedCostDoc.PK.ForeignKeyOf<POLandedCostReceipt>.By<lCDocType, lCRefNbr> { }
			public class Receipt : POReceipt.PK.ForeignKeyOf<POLandedCostReceipt>.By<pOReceiptType, pOReceiptNbr> { }
		}
		#endregion

		#region LCDocType
		public abstract class lCDocType : PX.Data.BQL.BqlString.Field<lCDocType> { }

		[POLandedCostDocType.List()]
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(POLandedCostDoc.docType))]
		[PXUIField(DisplayName = "Landed Cost Type", Visible = false)]
		public virtual String LCDocType
		{
			get;
			set;
		}
		#endregion
		#region LCRefNbr
		public abstract class lCRefNbr : PX.Data.BQL.BqlString.Field<lCRefNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(POLandedCostDoc.refNbr))]
		[PXUIField(DisplayName = "Landed Cost Nbr.")]
		[PXParent(typeof(FK.LandedCostDocument))]
		public virtual String LCRefNbr
		{
			get;
			set;
		}
		#endregion

		#region POReceiptType
		public abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }

		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "PO Receipt Type", Visible = false)]
		public virtual String POReceiptType
		{
			get;
			set;
		}
		#endregion
		#region POReceiptNbr
		public abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "PO Receipt Nbr.")]
		[PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.receiptType, Equal<Current<pOReceiptType>>>>))]
		public virtual String POReceiptNbr
		{
			get;
			set;
		}
		#endregion

		#region LineCntr
		// The formula in POLandedCostReceiptLine uses this field for creation and deletion of parent POLandedCostReceipt record.
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		[PXInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LineCntr
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
	}
}

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
using System;

namespace PX.Objects.CA
{
	[Serializable]
	[PXProjection(typeof(SelectFrom<CABatchDetail>.AggregateTo<GroupBy<CABatchDetail.batchNbr>, GroupBy<CABatchDetail.origModule>, GroupBy<CABatchDetail.origDocType>, GroupBy<CABatchDetail.origRefNbr>>))]
	[PXCacheName("Aggregated CA Batch Details")]
	public class CABatchDetailOrigDocAggregate : CABatchDetail
	{
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(CABatchDetail.batchNbr))]
		public override string BatchNbr { get; set; }
		#endregion
		#region OrigModule
		public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		[PXDBString(2, IsFixed = true, IsKey = true, BqlField = typeof(CABatchDetail.origModule))]
		[PXUIField(DisplayName = "Module", Enabled = false)]
		public override string OrigModule { get; set; }
		#endregion
		#region OrigDocType
		public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		[PXDBString(3, IsFixed = true, IsKey = true, BqlField = typeof(CABatchDetail.origDocType))]
		[PXUIField(DisplayName = "Doc. Type")]
		public override string OrigDocType { get; set; }
		#endregion
		#region OrigRefNbr
		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(CABatchDetail.origRefNbr))]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public override string OrigRefNbr { get; set; }
		#endregion
	}
}

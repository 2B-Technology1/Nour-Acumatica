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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN
{
	[PXHidden, PXProjection(typeof(
		SelectFrom<INTransitLineStatus>.
		Where<INTransitLineStatus.qtyOnHand.IsGreater<Zero>>.
		AggregateTo<GroupBy<INTransitLineStatus.transferNbr>>))]
	public class INTransferInTransit : IBqlTable
	{
		#region TransferNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTransitLineStatus.transferNbr))]
		public virtual String TransferNbr { get; set; }
		public abstract class transferNbr : BqlString.Field<transferNbr> { }
		#endregion
		#region RefNoteID
		[PXNote(BqlField = typeof(INTransitLineStatus.refNoteID))]
		public virtual Guid? RefNoteID { get; set; }
		public abstract class refNoteID : BqlGuid.Field<refNoteID> { }
		#endregion
	}
}

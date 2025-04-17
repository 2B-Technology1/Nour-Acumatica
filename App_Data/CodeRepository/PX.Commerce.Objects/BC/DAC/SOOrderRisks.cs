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
using System;
using PX.Objects.SO;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// Represents information about the risk fraud risk of a sales order.
	/// </summary>
	[Serializable]
	[PXCacheName("SO Order Risks")]
	public class SOOrderRisks : IBqlTable
	{
		public class PK : PrimaryKeyOf<SOOrderRisks>.By<SOOrderRisks.orderType, SOOrderRisks.orderNbr>
		{
			public static SOOrderRisks Find(PXGraph graph, string orderType, string orderNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, orderType, orderNbr, options);
		}
		public static class FK
		{
			public class Entity : SOOrderRisks.PK.ForeignKeyOf<SOOrder>.By<orderType, orderNbr> { }
		}
		#region OrderType
		/// <summary>
		/// The type of the order.
		/// </summary>
		[PXDBString(IsKey = true)]
		[PXDBDefault(typeof(SOOrder.orderType))]
		public virtual string OrderType { get; set; }
		/// <inheritdoc cref="OrderType" />
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		#endregion

		#region OrderNbr
		/// <summary>
		/// The order number.
		/// </summary>
		[PXDBString(IsKey = true)]
		[PXDBDefault(typeof(SOOrder.orderNbr))]
		public virtual string OrderNbr { get; set; }
		/// <inheritdoc cref="OrderNbr" />
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		#endregion

		#region LineNbr
		/// <summary>
		/// The order line number.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName ="Line Nbr." , Visible =false)]
		[PXLineNbr(typeof(SOOrder.riskLineCntr))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOOrderRisks.orderType>>,
											And<SOOrder.orderNbr, Equal<Current<SOOrderRisks.orderNbr>>
											>>>))]
		public virtual Int32? LineNbr { get; set; }
		/// <inheritdoc cref="LineNbr" />
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion

		#region NoteID
		/// <summary>
		/// The note ID of the record.
		/// </summary>
		[PXNote()]
		public Guid? NoteID { get; set; }
		/// <inheritdoc cref="NoteID" />
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion

		#region Recommendation
		/// <summary>
		/// The recommendation of what to do with this order.
		/// </summary>
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Recommendation")]
		public virtual string Recommendation { get; set; }
		/// <inheritdoc cref="Recommendation" />
		public abstract class recommendation : PX.Data.BQL.BqlString.Field<recommendation> { }
		#endregion

		#region Message
		/// <summary>
		/// The message describing the order risk.
		/// </summary>
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Message")]
		public virtual string Message { get; set; }
		/// <inheritdoc cref="Message" />
		public abstract class message : PX.Data.BQL.BqlString.Field<message> { }
		#endregion

		#region Score
		/// <summary>
		/// The order risk score as a percentage.
		/// </summary>
		[PXDBDecimal()]
		[PXUIField(DisplayName = "Score %")]

		public virtual decimal? Score { get; set; }
		/// <inheritdoc cref="Score" />
		public abstract class score : PX.Data.BQL.BqlDecimal.Field<score> { }
		#endregion
	}
}

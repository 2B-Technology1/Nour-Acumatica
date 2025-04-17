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

using PX.Commerce.Core;
using PX.Data;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// DAC extension of SOShipLine to add additional properties.
	/// </summary>
	[Serializable]
	public sealed class BCSOShipLineExt : PXCacheExtension<SOShipLine>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		#region AssociatedOrderLineNbr
		/// <summary>
		/// An optional associated order line number.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Associated Order Line Nbr.", Visible = false, Enabled = false)]
		public int? AssociatedOrderLineNbr { get; set; }
		/// <inheritdoc cref="AssociatedOrderLineNbr"/>
		public abstract class associatedOrderLineNbr : PX.Data.BQL.BqlInt.Field<associatedOrderLineNbr> { }
		#endregion

		#region GiftMessage
		/// <summary>
		/// An optional gift message for this Ship Line.
		/// </summary>
		[PXDBString(200, IsUnicode = true)]
		[PXUIField(DisplayName = "Gift Message", Visible = false, Enabled = false)]
		public string GiftMessage { get; set; }
		/// <inheritdoc cref="GiftMessage"/>
		public abstract class giftMessage : PX.Data.BQL.BqlString.Field<giftMessage> { }
		#endregion
	}
}

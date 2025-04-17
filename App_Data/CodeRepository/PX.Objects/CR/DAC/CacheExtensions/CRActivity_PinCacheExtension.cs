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
using PX.Web.UI;

namespace PX.Objects.CR
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[Serializable]
	public sealed class CRActivityPinCacheExtension : PXCacheExtension<CRActivity>
	{
		#region IsPinned
		public abstract class isPinned : PX.Data.BQL.BqlString.Field<isPinned>
		{
			public static string Pinned = Sprite.Ac.GetFullUrl(Sprite.Ac.Pin);
			public static string Unpinned = Sprite.Control.GetFullUrl(Sprite.Control.Empty);
		}

		[PXDBCustomImage(HeaderImage = (Sprite.AliasAc + "@" + Sprite.Ac.Pin))]
		[PXUIField(DisplayName = "Is Pinned", IsReadOnly = true, Visible = false)]
		public string IsPinned { get; set; }
		#endregion
	}
}

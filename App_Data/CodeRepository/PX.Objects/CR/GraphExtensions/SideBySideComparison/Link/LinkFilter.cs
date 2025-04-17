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
using PX.Data.BQL;

namespace PX.Objects.CR.Extensions.SideBySideComparison.Link
{
	/// <summary>
	/// The filter that is used for linking entities.
	/// </summary>
	[PXHidden]
	public class LinkFilter : IBqlTable
	{
		/// <summary>
		/// Specifies (if set to <see langword="true"/>) that the link should be processed.
		/// </summary>
		[PXBool]
		[PXUnboundDefault(true)]
		[PXUIField(DisplayName = MessagesNoPrefix.LinkEntitiesProcess)]
		public bool? ProcessLink { get; set; }
		public abstract class processLink : BqlBool.Field<processLink> { }

		/// <summary>
		/// The caption under the grid with extra information.
		/// </summary>
		/// <remarks>
		/// Can be made hidden.
		/// </remarks>
		[PXString(IsUnicode = true)]
		[PXUnboundDefault(MessagesNoPrefix.LinkEntitiesCaption)]
		[PXUIField(Enabled = false)]
		public string Caption { get; set; }
		public abstract class caption : BqlString.Field<caption> { }

		/// <summary>
		/// The ID of the entity that is used for linking.
		/// </summary>
		/// <value>
		/// Corresponds to the real ID of the entity, such as
		/// <see cref="BAccount.BAccountID"/> or <see cref="Contact.ContactID"/>.
		/// </value>
		[PXString]
		[PXUIField(DisplayName = "", Visible = false, Enabled = false)]
		public string LinkedEntityID { get; set; }
		public abstract class linkedEntityID : BqlString.Field<linkedEntityID> { }
	}
}

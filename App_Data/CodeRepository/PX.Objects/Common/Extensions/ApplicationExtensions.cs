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

using PX.Objects.Common.Abstractions;

namespace PX.Objects.Common.Extensions
{
	public static class ApplicationExtensions
	{
		/// <summary>
		/// Returns <c>true</c> if the specified document is on the adjusting side of the
		/// specified application, <c>false</c> otherwise. For example, this method will
		/// return <c>true</c> if you call it on an application of a payment to an invoice, 
		/// and specify the payment as the argument.
		/// </summary>
		public static bool IsOutgoingApplicationFor(this IDocumentAdjustment application, IDocumentKey document)
			=> document.DocType == application.AdjgDocType
			&& document.RefNbr == application.AdjgRefNbr;

		/// <summary>
		/// Returns <c>true</c> if the specified document is on the adjusted side of the
		/// specified application, <c>false</c> otherwise. For example, this method will
		/// return <c>true</c> if you call it on an application of a payment to an invoice, 
		/// and specify the invoice as the argument.
		/// </summary>
		public static bool IsIncomingApplicationFor(this IDocumentAdjustment application, IDocumentKey document)
			=> document.DocType == application.AdjdDocType
			&& document.RefNbr == application.AdjdRefNbr;

		/// <summary>
		/// Returns <c>true</c> if and only if the calling application corresponds to the
		/// specified document (on either the adjusting or the adjusted side).
		/// </summary>
		public static bool IsApplicationFor(this IDocumentAdjustment application, IDocumentKey document)
			=> application.IsOutgoingApplicationFor(document)
			|| application.IsIncomingApplicationFor(document);
	}
}

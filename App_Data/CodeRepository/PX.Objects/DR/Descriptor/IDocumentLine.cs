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

using PX.Objects.CM;

namespace PX.Objects.DR.Descriptor
{
	/// <summary>
	/// Represents a line of an AR / AP document,
	/// in parts relevant to Deferred Revenue.
	/// </summary>
	public interface IDocumentLine : IDocumentTran
	{
		/// <summary>
		/// The module of the source document
		/// should either be <see cref="GL.BatchModule.AR"/>
		/// or <see cref="GL.BatchModule.AP"/>.
		/// </summary>
		string Module { get; }
		string DeferredCode { get; }
		int? BranchID { get; }
	}
}

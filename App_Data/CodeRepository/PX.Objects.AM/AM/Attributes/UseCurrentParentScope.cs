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
using PX.Objects.Common;

namespace PX.Objects.AM.Attributes
{
	/// <summary>
	/// Sets <see cref="PXParentAttribute"/> <see cref="PXParentAttribute.UseCurrent"/> which helps avoid a trip to the database when a process is running bulk insert and parent is still only in cache.
	/// Helpful for performance reasons for large sets of data being processed.
	/// </summary>
	public class UseCurrentParentScope : OverrideAttributePropertyScope<PXParentAttribute, bool>
	{
		/// <summary>
		/// Sets <see cref="PXParentAttribute"/> <see cref="PXParentAttribute.UseCurrent"/> which helps avoid a trip to the database when a process is running bulk insert and parent is still only in cache.
		/// Helpful for performance reasons for large sets of data being processed.
		/// </summary>
		/// <param name="cache">Cache containing PXParent fields to set UseCurrent</param>
		/// <param name="useCurrentValue"> Mapped to <see cref="PXParentAttribute.UseCurrent"/> </param>
		/// <param name="fields">Field(s) which contain PXParentAttribute. Leave null to find all and any fields with PXParent</param>
		public UseCurrentParentScope(PXCache cache, bool useCurrentValue, params System.Type[] fields)
			: base(cache,
				fields,
				(attribute, ensureNewNoteIDOnly) => attribute.UseCurrent = ensureNewNoteIDOnly,
				attribute => attribute.UseCurrent,
				useCurrentValue) { }
	}
}

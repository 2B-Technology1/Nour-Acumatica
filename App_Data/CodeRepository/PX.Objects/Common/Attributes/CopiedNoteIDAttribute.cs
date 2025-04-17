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

namespace PX.Objects.Common.Attributes
{
	/// <exclude/>
	public class CopiedNoteIDAttribute : PXNoteAttribute
	{
		protected Type _entityType;

		public Type EntityType
		{
			get => _entityType;
			set => _entityType = value;
		}

		public Type GraphType
		{
			get;
			set;
		}

		public CopiedNoteIDAttribute(Type entityType)
		{
			_entityType = entityType;
			_ForceRetain = true;
		}


		[Obsolete("The constructor is obsolete. Use the constructor without \"searches\" parameter. " +
				  "This " + nameof(CopiedNoteIDAttribute) + " constructor is exactly the same. " +
				  "It does not provide any additional functionality and does not save values of provided fields in the note. " +
				  "The constructor will be removed in a future version of Acumatica ERP.")]
		public CopiedNoteIDAttribute(Type entityType, params Type[] searches)
			: base(searches)
		{
			_entityType = entityType;
			_ForceRetain = true;
		}

		protected override string GetEntityType(PXCache cache, Guid? noteId)
			=> _entityType.FullName;

		protected override string GetGraphType(PXGraph graph)
		{
			if (GraphType != null)
				return GraphType.FullName;

			return base.GetGraphType(graph);
		}
	}
}

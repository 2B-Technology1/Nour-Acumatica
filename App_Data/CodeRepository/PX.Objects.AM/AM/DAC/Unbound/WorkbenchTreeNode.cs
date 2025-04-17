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

namespace PX.Objects.AM
{
	/// <summary>
	/// Transfer object between graph and tree in <see cref="EngineeringWorkbenchMaint"/>
	/// </summary>
	[Serializable]
	[PXHidden]
	public class WorkbenchTreeNode : IBqlTable
	{
		#region IDName (key)
		public abstract class iDName : PX.Data.BQL.BqlString.Field<iDName> { }
		/// <summary>
		/// Key to the node in the tree
		/// </summary>
		[PXString(IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "ID Name")]
		public virtual string IDName { get; set; }
		#endregion
		#region IDNameOriginal
		public abstract class iDNameOriginal : PX.Data.BQL.BqlString.Field<iDNameOriginal> { }

		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "ID Name")]
		public virtual string IDNameOriginal { get; set; }
		#endregion
		#region IDParent
		public abstract class iDParent : PX.Data.BQL.BqlString.Field<iDParent> { }
		/// <summary>
		/// Key to the parents "IDName" field
		/// </summary>
		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Parent ID")]
		public virtual string IDParent { get; set; }
		#endregion
		#region description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		/// <summary>
		/// Displayed value in the tree to the user
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description { get; set; }
		#endregion
		#region Icon
		public abstract class icon : PX.Data.BQL.BqlString.Field<icon> { }

		[PXString(250, IsUnicode = true)]
		public virtual String Icon { get; set; }
		#endregion
		#region IconColor
		public abstract class iconColor : PX.Data.BQL.BqlString.Field<iconColor> { }

		[PXString(250, IsUnicode = true)]
		public virtual String IconColor { get; set; }
		#endregion
		#region Actions
		public abstract class actions : PX.Data.BQL.BqlString.Field<actions> { }
		/// <summary>
		/// Defines the actions available on this node in the tree.
		/// Set value based on in JSON-stringify-friendly format.
		/// Example: "{\"rename\": true, \"createChild\": true, \"createSibling\": true, \"delete\": true}"
		/// </summary>
		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Actions")]
		public virtual string Actions { get; set; }
		#endregion
		#region ExtraColumns
		public abstract class extraColumns : PX.Data.BQL.BqlString.Field<extraColumns> { }
		/// <summary>
		/// A single field is used to return all extra column values for the given tree node.
		/// Set value based on in JSON-stringify-friendly format.
		/// Example: if returning 2 extra fields of string and bool we would return as such:
		///		$"[\"1.00\", \"EA\"]"
		///	In the page we have the ExtraColumnField identified by setting ExtraColumnField="ExtraColumns"
		///	and then also the type definition on the PXTree as the following:
		///		ExtraColumns='[{"tagname": "qp-text-editor", "title": "Nums", "width": 50}, {"tagname": "qp-check-box", "title": "Chs", "width": 50}]' 
		/// </summary>
		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Extra Columns")]
		public virtual string ExtraColumns { get; set; }
		#endregion

		public static implicit operator AddTreeNodeParams(WorkbenchTreeNode workbenchTreeNode)
		{
			return new AddTreeNodeParams
			{
				ID = workbenchTreeNode?.IDName,
				Name = workbenchTreeNode?.Description,
				Actions = workbenchTreeNode?.Actions,
				Icon = workbenchTreeNode?.Icon,
				IconColor = workbenchTreeNode?.IconColor,
				ExtraColumns = workbenchTreeNode?.ExtraColumns
			};
		}
	}
}

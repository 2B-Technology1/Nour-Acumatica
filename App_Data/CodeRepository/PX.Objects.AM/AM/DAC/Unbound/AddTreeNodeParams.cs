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
	public class AddTreeNodeParams
	{
		#region ID (key)
		public abstract class iD : PX.Data.BQL.BqlString.Field<iD> { }
		/// <summary>
		/// Key to the node in the tree
		/// </summary>
		[PXString(IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "ID")]
		public virtual string ID { get; set; }
		#endregion
		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Name")]
		public virtual string Name { get; set; }
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
	}
}

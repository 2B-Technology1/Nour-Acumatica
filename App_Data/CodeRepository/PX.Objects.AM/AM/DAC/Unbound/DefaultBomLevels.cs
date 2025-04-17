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
	[Serializable]
	[PXCacheName("Default BOM Levels")]
	public class DefaultBomLevels : IBqlTable
	{
		#region Item
		public abstract class item : PX.Data.BQL.BqlBool.Field<item> { }
		protected bool? _Item;
		[PXBool]
		[PXUnboundDefault(true)]
		[PXUIField(DisplayName = "Item")]
		public virtual bool? Item
		{
			get
			{
				return this._Item;
			}
			set
			{
				this._Item = value;
			}
		}
		#endregion
		#region Warehouse
		public abstract class warehouse : PX.Data.BQL.BqlBool.Field<warehouse> { }
		protected bool? _Warehouse;
		[PXBool]
		[PXUnboundDefault(typeof(Switch<Case<Where<AMBomItem.siteID.FromCurrent, IsNotNull>, True>, False>))]
		[PXUIField(DisplayName = "Warehouse")]
		public virtual bool? Warehouse
		{
			get
			{
				return this._Warehouse;
			}
			set
			{
				this._Warehouse = value;
			}
		}
		#endregion
		#region SubItem
		public abstract class subItem : PX.Data.BQL.BqlBool.Field<subItem> { }
		protected bool? _SubItem;
		[PXBool]
		[PXUnboundDefault(true)]
		[PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM")]
		public virtual bool? SubItem
		{
			get
			{
				return this._SubItem;
			}
			set
			{
				this._SubItem = value;
			}
		}
		#endregion
	}
}

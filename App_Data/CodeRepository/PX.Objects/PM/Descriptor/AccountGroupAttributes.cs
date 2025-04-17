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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	/// <summary>
	/// Displays all AccountGroups sorted by SortOrder.
	/// </summary>
	/// 
	[PXDBInt()]
	[PXUIField(DisplayName = "Account Group", Visibility = PXUIVisibility.Visible)]
	public class AccountGroupAttribute : PXEntityAttribute
	{
		public const string DimensionName = "ACCGROUP";
		protected Type showGLAccountGroups;


		public AccountGroupAttribute() : this(typeof(Where<PMAccountGroup.groupID, IsNotNull>))
		{
		}

		public AccountGroupAttribute(Type WhereType)
		{
			Type SearchType =
				BqlCommand.Compose(
				typeof(Search<,,>),
				typeof(PMAccountGroup.groupID),
				WhereType,
				typeof(OrderBy<Asc<PMAccountGroup.sortOrder>>)
				);

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(PMAccountGroup.groupCD),
				typeof(PMAccountGroup.groupCD), typeof(PMAccountGroup.description), typeof(PMAccountGroup.type), typeof(PMAccountGroup.isActive));
			select.DescriptionField = typeof(PMAccountGroup.description);
			select.CacheGlobal = true;

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}


	/// <summary>
	/// Base attribute for AccountGroupCD field. Aggregates PXFieldAttribute, PXUIFieldAttribute and DimensionSelector without any restriction.
	/// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Account Group", Visibility = PXUIVisibility.Visible)]
	public class AccountGroupRawAttribute : PXEntityAttribute
	{
		public AccountGroupRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(AccountGroupAttribute.DimensionName);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#region accountGroupType
	public sealed class accountGroupType : PX.Data.BQL.BqlString.Constant<accountGroupType>
	{
		public accountGroupType()
			: base(typeof(PMAccountGroup).FullName)
		{
		}
	}
	#endregion
}

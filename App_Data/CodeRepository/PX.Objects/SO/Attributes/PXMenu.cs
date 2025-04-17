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
using System.Collections;
using System.Linq;
using System.Reflection;

namespace PX.Objects.SO
{
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class PXMenuItemAttribute : Attribute
	{
		public string Menu { get; }

		public int MenuItemID { get; }

		public PXMenuItemAttribute(string menu, int menuItemId)
		{
			Menu = menu;
			MenuItemID = menuItemId;
		}
	}

	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
	public class PXMenu<TNode> : PXAction<TNode> where TNode : class, IBqlTable, new()
	{
		protected PXMenu(PXGraph graph)
			: base(graph)
		{
		}

		public PXMenu(PXGraph graph, string name) : base(graph, name)
		{
		}

		public PXMenu(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		public virtual IEnumerable Press(PXAdapter adapter, int? menuItemID, string actionName = null)
		{
			if (menuItemID.HasValue && TryGetMenuItem(adapter.View.Graph, (int)menuItemID, out PXMenuItem<TNode> menuItem))
				return menuItem.Press(adapter);
			else
				return adapter.Get();
		}

		public virtual bool TryGetMenuItem(PXGraph graph, int menuItemID, out PXMenuItem<TNode> action)
		{
			action = graph.Actions.Values.OfType<PXMenuItem<TNode>>().FirstOrDefault(
				x => x.MenuItemID == menuItemID && string.Equals(x.Menu, _Name, StringComparison.OrdinalIgnoreCase));
			return action != null;
		}
	}

	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
	public class PXMenuItem<TNode> : PXAction<TNode> where TNode : class, IBqlTable, new()
	{
		public string Menu { get; private set; }

		public int MenuItemID { get; private set; }

		protected PXMenuItem(PXGraph graph)
			: base(graph)
		{
		}

		public PXMenuItem(PXGraph graph, string name):base(graph, name)
		{
			SetProperties((PXButtonDelegate)Handler);
		}

		public PXMenuItem(PXGraph graph, Delegate handler) : base(graph, handler)
		{
			SetProperties(_Handler);
		}

		protected virtual void SetProperties(Delegate handler)
		{
			var menuItem = handler.Method.GetCustomAttribute<PXMenuItemAttribute>(false);
			if (menuItem != null)
			{
				MenuItemID = menuItem.MenuItemID;
				Menu = menuItem.Menu;
			}
		}
	}
}

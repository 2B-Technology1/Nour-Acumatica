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
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.Common;
using PX.Objects.Common.GraphExtensions.Abstract;

namespace PX.Objects.IN.GraphExtensions
{
	/// <exclude/>
	[PXCacheName(Messages.InventoryLinkFilter)]
	public class InventoryLinkFilter : IBqlTable
	{
		#region InventoryID
		[Inventory(IsKey = true)]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		// Acuminator disable once PX1076 CallToInternalApi [Justification]
		[PXUIField(DisplayName = "Description", Enabled = false)]
		public abstract class AttachedInventoryDescription<TSelf, TGraph> : PXFieldAttachedTo<InventoryLinkFilter>.By<TGraph>.AsString.Named<TSelf>
			where TGraph : PXGraph
			where TSelf : PXFieldAttachedTo<InventoryLinkFilter>.By<TGraph>.AsString
		{
			public override string GetValue(InventoryLinkFilter Row) => InventoryItem.PK.Find(Base, Row?.InventoryID)?.Descr;
		}
	}

	public abstract class InventoryLinkFilterExtensionBase<TGraph, TGraphFilter, TGraphFilterInventoryID> : EntityLinkFilterExtensionBase<TGraph, TGraphFilter, TGraphFilterInventoryID, InventoryLinkFilter, InventoryLinkFilter.inventoryID, int?>
		where TGraph : PXGraph, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
		where TGraphFilter : class, IBqlTable, new()
		where TGraphFilterInventoryID : class, IBqlField, IImplement<IBqlInt>
	{
		protected override string EntityViewName => nameof(SelectedItems);

		[PXVirtualDAC]
		[PXImport]
		[PXReadOnlyView]
		public PXSelect<InventoryLinkFilter> SelectedItems;
		public IEnumerable selectedItems() => GetEntities();

		[PXButton, PXUIField(DisplayName = "List")]
		public virtual void selectItems() => SelectedItems.AskExt();
		public PXAction<TGraphFilter> SelectItems;

		public abstract class AttachedInventoryDescription<TSelf> : InventoryLinkFilter.AttachedInventoryDescription<TSelf, TGraph>
			where TSelf : PXFieldAttachedTo<InventoryLinkFilter>.By<TGraph>.AsString
		{
		}
	}
}

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.EP;
using PX.TM;

namespace PX.Objects.EP
{
	public class EquipmentTimecardPrimary : PXGraph<EquipmentTimecardPrimary>
	{
		[PXFilterable()]
		public PXSelectJoin<EPEquipmentTimeCard,
			LeftJoin<EPEquipmentTimeCardSpentTotals, On<EPEquipmentTimeCardSpentTotals.timeCardCD, Equal<EPEquipmentTimeCard.timeCardCD>>,
			LeftJoin<EPEquipmentTimeCardBillableTotals, On<EPEquipmentTimeCardBillableTotals.timeCardCD, Equal<EPEquipmentTimeCard.timeCardCD>>,
			LeftJoin<EPEquipmentTimeCardEx, On<EPEquipmentTimeCard.timeCardCD, Equal<EPEquipmentTimeCardEx.origTimeCardCD>>>>>,
			Where<EPEquipmentTimeCardEx.timeCardCD, IsNull>,
			OrderBy<Desc<EPEquipmentTimeCard.timeCardCD>>> Items;
		
		#region CRUD Actions

		public PXAction<EPEquipmentTimeCard> create;
		[PXButton(Tooltip = Messages.AddNewTimecardToolTip, ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		[PXEntryScreenRights(typeof(EPTimeCard), nameof(EquipmentTimeCardMaint.Insert))]
		protected virtual void Create()
		{
			using (new PXPreserveScope())
			{
				EquipmentTimeCardMaint graph = (EquipmentTimeCardMaint)PXGraph.CreateInstance(typeof(EquipmentTimeCardMaint));
				graph.Clear(PXClearOption.ClearAll);
				graph.Document.Insert();
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
			}
		}

		public PXAction<EPEquipmentTimeCard> update;
		[PXButton(Tooltip = Messages.EditTimecardToolTip, ImageKey = PX.Web.UI.Sprite.Main.RecordEdit)]
		protected virtual void Update()
		{
			if (Items.Current == null) return;

			PXRedirectHelper.TryRedirect(this, Items.Current, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<EPEquipmentTimeCard> delete;
		[PXDeleteButton(Tooltip = Messages.DeleteTimecardToolTip)]
		[PXEntryScreenRights(typeof(EPEquipmentTimeCard))]
		protected void Delete()
		{
			if (Items.Current == null) return;

			using (new PXPreserveScope())
			{
				EquipmentTimeCardMaint graph = (EquipmentTimeCardMaint)PXGraph.CreateInstance(typeof(EquipmentTimeCardMaint));
				graph.Clear(PXClearOption.ClearAll);
				graph.Document.Current = graph.Document.Search<EPEquipmentTimeCard.timeCardCD>(Items.Current.TimeCardCD);
				graph.Delete.Press();
			}
		}

		#endregion


		public partial class EPEquipmentTimeCardEx : EPEquipmentTimeCard
		{
			public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }
			public new abstract class origTimeCardCD : PX.Data.BQL.BqlString.Field<origTimeCardCD> { }
		}
	}
}

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Common;
using PX.Objects.GL;
using PX.Data.BQL.Fluent;
using PX.Objects.CT;
using PX.Data.BQL;

namespace PX.Objects.PM
{
	[Serializable]
	public class CostCodeMaint : PXGraph<CostCodeMaint>, PXImportAttribute.IPXPrepareItems
	{
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<PMCostCode.isProjectOverride> e)
		{
		}

		[PXImport(typeof(PMCostCode))]
		[PXViewName(Messages.CostCode)]
		[PXFilterable]
		public PXSelect<PMCostCode> Items;
		public PXSavePerRow<PMCostCode> Save;
		public PXCancel<PMCostCode> Cancel;

		public ChangeCostCode changeID;
		[PXUIField(DisplayName = "Change ID", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable ChangeID(PXAdapter adapter)
		{
			return (new ChangeCostCode(this, "changeID")).Press(adapter);
		}
				
		public PXSetup<PMSetup> Setup;

		public CostCodeMaint()
		{
			PMSetup setup = Setup.Current;
		}

		protected virtual void _(Events.RowDeleting<PMCostCode> e)
		{
			if ( e.Row.IsDefault == true )
			{
				throw new PXException(Messages.CannotDeleteDefaultCostCode);
			}
		}

		protected virtual void _(Events.RowSelected<PMCostCode> e)
		{
			var costCode = e.Row;
			if(costCode != null)
			{
				bool isEnabled = PXContext.PXIdentity.User.IsInRole(PredefinedRoles.ProjectAccountant);
				PXUIFieldAttribute.SetEnabled<PMCostCode.isActive>(e.Cache, costCode, isEnabled);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMCostCode.isActive> e)
		{
			var row = e.Row as PMCostCode;
			if (row != null)
			{
				bool? oldValue = e.OldValue as bool?;
				bool? newValue = e.NewValue as bool?;
				if (oldValue == true && newValue == false)
				{
					if (row.IsDefault == true)
					{
						throw new PXSetPropertyException<PMCostCode.isActive>(Messages.CannotDeactivateDefaultCostCode, PXErrorLevel.RowError);
					}

					var budget = SelectFrom<PMBudget>
						.InnerJoin<PMTask>.On<PMBudget.projectTaskID.IsEqual<PMTask.taskID>
							.And<PMBudget.projectID.IsEqual<PMTask.projectID>>>
						.InnerJoin<PMProject>.On<PMBudget.projectID.IsEqual<PMProject.contractID>>
						.Where<PMTask.status.IsEqual<ProjectTaskStatus.active>
							.And<PMProject.baseType.IsEqual<CTPRType.project>
							.And<PMProject.nonProject.IsEqual<False>>
							.And<PMBudget.costCodeID.IsEqual<P.AsInt>>>>
						.View
						.SelectSingleBound(this, null, row.CostCodeID)
						.FirstOrDefault();

					if (budget != null)
					{
						string costCodeCD = CostCodeDimensionSelectorAttribute.FormatValueByMask<PMCostCode.costCodeCD>(e.Cache, row, row.CostCodeCD);
						throw new PXSetPropertyException<PMCostCode.isActive>(Messages.CannotDeactivateCostCode, PXErrorLevel.RowError, costCodeCD, budget.GetItem<PMProject>().ContractCD);
					}
				}
			}
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (IsKeyChanged(keys, values))
			{
				throw new PXException(Messages.CannotModifyCostCode);
			}

			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public virtual bool IsKeyChanged(IDictionary keys, IDictionary values)
		{			
			if (keys.Contains(nameof(PMCostCode.CostCodeCD)))
			{
				string keyValue = (string) keys[nameof(PMCostCode.CostCodeCD)];
				if (!string.IsNullOrEmpty(keyValue))
				{
					if (values.Contains(nameof(PMCostCode.CostCodeCD)))
					{
						string valValue = (string)values[nameof(PMCostCode.CostCodeCD)];

						if (keyValue != valValue)
						{
							return true;
						}
					}
				}
			}

			return false;
			
		}

		#region PMImport Implementation
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }
		#endregion
	}

	public class ChangeCostCode : PXChangeID<PMCostCode, PMCostCode.costCodeCD>
	{
		public ChangeCostCode(PXGraph graph, string name)
			: base(graph, name)
		{
		}

		public ChangeCostCode(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
		}

		protected override IEnumerable Handler(PXAdapter adapter)
		{
			if (this.Graph.Views[ChangeIdDialogView].Answer == WebDialogResult.None)
				this.Graph.Views[ChangeIdDialogView].Cache.Clear();

			string newcd;
			if (adapter.View.Cache.Current != null && adapter.View.Cache.GetStatus(adapter.View.Cache.Current) != PXEntryStatus.Inserted)
			{
				var dialogResult = adapter.View.Cache.Graph.Views[ChangeIdDialogView].AskExt();
				if ((dialogResult == WebDialogResult.OK || (dialogResult == WebDialogResult.Yes && this.Graph.IsExport))
					&& !String.IsNullOrWhiteSpace(newcd = GetNewCD(adapter)))
				{
					ChangeCD(adapter.View.Cache, GetOldCD(adapter), newcd);
				}
			}

			if (this.Graph.IsContractBasedAPI)
				this.Graph.Actions.PressSave();
			
			return adapter.Get();
		}
	}
}

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
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.PM.ChangeRequest.GraphExtensions
{
	public class TemplateMaintExt : PXGraphExtension<PX.Objects.PM.TemplateMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.changeRequest>();
		}

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Markup)]
		public PXSelect<PMMarkup, Where<PMMarkup.projectID, Equal<Current<PMProject.contractID>>>> Markups;

		
		#region DAC Attributes Override

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXDBInt(IsKey = true)]
		protected virtual void PMMarkup_ProjectID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(PMTask.taskID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void PMMarkup_TaskID_CacheAttached(PXCache sender) { }

		#endregion

		//stores Original TaskID by Markup LineNbr.
		private Dictionary<int, int?> markupMapping = new Dictionary<int, int?>();
		protected virtual void _(Events.RowInserted<PMProject> e)
		{
			int? sourceProjectID = ProjectDefaultAttribute.NonProject();
			if (Base.IsCopyPaste && Base.CopySource != null && Base.CopySource.Project.Current != null)
			{
				sourceProjectID = Base.CopySource.Project.Current.ContractID;
			}

			var select = new PXSelect<PMMarkup, Where<PMMarkup.projectID, Equal<Required<PMMarkup.projectID>>>>(Base);

			markupMapping.Clear();
			bool MarkupsCacheIsDirty = Markups.Cache.IsDirty;
			foreach (PMMarkup setup in select.Select(sourceProjectID))
			{
				PMMarkup markup = new PMMarkup();
				markup.ProjectID = e.Row.ContractID;
				markup.Type = setup.Type;
				markup.Description = setup.Description;
				markup.Value = setup.Value;
				markup.AccountGroupID = setup.AccountGroupID;
				markup.CostCodeID = setup.CostCodeID;
				markup.InventoryID = setup.InventoryID;

				markup = Markups.Insert(markup);
				markupMapping.Add(markup.LineNbr.Value, setup.TaskID);
			}

			Markups.Cache.IsDirty = MarkupsCacheIsDirty;
		}

		protected virtual void _(Events.RowSelected<PMProject> e)
		{
			PXUIFieldAttribute.SetVisible<PMMarkup.inventoryID>(Markups.Cache, null, e.Row.BudgetLevel == BudgetLevels.Item);

			bool visible = CostCodeAttribute.UseCostCode() && e.Row.BudgetLevel == BudgetLevels.CostCode;
			PXUIFieldAttribute.SetVisible<PMMarkup.costCodeID>(Markups.Cache, null, visible);
		}
		
		[PXOverride]
		public virtual void OnCopyPasteTasksInserted(TemplateMaint target, Dictionary<int, int> taskMap)
		{
			TemplateMaintExt ext = target.GetExtension<TemplateMaintExt>();
			if (ext != null)
			{
				foreach (PMMarkup markup in ext.Markups.Select())
				{
					int? originalTaskID;
					if (ext.markupMapping.TryGetValue(markup.LineNbr.Value, out originalTaskID) && originalTaskID != null)
					{
						markup.TaskID = taskMap[originalTaskID.Value];
					}
				}
			}
			
		}
	}
}

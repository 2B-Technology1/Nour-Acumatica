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
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.CT;
using PX.Objects.CR;

namespace PX.Objects.PM
{
	public class TemplateTaskMaint : PXGraph<TemplateTaskMaint>
	{
        #region DAC Attributes Override

        #region PMTask
        [Project(typeof(Where<PMProject.baseType, Equal<CTPRType.projectTemplate>, And<PMProject.nonProject, Equal<False>>>), DisplayName = "Project Template ID", IsKey = true)]
        [PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
        [PXDefault(typeof(PMProject.contractID))]
        protected virtual void _(Events.CacheAttached<PMTask.projectID> e)
        {
        }

        [PXDimensionSelector(ProjectTaskAttribute.DimensionName,
            typeof(Search<PMTask.taskCD, Where<PMTask.projectID, Equal<Current<PMTask.projectID>>>>),
            typeof(PMTask.taskCD),
            typeof(PMTask.taskCD), typeof(PMTask.locationID), typeof(PMTask.description), typeof(PMTask.status), DescriptionField = typeof(PMTask.description))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Project Template Task ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void _(Events.CacheAttached<PMTask.taskCD> e)
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ProjectTaskStatus.Active)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.Invisible, Visible = false)]
        protected virtual void _(Events.CacheAttached<PMTask.status> e)
        {
        }

		[PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Rate Table")]
		[PXSelector(typeof(PMRateTable.rateTableID))]
		protected virtual void _(Events.CacheAttached<PMTask.rateTableID> e) { }
		#endregion
		        
        #endregion

		#region Views/Selects

		public PXSelectJoin<PMTask, 
			LeftJoin<PMProject, 
				On<PMTask.projectID, Equal<PMProject.contractID>>>, 
			Where<PMProject.nonProject, Equal<False>, 
				And<PMProject.baseType, Equal<CTPRType.projectTemplate>>>> Task;
		public PXSelect<PMTask, Where<PMTask.projectID, Equal<Current<PMTask.projectID>>, And<PMTask.taskID, Equal<Current<PMTask.taskID>>>>> TaskProperties;
        public PXSelect<PMRecurringItem,
            Where<PMRecurringItem.projectID, Equal<Current<PMTask.projectID>>,
            And<PMRecurringItem.taskID, Equal<Current<PMTask.taskID>>>>> BillingItems;
       [PXViewName(Messages.TaskAnswers)]
		public CRAttributeList<PMTask> Answers;
		public PXSetup<PMSetup> Setup;
		public PXSetup<Company> CompanySetup;
        #endregion

        #region	Actions/Buttons

        public PXSave<PMTask> Save;
		public PXCancel<PMTask> Cancel;
		public PXInsert<PMTask> Insert;
		public PXDelete<PMTask> Delete;
		public PXFirst<PMTask> First;
		public PXPrevious<PMTask> previous;
		public PXNext<PMTask> next;
		public PXLast<PMTask> Last;
						
        #endregion		 

        public TemplateTaskMaint()
		{
			if (Setup.Current == null)
			{
				throw new PXException(Messages.SetupNotConfigured);
			}

			
		}

		#region Event Handlers
		
		protected virtual void _(Events.RowSelected<PMTask> e)
		{
			if (e.Row != null)
			{
				PMProject prj = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.SelectSingleBound(this, new object[] { e.Row });
				PXUIFieldAttribute.SetEnabled<PMTask.autoIncludeInPrj>(e.Cache, e.Row, prj != null && prj.NonProject != true);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTask, PMTask.isDefault> e)
		{
			if (e.Row.IsDefault == true)
			{
				var select = new PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>>>(this);
				foreach (PMTask task in select.Select(e.Row.ProjectID))
				{
					if (task.IsDefault == true && task.TaskID != e.Row.TaskID)
					{
						Task.Cache.SetValue<PMTask.isDefault>(task, false);
						Task.Cache.SmartSetStatus(task, PXEntryStatus.Updated);
					}
				}
			}
		}

		protected virtual void PMTask_CustomerID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PMTask row = e.Row as PMTask;
            if(row == null) return;

            PMProject prj = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.SelectSingleBound(this, new object[] { row });
            if(prj != null && prj.NonProject == true)
            {
                e.Cancel = true;
            }
        }
		
		protected virtual void PMTask_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.Select(this);
			if (row != null && project != null)
			{
				row.BillingID = project.BillingID;
				row.DefaultSalesAccountID = project.DefaultSalesAccountID;
				row.DefaultSalesSubID = project.DefaultSalesSubID;
				row.DefaultExpenseAccountID = project.DefaultExpenseAccountID;
				row.DefaultExpenseSubID = project.DefaultExpenseSubID;

			}
		}
		
		protected virtual void PMTask_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row != null)
			{
				sender.SetDefaultExt<PMTask.customerID>(e.Row);
				sender.SetDefaultExt<PMTask.defaultSalesAccountID>(e.Row);
				sender.SetDefaultExt<PMTask.defaultSalesSubID>(e.Row);
				sender.SetDefaultExt<PMTask.defaultExpenseAccountID>(e.Row);
				sender.SetDefaultExt<PMTask.defaultExpenseSubID>(e.Row);
			}
		}

        protected virtual void _(Events.FieldUpdated<PMRecurringItem, PMRecurringItem.inventoryID> e)
        {
			e.Cache.SetDefaultExt<PMRecurringItem.description>(e.Row);
			e.Cache.SetDefaultExt<PMRecurringItem.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMRecurringItem.amount>(e.Row);
		}
	

		protected virtual void _(Events.FieldDefaulting<PMRecurringItem, PMRecurringItem.amount> e)
		{
			if (e.Row == null) return;
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, e.Row.InventoryID);
			if (item != null)
			{
				e.NewValue = item.BasePrice;
			}
		}

        protected virtual void _(Events.RowSelected<PMRecurringItem> e)
        {
            if (e.Row != null && Task.Current != null)
            {
                PXUIFieldAttribute.SetEnabled<PMRecurringItem.included>(e.Cache, e.Row, Task.Current.IsActive != true);
            }
        }

        #endregion
    }
}

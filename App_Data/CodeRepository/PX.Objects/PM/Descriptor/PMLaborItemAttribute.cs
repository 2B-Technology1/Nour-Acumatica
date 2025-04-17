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
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	[PXDBInt]
	[PXUIField(DisplayName = "Labor Item")]
	public class PMLaborItemAttribute : PXEntityAttribute, IPXFieldDefaultingSubscriber
	{
		protected Type projectField;
		protected Type earningTypeField;
		protected Type employeeSearch;

		public PMLaborItemAttribute(Type project, Type earningType, Type employeeSearch)
		{
			this.projectField = project;
			this.earningTypeField = earningType;
			this.employeeSearch = employeeSearch;

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<Match<Current<AccessInfo.userName>>>>>), typeof(InventoryItem.inventoryCD));
			
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPEmployee employee = null;

			if (employeeSearch != null)
			{
				BqlCommand cmd = BqlCommand.CreateInstance(employeeSearch);
				PXView view = new PXView(sender.Graph, false, cmd);

				employee = view.SelectSingle() as EPEmployee;
			}

			if (employee != null)
			{
				int? projectID = (int?)sender.GetValue(e.Row, projectField.Name);
				string earningType = (string)sender.GetValue(e.Row, earningTypeField.Name);
				int? laborItem = (int?)sender.GetValue(e.Row, FieldName);

				if (sender.Graph.IsImportFromExcel && laborItem != null)
				{
					e.NewValue = laborItem;
				}
				else
				{
				e.NewValue = GetDefaultLaborItem(sender.Graph, employee, earningType, projectID);
			}
		}
		}

		public virtual int? GetDefaultLaborItem(PXGraph graph, EPEmployee employee, string earningType, int? projectID)
		{
			if (employee == null)
				return null;

			int? result = null;

			if (ProjectDefaultAttribute.IsProject(graph, projectID))
			{
				result = EPContractRate.GetProjectLaborClassID(graph, projectID.Value, employee.BAccountID.Value, earningType);
			}

			if (result == null)
			{
				result = EPEmployeeClassLaborMatrix.GetLaborClassID(graph, employee.BAccountID, earningType);
			}

			if (result == null)
			{
				result = employee.LabourItemID;
			}

			return result;
		}
	}
}

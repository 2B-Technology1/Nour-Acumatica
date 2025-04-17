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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	[PXRestrictor(typeof(Where<PMUnion.isActive, Equal<True>>), PM.Messages.InactiveUnion, typeof(PMUnion.unionID))]
	[PXDBString(PMUnion.unionID.Length, IsUnicode = true)]
	[PXUIField(DisplayName = "Union Local", FieldClass = nameof(FeaturesSet.Construction))]
	public class PMUnionAttribute : PXEntityAttribute, IPXFieldDefaultingSubscriber
	{
		protected Type projectField;
		protected Type employeeSearch;

		public PMUnionAttribute(Type project, Type employeeSearch)
		{
			this.projectField = project;
			this.employeeSearch = employeeSearch;
			
			PXSelectorAttribute select = new PXSelectorAttribute(typeof(Search<PMUnion.unionID>), DescriptionField = typeof(PMUnion.description));
			
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPEmployee employee = null;

			if (employeeSearch != null)
			{
				BqlCommand cmd = BqlCommand.CreateInstance(employeeSearch);
				PXView view = new PXView(sender.Graph, false, cmd);

				employee = view.SelectSingle() as EPEmployee;
			}

			if (employee != null && !string.IsNullOrEmpty(employee.UnionID))
			{
				HashSet<string> validUnions = new HashSet<string>();
				if (projectField != null)
				{
					int? projectID = (int?)sender.GetValue(e.Row, projectField.Name);

					if (ProjectDefaultAttribute.IsProject(sender.Graph, projectID))
					{
						var select = new PXSelect<PMProjectUnion, Where<PMProjectUnion.projectID, Equal<Required<PMProjectUnion.projectID>>>>(sender.Graph);
						foreach (PMProjectUnion union in select.Select(projectID))
						{
							validUnions.Add(union.UnionID);
						}
					}
				}
				
				if (validUnions.Count == 0 || validUnions.Contains(employee.UnionID))
				{
					e.NewValue = employee.UnionID;
				}
			}
		}
	}
}

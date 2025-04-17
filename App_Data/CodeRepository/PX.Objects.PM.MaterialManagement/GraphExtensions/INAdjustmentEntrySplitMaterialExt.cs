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
using PX.Objects.IN;
using System;
using System.Linq;

namespace PX.Objects.PM.MaterialManagement
{
    public class INAdjustmentEntrySplitMaterialExt : PXGraphExtension<INAdjustmentEntrySplit, INAdjustmentEntry>
    {
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
		}
				
		[PXOverride]
		public virtual INTran SplitTransaction(INTran source,
			Func<INTran, INTran> baseMethod)
		{
			if (source.ProjectID == ProjectDefaultAttribute.NonProject() ||
				source.TaskID != null)
			{
				INTran newTran = baseMethod(source);
								
				if (source.Qty < 0)
				{
					IStatus availability = Base.FindImplementation<GraphExtensions.ItemAvailability.INAdjustmentItemAvailabilityProjectExtension>()
						.FetchWithBaseUOMProject(source, false, source.CostCenterID);
					decimal overflow = source.Qty.GetValueOrDefault() + availability.QtyOnHand.GetValueOrDefault();
					if (overflow < 0)
					{
						source.Qty = -availability.QtyOnHand;
						Base.transactions.Update(source);

						newTran.Qty = overflow;
					}
				}

				return newTran;
			}

			return null;
		}

		protected virtual void _(Events.FieldVerifying<INTran, INTran.projectID> e)
        {
			if (!string.IsNullOrEmpty(e.Row.PIID) && 
				GetAccountingMode((int?)e.NewValue) == ProjectAccountingModes.ProjectSpecific &&
				e.ExternalCall == true)
            {
				var ex = new PXSetPropertyException(Messages.SpecificProjectNotSupported);
				PMProject project = PMProject.PK.Find(Base, (int?)e.NewValue);
				ex.ErrorValue = project.ContractCD;

				throw ex;
            }
        }

		/// <summary>
		/// Overrides <see cref="INRegisterEntryBase.IsProjectTaskEnabled(INTran)" />
		/// </summary>
		[PXOverride]
		public virtual (bool? Project, bool? Task) IsProjectTaskEnabled(INTran row,
			Func<INTran, (bool? Project, bool? Task)> baseMethod)
		{
			var result = baseMethod(row);

			if (!string.IsNullOrEmpty(row.PIID))
			{
				string mode = GetAccountingMode(row.ProjectID);

				return ((result.Project ?? true) && mode != ProjectAccountingModes.ProjectSpecific,
					(result.Task ?? true) && mode != ProjectAccountingModes.ProjectSpecific);
			}

			return result;
		}

		protected string GetAccountingMode(int? projectID)
		{
			if (projectID != null)
			{
				PMProject project = PMProject.PK.Find(Base, projectID);
				if (project != null && project.NonProject != true)
				{
					return project.AccountingMode;
				}
			}

			return ProjectAccountingModes.Valuated;
		}

		[PXOverride]
		public INTran InsertNewSplit(INTran newLine, Func<INTran, INTran> baseMethod)
        {
			INTran tran = baseMethod(newLine);
			if (GetAccountingMode(tran.ProjectID) != ProjectAccountingModes.ProjectSpecific)
			{
				tran.ProjectID = null;
				tran.TaskID = null;
			}

			return tran;
        }
	} 
}

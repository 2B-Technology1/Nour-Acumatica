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
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Extends the PMProject graphs to adapt business logic / UI to work with PR fields
	/// </summary>
	/// <typeparam name="TGraph">Base graph for extends</typeparam>
	/// <typeparam name="TPrimary">DAC for extends</typeparam>
	public abstract class PRxProjectGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region Views
		public PXSelect<PRSetup> SetupPreference;
		#endregion

		#region Events

		protected virtual void _(Events.RowPersisting<TPrimary> e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
				return;

			PMProjectExtension entity = e.Row.GetExtension<PMProjectExtension>();
			PRSetup setup = SetupPreference.SelectSingle();

			if (setup == null || entity == null)
				return;

			if (setup.EarningsAcctDefault == PREarningsAcctSubDefault.MaskProject && entity.EarningsAcctID == null)
			{
				e.Cache.RaiseExceptionHandling<PMProjectExtension.earningsAcctID>(e.Row, entity.EarningsAcctID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMProjectExtension.earningsAcctID>(e.Cache)));
			}

			var earningsSubMaskIsProject = PRSetupMaint.SubMaskContainsValue(SetupPreference.Cache, setup, typeof(PRSetup.earningsSubMask), setup.EarningsSubMask, GLAccountSubSource.Project);
			if (earningsSubMaskIsProject == true && entity.EarningsSubID == null)
			{
				e.Cache.RaiseExceptionHandling<PMProjectExtension.earningsSubID>(e.Row, entity.EarningsSubID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMProjectExtension.earningsSubID>(e.Cache)));
			}

			if (setup.BenefitExpenseAcctDefault == PREarningsAcctSubDefault.MaskProject && entity.BenefitExpenseAcctID == null)
			{
				e.Cache.RaiseExceptionHandling<PMProjectExtension.benefitExpenseAcctID>(e.Row, entity.BenefitExpenseAcctID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMProjectExtension.benefitExpenseAcctID>(e.Cache)));
			}

			var benefitExpenseSubMaskIsProject = PRSetupMaint.SubMaskContainsValue(SetupPreference.Cache, setup, typeof(PRSetup.benefitExpenseSubMask), setup.BenefitExpenseSubMask, GLAccountSubSource.Project);
			if (benefitExpenseSubMaskIsProject == true && entity.BenefitExpenseSubID == null)
			{
				e.Cache.RaiseExceptionHandling<PMProjectExtension.benefitExpenseSubID>(e.Row, entity.BenefitExpenseSubID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMProjectExtension.benefitExpenseSubID>(e.Cache)));
			}

			if (setup.TaxExpenseAcctDefault == PREarningsAcctSubDefault.MaskProject && entity.TaxExpenseAcctID == null)
			{
				e.Cache.RaiseExceptionHandling<PMProjectExtension.taxExpenseAcctID>(e.Row, entity.TaxExpenseAcctID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMProjectExtension.taxExpenseAcctID>(e.Cache)));
			}

			var taxExpenseSubMaskIsProject = PRSetupMaint.SubMaskContainsValue(SetupPreference.Cache, setup, typeof(PRSetup.taxExpenseSubMask), setup.TaxExpenseSubMask, GLAccountSubSource.Project);
			if (taxExpenseSubMaskIsProject == true && entity.TaxExpenseSubID == null)
			{
				e.Cache.RaiseExceptionHandling<PMProjectExtension.taxExpenseSubID>(e.Row, entity.TaxExpenseSubID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMProjectExtension.taxExpenseSubID>(e.Cache)));
			}

			if (setup.PTOExpenseAcctDefault == PREarningsAcctSubDefault.MaskProject && entity.PTOExpenseAcctID == null)
			{
				e.Cache.RaiseExceptionHandling<PMProjectExtension.ptoExpenseAcctID>(e.Row, entity.PTOExpenseAcctID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMProjectExtension.ptoExpenseAcctID>(e.Cache)));
			}

			var ptoExpenseSubMaskIsProject = PRSetupMaint.SubMaskContainsValue(SetupPreference.Cache, setup, typeof(PRSetup.ptoExpenseSubMask), setup.PTOExpenseSubMask, GLAccountSubSource.Project);
			if (ptoExpenseSubMaskIsProject == true && entity.PTOExpenseSubID == null)
			{
				e.Cache.RaiseExceptionHandling<PMProjectExtension.ptoExpenseSubID>(e.Row, entity.PTOExpenseSubID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMProjectExtension.ptoExpenseSubID>(e.Cache)));
			}
		}

		#endregion

		public virtual void Copy(PMProjectExtension dstEntity, PMProjectExtension srcEntity)
		{
			dstEntity.EarningsAcctID = srcEntity.EarningsAcctID;
			dstEntity.EarningsSubID = srcEntity.EarningsSubID;
			dstEntity.BenefitExpenseAcctID = srcEntity.BenefitExpenseAcctID;
			dstEntity.BenefitExpenseSubID = srcEntity.BenefitExpenseSubID;
			dstEntity.TaxExpenseAcctID = srcEntity.TaxExpenseAcctID;
			dstEntity.TaxExpenseSubID = srcEntity.TaxExpenseSubID;
			dstEntity.PTOExpenseAcctID = srcEntity.PTOExpenseAcctID;
			dstEntity.PTOExpenseSubID = srcEntity.PTOExpenseSubID;
		}
	}

	/// <summary>
	/// Extends the ProjectEntry graph to adapt business logic / UI to work with PR fields
	/// </summary>
	public class PRxProjectEntry : PXGraphExtension<ProjectEntry>
	{
		public class PRxProjectGraph : PRxProjectGraph<ProjectEntry, PMProject>
		{
			public static bool IsActive()
			{
				return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
			}
		}

		public class PRxTaskGraph : PRxTaskGraph<ProjectEntry, PMTask>
		{
			public static bool IsActive()
			{
				return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
			}
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		[PXOverride]
		public virtual void OnCopyPasteTaskInserted(PMProject target, PMTask dstTask, PMTask srcTask)
		{
			PRxPMTask srcTaskExtension = srcTask.GetExtension<PRxPMTask>();
			PRxPMTask dstTaskExtension = dstTask.GetExtension<PRxPMTask>();
			Base.GetExtension<PRxTaskGraph>().Copy(dstTaskExtension, srcTaskExtension);
		}

		[PXOverride]
		public virtual void OnCopyPasteCompleted(PMProject dstProject, PMProject srcProject)
		{
			PMProjectExtension srcProjectExtension = srcProject.GetExtension<PMProjectExtension>();
			PMProjectExtension dstProjectExtension = dstProject.GetExtension<PMProjectExtension>();
			Base.GetExtension<PRxProjectGraph>().Copy(dstProjectExtension, srcProjectExtension);
		}
	}
}

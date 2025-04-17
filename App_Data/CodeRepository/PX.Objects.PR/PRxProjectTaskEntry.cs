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
	/// Extends the PMTask graphs to adapt business logic / UI to work with PR fields
	/// </summary>
	/// <typeparam name="TGraph">Base graph for extends</typeparam>
	/// <typeparam name="TPrimary">DAC for extends</typeparam>
	public abstract class PRxTaskGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
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

			PRxPMTask entity = e.Row.GetExtension<PRxPMTask>();
			PRSetup setup = SetupPreference.SelectSingle();

			if (setup == null || entity == null)
				return;

			if (setup.EarningsAcctDefault == PREarningsAcctSubDefault.MaskTask && entity.EarningsAcctID == null)
			{
				e.Cache.RaiseExceptionHandling<PRxPMTask.earningsAcctID>(e.Row, entity.EarningsAcctID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PRxPMTask.earningsAcctID>(e.Cache)));
			}

			var earningsSubMaskIsTask = PRSetupMaint.SubMaskContainsValue(SetupPreference.Cache, setup, typeof(PRSetup.earningsSubMask), setup.EarningsSubMask, GLAccountSubSource.Task);
			if (earningsSubMaskIsTask == true && entity.EarningsSubID == null)
			{
				e.Cache.RaiseExceptionHandling<PRxPMTask.earningsSubID>(e.Row, entity.EarningsSubID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PRxPMTask.earningsSubID>(e.Cache)));
			}

			if (setup.BenefitExpenseAcctDefault == PREarningsAcctSubDefault.MaskTask && entity.BenefitExpenseAcctID == null)
			{
				e.Cache.RaiseExceptionHandling<PRxPMTask.benefitExpenseAcctID>(e.Row, entity.BenefitExpenseAcctID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PRxPMTask.benefitExpenseAcctID>(e.Cache)));
			}

			var benefitExpenseSubMaskIsTask = PRSetupMaint.SubMaskContainsValue(SetupPreference.Cache, setup, typeof(PRSetup.benefitExpenseSubMask), setup.BenefitExpenseSubMask, GLAccountSubSource.Task);
			if (benefitExpenseSubMaskIsTask == true && entity.BenefitExpenseSubID == null)
			{
				e.Cache.RaiseExceptionHandling<PRxPMTask.benefitExpenseSubID>(e.Row, entity.BenefitExpenseSubID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PRxPMTask.benefitExpenseSubID>(e.Cache)));
			}

			if (setup.TaxExpenseAcctDefault == PREarningsAcctSubDefault.MaskTask && entity.TaxExpenseAcctID == null)
			{
				e.Cache.RaiseExceptionHandling<PRxPMTask.taxExpenseAcctID>(e.Row, entity.TaxExpenseAcctID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PRxPMTask.taxExpenseAcctID>(e.Cache)));
			}

			var taxExpenseSubMaskIsTask = PRSetupMaint.SubMaskContainsValue(SetupPreference.Cache, setup, typeof(PRSetup.taxExpenseSubMask), setup.TaxExpenseSubMask, GLAccountSubSource.Task);
			if (taxExpenseSubMaskIsTask == true && entity.TaxExpenseSubID == null)
			{
				e.Cache.RaiseExceptionHandling<PRxPMTask.taxExpenseSubID>(e.Row, entity.TaxExpenseSubID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PRxPMTask.taxExpenseSubID>(e.Cache)));
			}

			if (setup.PTOExpenseAcctDefault == PREarningsAcctSubDefault.MaskTask && entity.PTOExpenseAcctID == null)
			{
				e.Cache.RaiseExceptionHandling<PRxPMTask.ptoExpenseAcctID>(e.Row, entity.PTOExpenseAcctID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PRxPMTask.ptoExpenseAcctID>(e.Cache)));
			}

			var ptoExpenseSubMaskIsTask = PRSetupMaint.SubMaskContainsValue(SetupPreference.Cache, setup, typeof(PRSetup.ptoExpenseSubMask), setup.PTOExpenseSubMask, GLAccountSubSource.Task);
			if (ptoExpenseSubMaskIsTask == true && entity.PTOExpenseSubID == null)
			{
				e.Cache.RaiseExceptionHandling<PRxPMTask.ptoExpenseSubID>(e.Row, entity.PTOExpenseSubID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PRxPMTask.ptoExpenseSubID>(e.Cache)));
			}
		}

		#endregion

		public virtual void Copy(PRxPMTask dstEntity, PRxPMTask srcEntity)
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
	/// Extends the ProjectTaskEntry graph to adapt business logic / UI to work with PR fields
	/// </summary>
	public class PRxProjectTaskEntry : PXGraphExtension<ProjectTaskEntry>
	{
		public class PRxTaskGraph : PRxTaskGraph<ProjectTaskEntry, PMTask>
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
	}
}

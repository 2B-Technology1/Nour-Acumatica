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
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.CA
{
	public class CAAPAROpenPeriodAttribute : OpenPeriodAttribute
	{
		[PXHidden]
		public class OrigModulePh : BqlPlaceholderBase { }
		private readonly Type _origModuteField;
		public PXErrorLevel errorLevel { get; set; } = PXErrorLevel.Error;
		#region Ctor
		public CAAPAROpenPeriodAttribute(Type origModule,
			Type sourceType,
			Type branchSourceType,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			Type masterFinPeriodIDType = null,
		    bool redefaultOnDateChanged = true)
			: base(BqlTemplate.OfCommand<
					Search<FinPeriod.finPeriodID,
					Where<FinPeriod.status, Equal<FinPeriod.status.open>,
						And<Where2<
								Where<Current<OrigModulePh>, Equal<BatchModule.moduleCA>, And<FinPeriod.cAClosed, Equal<False>>>,
							Or2<
								Where<Current<OrigModulePh>, Equal<BatchModule.moduleAP>, And<FinPeriod.aPClosed, Equal<False>>>,
							Or2<
								Where<Current<OrigModulePh>, Equal<BatchModule.moduleAR>, And<FinPeriod.aRClosed, Equal<False>>>,
							Or<
								Current<OrigModulePh>, IsNull>>>>>>>>
				  .Replace<OrigModulePh>(origModule).ToType(),
					sourceType,
				branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType,
				useMasterCalendarSourceType: useMasterCalendarSourceType,
				defaultType: defaultType,
				redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
				masterFinPeriodIDType: masterFinPeriodIDType,
			    redefaultOnDateChanged: redefaultOnDateChanged)
		{
			_origModuteField = origModule;
		}
		#endregion
		#region Implementation
		public static void VerifyPeriod<Field>(PXCache cache, object row)
			where Field : IBqlField
		{
			foreach (CAAPAROpenPeriodAttribute attr in cache.GetAttributesReadonly<Field>(row).OfType<CAAPAROpenPeriodAttribute>())
			{
				attr.IsValidPeriod(cache, row, cache.GetValue<Field>(row));	
			}
		}

		protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult result = base.ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

			if (!result.HasWarningOrError)
			{
				var origModule = (string)sender.GetValue(row, _origModuteField.Name);

				if (origModule == BatchModule.CA && finPeriod.CAClosed == true ||
					origModule == BatchModule.AP && finPeriod.APClosed == true ||
					origModule == BatchModule.AR && finPeriod.ARClosed == true)
				{
					result = HandleErrorThatPeriodIsClosed(sender, finPeriod);
				}
			}

			return result;
		}
		#endregion
	}
}

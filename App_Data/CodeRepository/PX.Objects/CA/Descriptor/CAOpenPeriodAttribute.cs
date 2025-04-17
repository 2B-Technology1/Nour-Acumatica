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
using System.Reflection;
using PX.Data;
using PX.CCProcessingBase.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.Descriptor;

namespace PX.Objects.CA
{
	#region CAOpenPeriodAttribute
	/// <summary>
	/// Specialized for CA version of the <see cref="OpenPeriodAttribut"/><br/>
	/// Selector. Provides  a list  of the active Fin. Periods, having CAClosed flag = false <br/>
	/// <example>
	/// [CAOpenPeriod(typeof(CATran.tranDate))]
	/// </example>
	/// </summary>
	public class CAOpenPeriodAttribute : OpenPeriodAttribute
	{
		private Type cashAccountType { get; set; }

		#region Ctor

		/// <summary>
		/// Extended Ctor. 
		/// </summary>
		/// <param name="sourceType">Must be IBqlField. Refers a date, based on which "current" period will be defined</param>
		public CAOpenPeriodAttribute(Type sourceType, 
			Type branchSourceType,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			Type masterFinPeriodIDType = null)
			: base(typeof(Search<FinPeriod.finPeriodID,
								Where<FinPeriod.cAClosed, Equal<False>,
										And<FinPeriod.status, Equal<FinPeriod.status.open>>>>), 
					sourceType,
					branchSourceType: branchSourceType,
					branchSourceFormulaType: branchSourceFormulaType,
					organizationSourceType: organizationSourceType,
					useMasterCalendarSourceType: useMasterCalendarSourceType,
					defaultType: defaultType,
					redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
					masterFinPeriodIDType: masterFinPeriodIDType)
		{
			cashAccountType = branchSourceType;
		}

		public CAOpenPeriodAttribute()
			: this(null, null)
		{
		}
		#endregion

		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), cashAccountType.Name, CashAccountVerifying);
		}

		public virtual void CashAccountVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			object finPeriodValue = sender.GetValue(e.Row, _FieldName);
			if (finPeriodValue != null)
			{
				if (cashAccountType != null && e.NewValue == null)
				{
					throw new PXSetPropertyException(Messages.FinPeriodCanNotBeSpecifiedCashAccountIsEmpty);
				}
			}
		}

		protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult result = base.ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

			if (!result.HasWarningOrError && finPeriod.CAClosed == true)
			{
				result = HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: Messages.FinancialPeriodClosedInCA);
			}

			return result;
		}

		protected override void ValidateFinPeriodID(PXCache sender, object row, string finPeriodID)
		{
			if (cashAccountType != null && sender.GetValue(row, cashAccountType.Name) == null)
			{
				throw new PXSetPropertyException(Messages.FinPeriodCanNotBeSpecifiedCashAccountIsEmpty);
			}

			base.ValidateFinPeriodID(sender, row, finPeriodID);
		}
		#endregion
	}
	#endregion
}

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
using PX.Data;

namespace PX.Objects.CT
{
	public class ContractExpirationDate<ContractType, DurationType, StartDate, Duration> : BqlFormulaEvaluator<StartDate, ContractType, DurationType, StartDate, Duration>, IBqlOperand
		where ContractType : IBqlField
		where DurationType : IBqlField
		where StartDate : IBqlField
		where Duration : IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			DateTime? startDate = (DateTime?)parameters[typeof(StartDate)];
			string contractType = (string)parameters[typeof(ContractType)];
			string durationType = (string)parameters[typeof(DurationType)];
			int? duration = (int?)parameters[typeof(Duration)];

			if (contractType != Contract.type.Unlimited &&
				startDate != null && !string.IsNullOrEmpty(durationType) && duration != null)
			{
				DateTime origin = (DateTime)startDate;
				switch (durationType)
				{
					case Contract.durationType.Annual:
						return origin.AddYears((int) duration).AddDays(-1);
					case Contract.durationType.Monthly:
						return origin.AddMonths((int) duration).AddDays(-1);
					case Contract.durationType.Quarterly:
						return origin.AddMonths(3 * (int) duration).AddDays(-1);
					case Contract.durationType.Custom:
						return origin.AddDays((double) duration).AddDays(-1);
				}
			}
			return null;
		}
	}
}

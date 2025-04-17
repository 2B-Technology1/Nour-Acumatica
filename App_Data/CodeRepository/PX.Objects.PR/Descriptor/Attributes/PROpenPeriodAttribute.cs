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
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System;
using System.Linq;

namespace PX.Objects.PR
{
	/// <summary>
	/// Specialized for PR version of the <see cref="OpenPeriodAttribute"/><br/>
	/// Selector. Provides a list  of the active Fin. Periods, having PRClosed flag = false <br/>
	/// <example>
	/// [PROpenPeriod(typeof(PRPayment.paymentDate), typeof(PRPayment.organizationID))]
	/// </example>
	/// </summary>
	public class PROpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor

		/// <summary>
		/// Extended Ctor. 
		/// </summary>
		/// <param name="SourceType">Must be IBqlField. Refers a date, based on which "current" period will be defined</param>
		public PROpenPeriodAttribute(Type SourceType, Type organizationSourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.pRClosed, Equal<False>, And<FinPeriod.status, Equal<FinPeriod.status.open>>>>), SourceType, organizationSourceType: organizationSourceType)
		{
		}
		#endregion

		#region Implementation

		public static void DefaultFirstOpenPeriod(PXCache sender, string FieldName)
		{
			foreach (PeriodIDAttribute attr in sender.GetAttributesReadonly(FieldName).OfType<PeriodIDAttribute>())
			{
				attr.SearchType = typeof(Search2<FinPeriod.finPeriodID, CrossJoin<GLSetup>, Where<FinPeriod.endDate, Greater<Required<FinPeriod.endDate>>, And<FinPeriod.active, Equal<True>, And<Where<GLSetup.restrictAccessToClosedPeriods, NotEqual<True>, Or<FinPeriod.pRClosed, Equal<False>>>>>>, OrderBy<Asc<FinPeriod.finPeriodID>>>);
			}
		}

		public static void DefaultFirstOpenPeriod<Field>(PXCache sender)
			where Field : IBqlField
		{
			DefaultFirstOpenPeriod(sender, typeof(Field).Name);
		}

		#endregion

	}
}

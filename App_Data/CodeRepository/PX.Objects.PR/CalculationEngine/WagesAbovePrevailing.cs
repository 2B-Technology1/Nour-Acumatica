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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class WagesAbovePrevailing
	{
		private decimal _WagesAtPrevailing = 0;
		private decimal _ActualWages = 0;
		private decimal _WageBaseHours = 0;
		
		public decimal ExcessWageAmount => _ActualWages - _WagesAtPrevailing;
		public decimal EffectivePrevailingRate => _WageBaseHours != 0 ? _WagesAtPrevailing / _WageBaseHours : 0m;
		public decimal EffectivePayRate => _WageBaseHours != 0 ? _ActualWages / _WageBaseHours : 0m;

		public void Add(decimal? prevailingRate, decimal? actualWages, decimal? hours)
		{
			_WagesAtPrevailing += prevailingRate.GetValueOrDefault() * hours.GetValueOrDefault();
			_ActualWages += actualWages.GetValueOrDefault();
			_WageBaseHours += hours.GetValueOrDefault();
		}

	}
}

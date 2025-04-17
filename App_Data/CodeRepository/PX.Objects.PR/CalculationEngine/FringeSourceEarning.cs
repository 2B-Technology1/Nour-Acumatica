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
using PX.Objects.PM;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		public struct FringeSourceEarning
		{
			public FringeSourceEarning(PREarningDetail earning, PMProject project, decimal calculatedFringeRate, decimal setupFringeRate, decimal overtimeMultiplier)
			{
				this.Earning = earning;
				this.Project = project;
				this.CalculatedFringeRate = calculatedFringeRate;
				this.SetupFringeRate = setupFringeRate;
				this.OvertimeMultiplier = overtimeMultiplier;
			}

			public PREarningDetail Earning;
			public PMProject Project;
			public decimal CalculatedFringeRate;
			public decimal SetupFringeRate;
			public decimal OvertimeMultiplier;

			public decimal CalculatedFringeAmount => Math.Round(CalculatedFringeRate * Earning.Hours.GetValueOrDefault(), 2, MidpointRounding.AwayFromZero);

		}
	}
}

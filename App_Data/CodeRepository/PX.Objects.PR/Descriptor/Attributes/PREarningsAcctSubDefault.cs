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
using System;
using System.Linq;

namespace PX.Objects.PR
{
	public class PREarningsAcctSubDefault
	{
		public class AcctListAttribute : CustomListAttribute
		{
			private static Tuple<string, string>[] Pairs => new[]
			{
				Pair(MaskEarningType, PR.Messages.EarningType),
				Pair(MaskEmployee,  PR.Messages.PREmployee),
				Pair(MaskPayGroup,  PR.Messages.PRPayGroup),
				Pair(MaskLaborItem,  PR.Messages.LaborItem),
				Pair(MaskProject,  PR.Messages.Project),
				Pair(MaskTask,  PR.Messages.ProjectTask),
			};

			public AcctListAttribute() : base(Pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class AlternateListAttribute : CustomListAttribute
		{
			private static Tuple<string, string>[] Pairs => new[]
			{
				Pair(MaskEarningType, PR.Messages.EarningType),
				Pair(MaskEmployee,  PR.Messages.PREmployee),
				Pair(MaskPayGroup,  PR.Messages.PRPayGroup),
			};

			public AlternateListAttribute() : base(Pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class SubListAttribute : CustomListAttribute
		{
			private static Tuple<string, string>[] Pairs => new[]
			{
				Pair(MaskBranch,    PR.Messages.Branch),
				Pair(MaskEmployee,  PR.Messages.PREmployee),
				Pair(MaskPayGroup,  PR.Messages.PRPayGroup),
				Pair(MaskEarningType, PR.Messages.EarningType),
				Pair(MaskLaborItem,  PR.Messages.LaborItem),
				Pair(MaskProject,  PR.Messages.Project),
				Pair(MaskTask,  PR.Messages.ProjectTask),
			};

			public SubListAttribute() : base(Pairs) { }
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class AlternateSubListAttribute : CustomListAttribute
		{
			private static Tuple<string, string>[] Pairs => new[]
			{
				Pair(MaskBranch,    PR.Messages.Branch),
				Pair(MaskEmployee,  PR.Messages.PREmployee),
				Pair(MaskPayGroup,  PR.Messages.PRPayGroup),
				Pair(MaskEarningType, PR.Messages.EarningType),
			};

			public AlternateSubListAttribute() : base(Pairs) { }
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskBranch = GLAccountSubSource.Branch;
		public const string MaskEmployee = GLAccountSubSource.Employee;
		public const string MaskPayGroup = GLAccountSubSource.PayGroup;
		public const string MaskEarningType = GLAccountSubSource.EarningType;
		public const string MaskLaborItem = GLAccountSubSource.LaborItem;
		public const string MaskProject = GLAccountSubSource.Project;
		public const string MaskTask = GLAccountSubSource.Task;
	}

	public abstract class CustomListAttribute : PXStringListAttribute
	{
		public string[] AllowedValues => _AllowedValues;
		public string[] AllowedLabels => _AllowedLabels;

		protected CustomListAttribute(Tuple<string, string>[] valuesToLabels) : base(valuesToLabels) { }

		protected abstract Tuple<string, string>[] GetPairs();

		public override void CacheAttached(PXCache sender)
		{
			var pairs = GetPairs();
			_AllowedValues = pairs.Select(t => t.Item1).ToArray();
			_AllowedLabels = pairs.Select(t => t.Item2).ToArray();
			_NeutralAllowedLabels = _AllowedLabels;

			base.CacheAttached(sender);
		}
	}
}

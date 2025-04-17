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

namespace PX.Objects.PR
{
	public class PRPTOAcctSubDefault
	{
		public class AcctListAttribute : CustomListAttribute
		{
			protected static Tuple<string, string>[] Pairs => new []
			{
				Pair(MaskPTOBank, Messages.PTOBank),
				Pair(MaskEmployee, Messages.PREmployee),
				Pair(MaskPayGroup, Messages.PRPayGroup),
			};

			public AcctListAttribute() : base(Pairs) { }
			protected AcctListAttribute(Tuple<string, string>[] pairs) : base(pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class SubListAttribute : CustomListAttribute
		{
			protected static Tuple<string, string>[] Pairs => new []
			{
				Pair(MaskEmployee, Messages.PREmployee),
				Pair(MaskPayGroup, Messages.PRPayGroup),
				Pair(MaskPTOBank, Messages.PTOBank),
			};

			public SubListAttribute() : base(Pairs) { }
			protected SubListAttribute(Tuple<string, string>[] pairs) : base(pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskEmployee = GLAccountSubSource.Employee;
		public const string MaskPayGroup = GLAccountSubSource.PayGroup;
		public const string MaskPTOBank = GLAccountSubSource.PTOBank;
	}

	public class PRPTOExpenseAcctSubDefault : PRPTOAcctSubDefault
	{
		public new class AcctListAttribute : PRPTOAcctSubDefault.AcctListAttribute
		{
			protected static new Tuple<string, string>[] Pairs => PRPTOAcctSubDefault.AcctListAttribute.Pairs.Union(new[]
			{
				Pair(MaskEarningType, Messages.EarningType),
				Pair(MaskLaborItem, Messages.LaborItem),
				Pair(MaskProject, Messages.Project),
				Pair(MaskTask, Messages.ProjectTask),
			}).ToArray();

			public AcctListAttribute() : base(Pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public new class AlternateListAttribute : PRPTOAcctSubDefault.AcctListAttribute
		{
			protected static new Tuple<string, string>[] Pairs => PRPTOAcctSubDefault.AcctListAttribute.Pairs.Union(new[]
			{
				Pair(MaskEarningType, Messages.EarningType),
			}).ToArray();

			public AlternateListAttribute() : base(Pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public new class SubListAttribute : PRPTOAcctSubDefault.SubListAttribute
		{
			protected static new Tuple<string, string>[] Pairs => PRPTOAcctSubDefault.SubListAttribute.Pairs.Union(new[]
			{
				Pair(MaskEarningType, Messages.EarningType),
				Pair(MaskLaborItem, Messages.LaborItem),
				Pair(MaskProject, Messages.Project),
				Pair(MaskTask, Messages.ProjectTask),
			}).ToArray();

			public SubListAttribute() : base(Pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public new class AlternateSubListAttribute : PRPTOAcctSubDefault.SubListAttribute
		{
			protected static new Tuple<string, string>[] Pairs => PRPTOAcctSubDefault.SubListAttribute.Pairs.Union(new[]
			{
				Pair(MaskEarningType, Messages.EarningType),
			}).ToArray();

			public AlternateSubListAttribute() : base(Pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskEarningType = GLAccountSubSource.EarningType;
		public const string MaskLaborItem = GLAccountSubSource.LaborItem;
		public const string MaskProject = GLAccountSubSource.Project;
		public const string MaskTask = GLAccountSubSource.Task;

		public class maskEarningType : PX.Data.BQL.BqlString.Constant<maskEarningType>
		{
			public maskEarningType() : base(MaskEarningType) { }
		}

		public class maskLaborItem : PX.Data.BQL.BqlString.Constant<maskLaborItem>
		{
			public maskLaborItem() : base(MaskLaborItem) { }
		}

		public class maskProject : PX.Data.BQL.BqlString.Constant<maskProject>
		{
			public maskProject() : base(MaskProject) { }
		}

		public class maskTask : PX.Data.BQL.BqlString.Constant<maskTask>
		{
			public maskTask() : base(MaskTask) { }
		}
	}
}

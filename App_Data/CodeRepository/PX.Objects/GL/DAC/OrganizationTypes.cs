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
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;

namespace PX.Objects.GL.DAC
{
	public class OrganizationTypes
	{

		public const string WithoutBranches = "WithoutBranches";
		public const string WithBranchesNotBalancing = "NotBalancing";
		public const string WithBranchesBalancing = "Balancing";
		public const string Group = "Group";

		public class withoutBranches : PX.Data.BQL.BqlString.Constant<withoutBranches>
		{
			public withoutBranches() : base(WithoutBranches) {; }
		}

		public class withBranchesNotBalancing : PX.Data.BQL.BqlString.Constant<withBranchesNotBalancing>
		{
			public withBranchesNotBalancing() : base(WithBranchesNotBalancing) {; }
		}

		public class withBranchesBalancing : PX.Data.BQL.BqlString.Constant<withBranchesBalancing>
		{
			public withBranchesBalancing() : base(WithBranchesBalancing) {; }
		}
		public class group : PX.Data.BQL.BqlString.Constant<group>
		{
			public group() : base(Group) {; }
		}

		public class ListAttribute : PXStringListAttribute
		{
			protected string[] ExcludedTypes { get; set; }

			public ListAttribute() : base() { }

			public ListAttribute(params string[] excludedTypes) : base() 
			{
				ExcludedTypes = excludedTypes;
			}

			public override void CacheAttached(PXCache sender)
			{
				List<string> orgTypesValues = new List<string>();
				List<string> orgTypesLabels = new List<string>();

				orgTypesValues.Add(WithoutBranches);
				orgTypesLabels.Add(Messages.WithoutBranches);
				

				if (PXAccess.FeatureInstalled<FeaturesSet.branch>())
				{
					orgTypesValues.Add(WithBranchesNotBalancing);
					orgTypesLabels.Add(Messages.WithBranchesNotRequiringBalancing);

					if (PXAccess.FeatureInstalled<FeaturesSet.interBranch>())
					{
						orgTypesValues.Add(WithBranchesBalancing);
						orgTypesLabels.Add(Messages.WithBranchesRequiringBalancing);
					}
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>())
				{
					orgTypesValues.Add(Group);
					orgTypesLabels.Add(Messages.Group);
				}

				if (ExcludedTypes != null)
				{
					foreach(var toRemove in ExcludedTypes)
					{
						if (orgTypesValues.Contains(toRemove))
						{
							var index = orgTypesValues.IndexOf(toRemove);
							orgTypesValues.RemoveAt(index);
							orgTypesLabels.RemoveAt(index);
						}
					}
				}

				_AllowedValues = orgTypesValues.ToArray();
				_AllowedLabels = orgTypesLabels.ToArray();
				_NeutralAllowedLabels = null;

				base.CacheAttached(sender);
			}
		}
	}
}

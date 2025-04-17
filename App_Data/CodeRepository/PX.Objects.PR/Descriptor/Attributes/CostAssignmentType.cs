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
using System.Collections.Generic;
using System.Text;

namespace PX.Objects.PR
{
	public class CostAssignmentType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
			{
				char[] availableDigits = { NoAssignment, ProjectAssignment, LaborItemAssignment, EarningTypeAssignment, ProjectAndLaborItemAssignment, ProjectAndEarningTypeAssignment };
				List<string> valueList = new List<string>();
				foreach (char i in availableDigits)
				{
					foreach (char j in availableDigits)
					{
						foreach (char k in availableDigits)
						{
							valueList.Add(new string(new char[] { i, j, k }));
						}
					}
				}

				_AllowedValues = valueList.ToArray();
			}
		}

		public static string GetLaborCostSplitCode(CostAssignmentSetting earningSetting, CostAssignmentSetting benefitSetting, CostAssignmentSetting taxSetting)
		{
			StringBuilder sb = new StringBuilder();
			foreach (CostAssignmentSetting setting in new CostAssignmentSetting[] { earningSetting, benefitSetting, taxSetting })
			{
				sb.Append(GetSingleSplitCode(setting));
			}

			return sb.ToString();
		}

		public static char GetSingleSplitCode(CostAssignmentSetting setting)
		{
			if (setting.AssignCostToProject)
			{
				if (setting.AssignCostToLaborItem)
				{
					return ProjectAndLaborItemAssignment;
				}
				else if (setting.AssignCostToEarningType)
				{
					return ProjectAndEarningTypeAssignment;
				}
				else
				{
					return ProjectAssignment;
				}
			}
			else if (setting.AssignCostToLaborItem)
			{
				return LaborItemAssignment;
			}
			else if (setting.AssignCostToEarningType)
			{
				return LaborItemAssignment;
			}
			else
			{
				return NoAssignment;
			}
		}

		public static CostAssignmentSetting GetEarningSetting(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				return new CostAssignmentSetting(false, false, false, false);
			}

			return GetSetting(code[0]);
		}

		public static CostAssignmentSetting GetBenefitSetting(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				return new CostAssignmentSetting(false, false, false, false);
			}

			return GetSetting(code[1]);
		}

		public static CostAssignmentSetting GetTaxSetting(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				return new CostAssignmentSetting(false, false, false, false);
			}

			return GetSetting(code[2]);
		}

		public static CostAssignmentSetting GetSetting(char? code)
		{
			if (code == null || code == default(char))
			{
				return new CostAssignmentSetting(false, false, false, false);
			}

			bool assignCostToProject = code == ProjectAssignment || code == ProjectAndLaborItemAssignment || code == ProjectAndEarningTypeAssignment;
			bool assignCostToLaborItem = code == LaborItemAssignment || code == ProjectAndLaborItemAssignment;
			bool assignCostToEarningType = code == EarningTypeAssignment || code == ProjectAndEarningTypeAssignment;
			return new CostAssignmentSetting(!assignCostToProject, assignCostToProject, assignCostToLaborItem, assignCostToEarningType);
		}

		public static bool IsProjectSplitEnabled(PXGraph graph, PRPayment currentPayment, DetailType detailType)
		{
			if (detailType == DetailType.Benefit)
			{
				return CostAssignmentColumnVisibilityEvaluator.BenefitProject.Evaluate(graph, currentPayment);
			}
			else
			{
				return CostAssignmentColumnVisibilityEvaluator.TaxProject.Evaluate(graph, currentPayment);
			}
		}

		public static bool IsEarningTypeSplitEnabled(PXGraph graph, PRPayment currentPayment, DetailType detailType)
		{
			if (detailType == DetailType.Benefit)
			{
				return CostAssignmentColumnVisibilityEvaluator.BenefitEarningType.Evaluate(graph, currentPayment);
			}
			else
			{
				return CostAssignmentColumnVisibilityEvaluator.TaxEarningType.Evaluate(graph, currentPayment);
			}
		}

		public static bool IsLaborItemSplitEnabled(PXGraph graph, PRPayment currentPayment, DetailType detailType)
		{
			if (detailType == DetailType.Benefit)
			{
				return CostAssignmentColumnVisibilityEvaluator.BenefitLaborItem.Evaluate(graph, currentPayment);
			}
			else
			{
				return CostAssignmentColumnVisibilityEvaluator.TaxLaborItem.Evaluate(graph, currentPayment);
			}
		}

		private const char NoAssignment = 'N';
		private const char ProjectAssignment = 'P';
		private const char LaborItemAssignment = 'L';
		private const char EarningTypeAssignment = 'T';
		private const char ProjectAndLaborItemAssignment = 'X';
		private const char ProjectAndEarningTypeAssignment = 'Y';

		public enum DetailType
		{
			Benefit,
			Tax,
			PTO
		}
	}

	public class CostAssignmentSetting
	{
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R1)]
		public CostAssignmentSetting(bool assignCostToProject, bool assignCostToLaborItem, bool assignCostToEarningType)
		{
			AssignCostToProject = assignCostToProject;
			AssignCostToLaborItem = assignCostToLaborItem;
			AssignCostToEarningType = assignCostToEarningType;
		}

		public CostAssignmentSetting(bool noCostAssigned, bool assignCostToProject, bool assignCostToLaborItem, bool assignCostToEarningType)
		{
			NoCostAssigned = noCostAssigned;
			AssignCostToProject = assignCostToProject;
			AssignCostToLaborItem = assignCostToLaborItem;
			AssignCostToEarningType = assignCostToEarningType;
		}

		public bool NoCostAssigned { get; set; }
		public bool AssignCostToProject { get; set; } = false;
		public bool AssignCostToLaborItem
		{
			get => _AssignCostToLaborItem;
			set
			{
				if (value && AssignCostToEarningType && !NoCostAssigned)
				{
					throw new PXException(Messages.CantAssignCostToLaborItemAndEarningType);
				}
				_AssignCostToLaborItem = value;
			}
		}
		public bool AssignCostToEarningType
		{
			get => _AssignCostToEarningType;
			set
			{
				if (value && AssignCostToLaborItem && !NoCostAssigned)
				{
					throw new PXException(Messages.CantAssignCostToLaborItemAndEarningType);
				}
				_AssignCostToEarningType = value;
			}
		}

		private bool _AssignCostToLaborItem = false;
		private bool _AssignCostToEarningType = false;
	}
}

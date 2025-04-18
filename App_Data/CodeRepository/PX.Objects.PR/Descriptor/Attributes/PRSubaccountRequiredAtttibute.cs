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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public abstract class PRSubaccountRequiredAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber, IPXRowPersistingSubscriber
	{
		protected List<string> _SetupFieldNameList = new List<string>();
		protected string _ValueWhenRequired;
		protected Type _RequiredCondition;

		public PRSubaccountRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
		{
			_ValueWhenRequired = valueWhenRequired;
			_RequiredCondition = requiredCondition;
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetRequired(sender, this.FieldName, IsRequired(sender, e.Row));
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object newValue = sender.GetValue(e.Row, this.FieldName);
			object oldValue = sender.GetValueOriginal(e.Row, this.FieldName);
			if (e.Operation != PXDBOperation.Delete && newValue == null && oldValue != null && IsRequired(sender, e.Row))
			{
				PXUIFieldAttribute.SetError(sender, e.Row, this.FieldName, PXMessages.LocalizeFormatNoPrefix(Messages.CantBeEmpty, PXUIFieldAttribute.GetDisplayName(sender, this.FieldName)));
			}
		}

		public virtual bool IsRequired(PXCache sender, object row)
		{
			PXCache prSetupCache = sender.Graph.Caches[typeof(PRSetup)];
			PRSetup setupRecord = SelectFrom<PRSetup>.View.Select(sender.Graph).TopFirst;
			if (setupRecord == null)
			{
				return false;
			}

			if (row != null && _RequiredCondition != null && !ConditionEvaluator.GetResult(sender, row, _RequiredCondition))
			{
				return false;
			}

			foreach (var fieldName in _SetupFieldNameList)
			{
				PRSubAccountMaskAttribute maskAttribute = prSetupCache.GetAttributesOfType<PRSubAccountMaskAttribute>(setupRecord, fieldName).SingleOrDefault();
				if (maskAttribute != null)
				{
					var dimensionAttribute = maskAttribute.GetAttribute<PRDimensionMaskAttribute>();
					if (dimensionAttribute != null)
					{
						string mask = prSetupCache.GetValue(setupRecord, fieldName) as string;
						if(string.IsNullOrEmpty(mask))
						{
							continue;
						}

						IEnumerable<string> segments = dimensionAttribute.GetSegmentMaskValues(mask); 
						if (segments.Any(x => x == _ValueWhenRequired))
						{
							return true;
						}
					}
				}
			}

			return false;
		}
	}

	public class PREarningSubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PREarningSubRequiredAttribute(string valueWhenRequired)
			: base(valueWhenRequired)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.earningsSubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.earningsAlternateSubMask).Name);
		}
	}

	public class PRDedLiabilitySubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRDedLiabilitySubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.deductLiabilitySubMask).Name);
		}
	}

	public class PRBenExpenseSubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRBenExpenseSubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.benefitExpenseSubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.benefitExpenseAlternateSubMask).Name);
		}
	}

	public class PRBenLiabilitySubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRBenLiabilitySubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.benefitLiabilitySubMask).Name);
		}
	}

	public class PRTaxExpenseSubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRTaxExpenseSubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.taxExpenseSubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.taxExpenseAlternateSubMask).Name);
		}
	}

	public class PRTaxLiabilitySubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRTaxLiabilitySubRequiredAttribute(string valueWhenRequired)
			: base(valueWhenRequired)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.taxLiabilitySubMask).Name);
		}
	}

	public class PRPTOExpenseSubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRPTOExpenseSubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.ptoExpenseSubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.ptoExpenseAlternateSubMask).Name);
		}
	}

	public class PRPTOLiabilitySubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRPTOLiabilitySubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.ptoLiabilitySubMask).Name);
		}
	}

	public class PRPTOAssetSubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRPTOAssetSubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.ptoAssetSubMask).Name);
		}
	}

	public class PRBranchSubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRBranchSubRequiredAttribute(string valueWhenRequired)
			: base(valueWhenRequired)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.earningsSubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.deductLiabilitySubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.benefitExpenseSubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.benefitLiabilitySubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.taxExpenseSubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.taxLiabilitySubMask).Name);
		}
	}
}

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

namespace PX.Objects.PM
{
	public class CostCodeManager : ICostCodeManager
	{			
		private class DefaultCostCodeDefinition : IPrefetchable
		{
			public const string SLOT_KEY = "DefaultCostCodeDefinition";

			public static Type[] DependentTables
			{
				get
				{
					return new[] { typeof(PMCostCode) };
				}
			}

			
			public int? DefaultCostCodeID { get; private set; }

			public DefaultCostCodeDefinition()
			{				
			}

			public void Prefetch()
			{
				foreach (PXDataRecord record in PXDatabase.SelectMulti<PMCostCode>(
					new PXDataField<PMCostCode.costCodeID>(),
					new PXDataFieldValue<PMCostCode.isDefault>(true, PXComp.EQ)))
				{
					DefaultCostCodeID = record.GetInt32(0).GetValueOrDefault();
				}
			}
		}
				
		private DefaultCostCodeDefinition DefaultDefinition
		{
			get
			{
				return PXDatabase.GetSlot<DefaultCostCodeDefinition>(DefaultCostCodeDefinition.SLOT_KEY, DefaultCostCodeDefinition.DependentTables);
			}
		}

		public int? DefaultCostCodeID
		{
			get
			{
				return DefaultDefinition.DefaultCostCodeID;
			}
		}
	}

	public interface ICostCodeManager
	{
		int? DefaultCostCodeID { get;  }
	}
}

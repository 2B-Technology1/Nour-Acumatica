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
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class ProjectSettingsManager : IProjectSettingsManager
	{		
		private class DefaultProjectSettingsDefinition : IPrefetchable
		{
			public const string SLOT_KEY = "DefaultProjectSettingsDefinition";

			public static Type[] DependentTables
			{
				get
				{
					return new[] { typeof(PMSetup) };
				}
			}

			
			public int NonProjectID { get; private set; }

			public int EmptyInventoryID { get; private set; }

			public string CostBudgetUpdateMode { get; private set; }

            public string RevenueBudgetUpdateMode { get; private set; }

			public bool CalculateProjectSpecificTaxes { get; private set; }

			public bool CostCommitmentTracking { get; private set; }

			public HashSet<string> VisibleModules { get; private set; }
			
			public DefaultProjectSettingsDefinition()
			{
				VisibleModules = new HashSet<string>();
			}

			public void Prefetch()
			{
				string emptyInventoryCD = PMInventorySelectorAttribute.EmptyComponentCD;
				CostBudgetUpdateMode = CostBudgetUpdateModes.Detailed;
                RevenueBudgetUpdateMode = CostBudgetUpdateModes.Summary;

                foreach (PXDataRecord record in PXDatabase.SelectMulti<PMSetup>(
					new PXDataField<PMSetup.visibleInAP>(),
					new PXDataField<PMSetup.visibleInAR>(),
					new PXDataField<PMSetup.visibleInCA>(),
					new PXDataField<PMSetup.visibleInCR>(),
					new PXDataField<PMSetup.visibleInEA>(),
					new PXDataField<PMSetup.visibleInGL>(),
					new PXDataField<PMSetup.visibleInIN>(),
					new PXDataField<PMSetup.visibleInPO>(),
					new PXDataField<PMSetup.visibleInSO>(),
					new PXDataField<PMSetup.visibleInTA>(),
					new PXDataField<PMSetup.emptyItemCode>(),
					new PXDataField<PMSetup.costBudgetUpdateMode>(),
                    new PXDataField<PMSetup.revenueBudgetUpdateMode>(),
					new PXDataField<PMSetup.calculateProjectSpecificTaxes>(),
					new PXDataField<PMSetup.costCommitmentTracking>()))
				{
					if (record.GetBoolean(0).GetValueOrDefault() == true) VisibleModules.Add(GL.BatchModule.AP);
					if (record.GetBoolean(1).GetValueOrDefault() == true) VisibleModules.Add(GL.BatchModule.AR);
					if (record.GetBoolean(2).GetValueOrDefault() == true) VisibleModules.Add(GL.BatchModule.CA);
					if (record.GetBoolean(3).GetValueOrDefault() == true) VisibleModules.Add(GL.BatchModule.CR);
					if (record.GetBoolean(4).GetValueOrDefault() == true) VisibleModules.Add(GL.BatchModule.EA);
					if (record.GetBoolean(5).GetValueOrDefault() == true) VisibleModules.Add(GL.BatchModule.GL);
					if (record.GetBoolean(6).GetValueOrDefault() == true) VisibleModules.Add(GL.BatchModule.IN);
					if (record.GetBoolean(7).GetValueOrDefault() == true) VisibleModules.Add(GL.BatchModule.PO);
					if (record.GetBoolean(8).GetValueOrDefault() == true) VisibleModules.Add(GL.BatchModule.SO);
					if (record.GetBoolean(9).GetValueOrDefault() == true) VisibleModules.Add(GL.BatchModule.TA);

					emptyInventoryCD = record.GetString(10);
					CostBudgetUpdateMode = record.GetString(11);
                    RevenueBudgetUpdateMode = record.GetString(12);
					CalculateProjectSpecificTaxes = record.GetBoolean(13) == true;
					CostCommitmentTracking = record.GetBoolean(14) == true;
				}

				foreach (PXDataRecord record in PXDatabase.SelectMulti<CT.Contract>(
					new PXDataField<CT.Contract.contractID>(),
					new PXDataFieldValue<CT.Contract.nonProject>(true, PXComp.EQ)))
				{
					NonProjectID = record.GetInt32(0).GetValueOrDefault();
				}

				foreach (PXDataRecord record in PXDatabase.SelectMulti<IN.InventoryItem>(
					new PXDataField<IN.InventoryItem.inventoryID>(),
					new PXDataFieldValue<IN.InventoryItem.inventoryCD>(emptyInventoryCD, PXComp.EQ)))
				{
					EmptyInventoryID = record.GetInt32(0).GetValueOrDefault();
				}
			}
		}
				
		private DefaultProjectSettingsDefinition DefaultDefinition
		{
			get
			{
				return PXDatabase.GetSlot<DefaultProjectSettingsDefinition>(DefaultProjectSettingsDefinition.SLOT_KEY, DefaultProjectSettingsDefinition.DependentTables);
			}
		}

		public int NonProjectID
		{
			get
			{
				return DefaultDefinition.NonProjectID;
			}
		}

		public int EmptyInventoryID
		{
			get
			{
				return DefaultDefinition.EmptyInventoryID;
			}
		}

		public string CostBudgetUpdateMode
		{
			get
			{
				return DefaultDefinition.CostBudgetUpdateMode;
			}
		}

        public string RevenueBudgetUpdateMode
        {
            get
            {
                return DefaultDefinition.RevenueBudgetUpdateMode;
            }
        }

		public bool CalculateProjectSpecificTaxes
		{
			get
			{
				return DefaultDefinition.CalculateProjectSpecificTaxes;
			}
		}
		public bool CostCommitmentTracking
		{
			get
			{
				return DefaultDefinition.CostCommitmentTracking;
			}
		}
		
		public bool IsPMVisible(string module)
		{
			return DefaultDefinition.VisibleModules.Contains(module);
		}
	}

	public interface IProjectSettingsManager
	{
		int NonProjectID { get;  }

		int EmptyInventoryID { get; }

		string CostBudgetUpdateMode { get;  }

		string RevenueBudgetUpdateMode { get; }

		bool CalculateProjectSpecificTaxes { get; }

		bool CostCommitmentTracking { get; }

		bool IsPMVisible(string module);
	}
}

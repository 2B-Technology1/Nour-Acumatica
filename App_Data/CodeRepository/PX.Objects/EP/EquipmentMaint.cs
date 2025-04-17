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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	public class EquipmentMaint : PXGraph<EquipmentMaint, EPEquipment>
	{
		#region DAC Attributes Override

		[PXDBIdentity]
		protected virtual void _(Events.CacheAttached<EPEquipment.equipmentID> _) {}

		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Equipment ID")]
		[PXSelector(typeof(Search<EPEquipment.equipmentCD>),
			typeof(EPEquipment.equipmentCD), typeof(EPEquipment.description), typeof(EPEquipment.status))]
		protected virtual void _(Events.CacheAttached<EPEquipment.equipmentCD> _) {}

		#endregion

		#region Views/Selects

		public PXSelect<EPEquipment> Equipment;
		public PXSelect<EPEquipment, Where<EPEquipment.equipmentID, Equal<Current<EPEquipment.equipmentID>>>> EquipmentProperties;
		public PXSelectJoin<EPEquipmentRate, InnerJoin<PMProject, On<PMProject.contractID, Equal<EPEquipmentRate.projectID>>>, Where<EPEquipmentRate.equipmentID, Equal<Current<EPEquipment.equipmentID>>>> Rates;

		[PXViewName(Messages.EquipmentAnswers)]
		public CRAttributeList<EPEquipment> Answers;

		#endregion

		protected virtual void _(Events.RowSelected<EPEquipment> e)
		{
			Rates.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}
	}
}

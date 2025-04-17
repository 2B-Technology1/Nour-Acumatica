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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	public class PRxRegisterEntry : PXGraphExtension<RegisterEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
#pragma warning disable CS0618 // Type or member is obsolete
		[PXRemoveBaseAttribute(typeof(LocationActiveAttribute))]
#pragma warning restore CS0618 // Type or member is obsolete
		[HybridLocationID(typeof(Where<Location.bAccountID, Equal<Current<PMTran.bAccountID>>>), typeof(PMTran.origModule), typeof(PRxPMTran.payrollWorkLocationID), DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		protected virtual void _(Events.CacheAttached<PMTran.locationID> e) { }
	}

#pragma warning disable CS0618 // Type or member is obsolete
	public class HybridLocationIDAttribute : LocationActiveAttribute, IPXFieldSelectingSubscriber
#pragma warning restore CS0618 // Type or member is obsolete
	{
		Type _ModuleField;
		Type _PayrollWorkLocationIDField;

		public HybridLocationIDAttribute(Type whereType, Type moduleField, Type payrollWorkLocationIDField)
			: base(whereType)
		{
			_ModuleField = moduleField;
			_PayrollWorkLocationIDField = payrollWorkLocationIDField;
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object module = sender.GetValue(e.Row, _ModuleField.Name);

			if (module?.Equals(BatchModule.PR) == true && sender.GetValue(e.Row, _PayrollWorkLocationIDField.Name) is int payrollWorkLocationID)
			{
				PRLocation location = new SelectFrom<PRLocation>.Where<PRLocation.locationID.IsEqual<P.AsInt>>.View(sender.Graph).SelectSingle(payrollWorkLocationID);
				if (location != null)
				{
					e.ReturnValue = location.LocationCD;
				}
			}
		}
	}
}

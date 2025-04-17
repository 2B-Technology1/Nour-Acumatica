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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using PX.Data;
using PX.Objects.CT;

namespace PX.Objects.Common.MigrationMode
{
	/// <summary>
	/// Attribute that sets <c>true</c> on the underlying field defaulting 
	/// if migration mode is activated in the specified setup field, and
	/// <c>false</c> otherwise.
	/// </summary>
	[PXDBBool]
	[PXDefault]
	public class MigratedRecordAttribute : PXAggregateAttribute, IPXFieldDefaultingSubscriber
	{
		public virtual Type MigrationModeSetupField
		{
			get;
			private set;
		}
		

		
		public MigratedRecordAttribute(Type migrationModeSetupField)
			: base()
		{
			MigrationModeSetupField = migrationModeSetupField;
		}
		
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Type Tsetup = BqlCommand.Compose(typeof(PXSetup<>), MigrationModeSetupField.DeclaringType);
			MethodInfo select = Tsetup.GetMethod("Select", BindingFlags.Public | BindingFlags.Static);
			var source = (IList)select.Invoke(null, new object[] { sender.Graph, new object[] { } });
			bool? _isMigrationModeEnabled = 
				source != null && 
				source.Count > 0 && 
				(sender.Graph.Caches[MigrationModeSetupField.DeclaringType].GetValue(
				PXResult.Unwrap(source[0], MigrationModeSetupField.DeclaringType)
					, MigrationModeSetupField.Name) as bool?) == true;
			e.NewValue = _isMigrationModeEnabled;
		}
	}
}

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
using PX.Data;
using PX.Objects.Common.GraphExtensions;

namespace PX.Objects.AM.GraphExtensions
{
	public class BomOperCurySettingsBOMMaint : BomOperCurySettingsBomExtensionBase<BOMMaint> { public static bool IsActive() => true; }
	public class BomOperCurySettingsEngineeringWorkbenchMaint : BomOperCurySettingsBomExtensionBase<EngineeringWorkbenchMaint> { public static bool IsActive() => true; }

	/// <summary>
	/// Shared changes for extensions which use BOMBaseGraph
	/// </summary>
	public abstract class BomOperCurySettingsBomExtensionBase<TGraph> : BomOperCurySettingsBase<TGraph>
		where TGraph : BOMBaseGraph<TGraph>, new()
	{
		[PXOverride]
		public virtual void EnableOperCache(bool enabled)
		{
			var view = Base.Views[this.ViewName];
			if(view != null)
			{
				view.AllowInsert = enabled;
				view.AllowUpdate = enabled;
				view.AllowDelete = enabled;
			}
		}
	}

	public abstract class BomOperCurySettingsBase<TGraph> : CurySettingsExtension<TGraph, AMBomOper, AMBomOperCury>
		where TGraph : PXGraph
	{
		protected override List<Type> ComposeCommand()
		{
			List<Type> list = new List<Type>(15)
				{
					typeof(Select<,>),
					typeof(AMBomOperCury),
					typeof(Where<,,>),
					typeof(AMBomOperCury.bOMID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomOper.bOMID),
					typeof(And<,,>),
					typeof(AMBomOperCury.revisionID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomOper.revisionID),
					typeof(And<,,>),
					typeof(AMBomOperCury.operationID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomOper.operationID),
					typeof(And<,>),
					typeof(AMBomOperCury.curyID),
					typeof(Equal<>),
					typeof(Optional<>),
					typeof(AccessInfo.baseCuryID)
				};

			return list;
		}
	}
}

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
	public class BomToolCurySettingsBOMMaint : BomToolCurySettingsBase<BOMMaint> { public static bool IsActive() => true; }
	public class BomToolCurySettingsEngineeringWorkbenchMaint : BomToolCurySettingsBase<EngineeringWorkbenchMaint> { public static bool IsActive() => true; }

	public abstract class BomToolCurySettingsBase<TGraph> : CurySettingsExtension<TGraph, AMBomTool, AMBomToolCury>
		where TGraph : PXGraph
	{
		protected override List<Type> ComposeCommand()
		{
			List<Type> list = new List<Type>(15)
				{
					typeof(Select<,>),
					typeof(AMBomToolCury),
					typeof(Where<,,>),
					typeof(AMBomToolCury.bOMID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomTool.bOMID),
					typeof(And<,,>),
					typeof(AMBomToolCury.revisionID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomTool.revisionID),
					typeof(And<,,>),
					typeof(AMBomToolCury.operationID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomTool.operationID),
					typeof(And<,,>),
					typeof(AMBomToolCury.lineID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomTool.lineID),
					typeof(And<,>),
					typeof(AMBomToolCury.curyID),
					typeof(Equal<>),
					typeof(Optional<>),
					typeof(AccessInfo.baseCuryID)
				};

			return list;
		}
	}
}

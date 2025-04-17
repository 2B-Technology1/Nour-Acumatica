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
	public class BomMatlCurySettingsBOMMaint : BomMatlCurySettingsBase<BOMMaint> { public static bool IsActive() => true; }
	public class BomMatlCurySettingsEngineeringWorkbenchMaint : BomMatlCurySettingsBase<EngineeringWorkbenchMaint> { public static bool IsActive() => true; }

	public abstract class BomMatlCurySettingsBase<TGraph> : CurySettingsExtension<TGraph, AMBomMatl, AMBomMatlCury>
		where TGraph : PXGraph
	{
		protected override List<Type> ComposeCommand()
		{
			List<Type> list = new List<Type>(15)
				{
					typeof(Select<,>),
					typeof(AMBomMatlCury),
					typeof(Where<,,>),
					typeof(AMBomMatlCury.bOMID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomMatl.bOMID),
					typeof(And<,,>),
					typeof(AMBomMatlCury.revisionID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomMatl.revisionID),
					typeof(And<,,>),
					typeof(AMBomMatlCury.operationID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomMatl.operationID),
					typeof(And<,,>),
					typeof(AMBomMatlCury.lineID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomMatl.lineID),
					typeof(And<,>),
					typeof(AMBomMatlCury.curyID),
					typeof(Equal<>),
					typeof(Optional<>),
					typeof(AccessInfo.baseCuryID)
				};

			return list;
		}
	}
}

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
using System.Linq;
using System.Text;

using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;


namespace PX.Objects.AR.BQL
{
	/// <summary>
	/// A predicate that returns <c>true</c> if and only if the
	/// <see cref="APRegister"/> descendant record can be added to
	/// a recurring transaction schedule.
	/// </summary>
	/// <remarks>
	/// Due to the absence of Exists&lt;&gt; BQL predicate, the BQL implementation
	/// of the current predicate does not check for the presence of <see cref="GLVoucher"/> 
	/// records referencing the document record. It should be done separately via a 
	/// left join and a null check. See <see cref="IsDocumentSchedulable"/> for example 
	/// and details.
	/// </remarks>
	public class IsSchedulable<TRegister> : IBqlUnary
		where TRegister : ARRegister
	{
		private static Type _whereType;
		
		private static IBqlCreator Where
		{
			get
			{
				if (_whereType == null)
				{
				Dictionary<string, Type> fields = typeof(TRegister)
					.GetNestedTypes()
					.ToDictionary(nestedTypes => nestedTypes.Name);

				Type releasedField = fields[nameof(ARRegister.released)];
				Type holdField = fields[nameof(ARRegister.hold)];
				Type voidedField = fields[nameof(ARRegister.voided)];
				Type origModuleField = fields[nameof(ARRegister.origModule)];
				Type isMigratedRecordField = fields[nameof(ARRegister.isMigratedRecord)];
				
					_whereType = BqlCommand.Compose(
					typeof(Where<,,>),
						releasedField, typeof(Equal<>), typeof(False),
						typeof(And<,,>), holdField, typeof(Equal<>), typeof(False),
						typeof(And<,,>), voidedField, typeof(Equal<>), typeof(False),
						typeof(And<,,>), origModuleField, typeof(Equal<>), typeof(BatchModule.moduleAR),
						typeof(And<,>), isMigratedRecordField, typeof(Equal<>), typeof(False));
				}

				return Activator.CreateInstance(_whereType) as IBqlUnary;
			}
		}

		public static bool IsDocumentSchedulable(PXGraph graph, ARRegister document)
		{
			PXSelectBase<ARRegister> schedulableDocumentSelect = new PXSelectJoin<
				ARRegister,
					LeftJoin<GLVoucher,
						On<GLVoucher.refNoteID, Equal<ARRegister.noteID>,
						And<FeatureInstalled<FeaturesSet.gLWorkBooks>>>>,
				Where<
					ARRegister.docType, Equal<Required<ARRegister.docType>>,
					And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>,
					And<GLVoucher.refNbr, IsNull,
					And<IsSchedulable<ARRegister>>>>>>(graph);

			return schedulableDocumentSelect.Any(document.DocType, document.RefNbr);
		}

		public static void Ensure(PXGraph graph, ARRegister document)
		{
			if (!IsDocumentSchedulable(graph, document))
			{
				throw new PXException(Messages.DocumentCannotBeScheduled);
			}
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> Where.AppendExpression(ref exp, graph, info, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			=> Where.Verify(cache, item, pars, ref result, ref value);
	}
}

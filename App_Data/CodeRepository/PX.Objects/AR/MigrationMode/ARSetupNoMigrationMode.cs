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
using PX.Objects.Common;

namespace PX.Objects.AR.MigrationMode
{
	/// <summary>
	/// An <see cref="PXSetup{ARSetup}"/> descendant. In addition to checking the
	/// presence of the setup record, also checks that <see cref="ARSetup.MigrationMode">
	/// migration mode</see> is not enabled in the module.
	/// </summary>
	public class ARSetupNoMigrationMode : PXSetup<ARSetup>
	{
		private PXGraph Graph
		{
			get;
			set;
		}

		public ARSetupNoMigrationMode(PXGraph graph) : base(graph)
		{
			Graph = graph;
		}

		public override ARSetup Current
		{
			get
			{
				ARSetup setup = base.Current;

				EnsureMigrationModeDisabled(Graph, setup);

				return setup;
			}
			set
			{
				base.Current = value;
			}
		}

		public static void EnsureMigrationModeDisabled(PXGraph graph, ARSetup setup = null)
		{
			setup = setup ?? PXSelectReadonly<ARSetup>.Select(graph);

			if (setup != null)
				setup.MigrationMode = CurrentConfiguration.ActualARSetup.isMigrationModeEnabled;

			if (setup?.MigrationMode != true) return;

			throw new PXSetupNotEnteredException(
				Messages.MigrationModeIsActivated,
				typeof(ARSetup),
				PXMessages.LocalizeNoPrefix(Messages.ARSetup));
		}
	}
}

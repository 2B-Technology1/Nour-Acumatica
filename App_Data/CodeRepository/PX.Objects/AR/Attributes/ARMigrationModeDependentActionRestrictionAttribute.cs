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

namespace PX.Objects.AR
{
	/// <summary>
	/// Provides a restrictor for the <see cref="PXAction{TNode}"> actions, which are dependent on migration mode logic. <br/>
	/// If you need to stop the action in migration mode, 
	/// set <see cref="restrictInMigrationMode"> parameter equal to true.
	/// If you need to stop the action for regular document in migration mode, 
	/// set <see cref="restrictForRegularDocumentInMigrationMode"> parameter equal to true.
	/// If you need to stop the action for unreleased migrated document in normal mode, 
	/// set <see cref="restrictForUnreleasedMigratedDocumentInNormalMode"> parameter equal to true.
	/// <example>
	/// [ARMigrationModeDependentActionRestriction(
	///    restrictInMigrationMode: false, 
	///    restrictForRegularDocumentInMigrationMode: true,
	///    restrictForUnreleasedMigratedDocumentInNormalMode: true)]
	/// </example>
	/// </summary>
	public class ARMigrationModeDependentActionRestrictionAttribute : PXAggregateAttribute
	{
		public ARMigrationModeDependentActionRestrictionAttribute(
			bool restrictInMigrationMode,
			bool restrictForRegularDocumentInMigrationMode,
			bool restrictForUnreleasedMigratedDocumentInNormalMode)
			: base()
		{
			if (restrictInMigrationMode)
			{
				_Attributes.Add(new PXActionRestrictionAttribute(typeof(Where<Current<ARSetup.migrationMode>, Equal<True>>),
					Messages.MigrationModeIsActivated));
			}

			if (restrictForRegularDocumentInMigrationMode)
			{
				_Attributes.Add(new PXActionRestrictionAttribute(typeof(Where<Current<ARRegister.isMigratedRecord>, NotEqual<True>,
					And<Current<ARSetup.migrationMode>, Equal<True>>>), Messages.MigrationModeIsActivatedForRegularDocument));
			}

			if (restrictForUnreleasedMigratedDocumentInNormalMode)
			{
				_Attributes.Add(new PXActionRestrictionAttribute(typeof(Where<Current<ARRegister.isMigratedRecord>, Equal<True>,
					And<Current<ARRegister.released>, NotEqual<True>,
					And<Current<ARSetup.migrationMode>, NotEqual<True>>>>), Messages.MigrationModeIsDeactivatedForMigratedDocument));
			}
		}
	}
}
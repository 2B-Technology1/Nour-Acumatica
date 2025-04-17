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

using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;

using SystemFieldNames = PX.Data.PXDBLastModifiedByIDAttribute.DisplayFieldNames;

namespace PX.Objects.PM
{
	/// <summary>
	/// Groups of projects for restricting user access via row-level security.
	/// </summary>
	// Acuminator disable once PX1034 MissingForeignKeyDeclaration [DAC has no foreign key]
	[Serializable]
	[PXCacheName(Messages.PMProjectGroup)]
	[PXPrimaryGraph(typeof(PMProjectGroupMaint))]
	public class PMProjectGroup : IBqlTable, PX.SM.IIncludable
	{
		public abstract class projectGroupID : BqlString.Field<projectGroupID>
		{
			public const int Length = 15;
		}

		/// <summary>
		/// The unique string identifier of the project group.
		/// </summary>
		[PXDBString(projectGroupID.Length, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Project Group ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.Both)]
		public virtual string ProjectGroupID { get; set; }

		public abstract class description : BqlString.Field<description>
		{
			public const int Length = 255;
		}

		/// <summary>
		/// The description of the project group.
		/// </summary>
		[PXDBString(description.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Description { get; set; }

		public abstract class isActive : BqlBool.Field<isActive> {}

		/// <summary>
		/// Is project group active or not.
		/// User cannot change project group of any project to inactive one.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; }

		public abstract class included : BqlBool.Field<included> {}

		/// <summary>
		/// An unbound field used in the user interface (PM102000)
		/// to include the project group into a <see cref="PX.SM.RelationGroup">restriction group</see>.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Included")]
		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Included { get; set; }

		public abstract class noteID : BqlGuid.Field<noteID> {}

		[PXNote]
		public virtual Guid? NoteID { get; set; }

		public abstract class createdByID : BqlGuid.Field<createdByID> {}

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }

		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> {}

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }

		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> {}

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = SystemFieldNames.CreatedDateTime,
			Enabled = false,
			IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }

		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> {}

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }

		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> {}

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }

		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> {}

		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = SystemFieldNames.LastModifiedDateTime,
			Enabled = false,
			IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime { get; set; }

		public abstract class Tstamp : BqlByteArray.Field<Tstamp> {}

		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }

		public abstract class groupMask : BqlByteArray.Field<groupMask> {}

		/// <summary>
		/// System field used for restrictions via row-level security.
		/// </summary>
		[PXDBGroupMask(HideFromEntityTypesList = true)]
		public virtual byte[] GroupMask { get; set; }

		public class PK : PrimaryKeyOf<PMProjectGroup>.By<projectGroupID>
		{
			public static PMProjectGroup Find(PXGraph graph, string projectGroupID)
				=> FindBy(graph, projectGroupID);
		}
	}
}

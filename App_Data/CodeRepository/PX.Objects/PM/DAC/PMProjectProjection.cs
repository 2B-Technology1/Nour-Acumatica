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
using PX.Objects.CT;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// Project Template projection based on PMProject
	/// </summary>
	[PXCacheName(Messages.PMProjectTemplate)]
	[Serializable]
	[PXProjection(typeof(Select<PMProject,
		Where<PMProject.baseType, Equal<CTPRType.projectTemplate>,
			And<PMProject.nonProject, Equal<False>>>>), Persistent = false)]
	public class PMProjectTemplate : IBqlTable
	{
		#region ContractID
		public new abstract class contractID : BqlInt.Field<contractID> { }

		/// <summary>The project template ID.</summary>
		[PXDBIdentity(BqlField = typeof(PMProject.contractID))]
		public virtual int? ContractID { get; set; }
		#endregion

		#region ContractID
		public abstract class contractCD : BqlString.Field<contractCD> { }

		/// <inheritdoc cref="PMProject.ContractCD" />
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty False alert.
		[PXDimensionSelector(
			ProjectAttribute.DimensionName,
			typeof(Search<PMProjectTemplate.contractCD>),
			typeof(PMProjectTemplate.contractCD),
			typeof(PMProjectTemplate.contractCD))]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(PMProject.contractCD))]
		[PXUIField(DisplayName = "Project ID")]
		public virtual string ContractCD { get; set; }
		#endregion

		#region Description
		public new abstract class description : BqlString.Field<description> { }

		/// <inheritdoc cref="PMProject.Description" />
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty False alert.
		[PXDBString(BqlField = typeof(PMProject.description))]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description { get; set; }
		#endregion

		#region Status
		public new abstract class status : BqlString.Field<status> { }

		/// <inheritdoc cref="PMProject.Status" />
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty False alert.
		[PXDBString(1, IsFixed = true, BqlField = typeof(PMProject.status))]
		[ProjectStatus.TemplStatusList()]
		[PXUIField(DisplayName = "Status")]
		public virtual string Status { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime(BqlField = typeof(PMProject.createdDateTime))]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID(BqlField = typeof(PMProject.createdByID))]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(PMProject.lastModifiedDateTime))]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID(BqlField = typeof(PMProject.lastModifiedByID))]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
	}
}

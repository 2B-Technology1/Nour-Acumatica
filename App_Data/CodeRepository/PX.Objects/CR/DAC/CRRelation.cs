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
using System.Diagnostics;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.Relations)]
	[DebuggerDisplay("{GetType().Name,nq} (ID = {RelationID}, Role = {Role}): {RefEntityType}: {RefNoteID} => {TargetType}: {TargetNoteID}, BAccountID = {EntityID}, ContactID = {ContactID}")]
	public class CRRelation : IBqlTable
	{
		#region Keys

		public class PK : PrimaryKeyOf<CRRelation>.By<relationID>
		{
			public static CRRelation Find(PXGraph graph, int? relationID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, relationID, options);
		}

		public class UK : PrimaryKeyOf<CRRelation>.By<refNoteID, targetNoteID, role>
		{
			public static CRRelation Find(PXGraph graph, Guid? refNoteID, Guid? targetNoteID, string role, PKFindOptions options = PKFindOptions.None) => FindBy(graph, refNoteID, targetNoteID, role, options);
		}

		#endregion

		#region RelationID

		public abstract class relationID : PX.Data.BQL.BqlInt.Field<relationID> { }

		[PXDBIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		[PXReverseRelation]
		public virtual Int32? RelationID { get; set; }

		#endregion

		#region RefNoteID

		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		[PXParent(typeof(Select<ARInvoice, Where<ARInvoice.noteID, Equal<Current<CRRelation.refNoteID>>>>))]
		[PXDBGuid]
		[PXDefault]
		public virtual Guid? RefNoteID { get; set; }

		#endregion

		#region RefEntityType
		public abstract class refEntityType : PX.Data.BQL.BqlString.Field<refEntityType> { }

		[PXDBString(40)]
		[PXUIField(DisplayName = "Ref Type")]
		[PXDefault]
		[CRRelationTypeListAttribure(typeof(CRRelation.role))]
		[PXFormula(typeof(Default<CRRelation.role>))]
		public virtual string RefEntityType { get; set; }
		#endregion

		#region Role

		public abstract class role : PX.Data.BQL.BqlString.Field<role> { }

		[PXDBString(2)]
		[PXUIField(DisplayName = "Role")]
		[PXDefault]
		[CRRoleTypeList.FullList]
		[PXUIEnabled(typeof(Where<Not<IsInDatabase>>))]
		public virtual String Role { get; set; }

		#endregion

		#region IsPrimary

		public abstract class isPrimary : PX.Data.BQL.BqlBool.Field<isPrimary> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Primary")]
		[PXDefault(false)]
		public virtual bool? IsPrimary { get; set; }

		#endregion

		#region TargetType

		public abstract class targetType : PX.Data.BQL.BqlString.Field<targetType> { }

		[PXDBString(40)]
		[PXUIField(DisplayName = "Type")]
		[PXDefault]
		[CRRelationTypeListAttribure(typeof(CRRelation.role))]
		[PXFormula(typeof(Default<CRRelation.role>))]
		[PXUIEnabled(typeof(Where<Not<IsInDatabase>>))]
		public virtual string TargetType { get; set; }

		#endregion

		#region TargetNoteID

		public abstract class targetNoteID : PX.Data.BQL.BqlGuid.Field<targetNoteID> { }

		[EntityIDSelector(typeof(CRRelation.targetType))]
		[PXDBGuid]
		[PXUIField(DisplayName = "Document")]
		[PXFormula(typeof(Default<CRRelation.targetType>))]
		[PXUIEnabled(typeof(Where<CRRelation.role.IsNotIn<
			CRRoleTypeList.referrer,
			CRRoleTypeList.supervisor,
			CRRoleTypeList.businessUser,
			CRRoleTypeList.decisionMaker,
			CRRoleTypeList.technicalExpert,
			CRRoleTypeList.supportEngineer,
			CRRoleTypeList.evaluator,
			CRRoleTypeList.licensee>>))]
		public virtual Guid? TargetNoteID { get; set; }

		#endregion

		#region DocNoteID

		public abstract class docNoteID : PX.Data.BQL.BqlGuid.Field<docNoteID> { }

		[PXDBGuid]
		public virtual Guid? DocNoteID { get; set; }

		#endregion

		#region EntityID

		public abstract class entityID : PX.Data.BQL.BqlInt.Field<entityID> { }

		[PXDBInt]
		[PXSelector(typeof(Search<
					BAccount.bAccountID,
				Where<BAccount.type, In3<
						BAccountType.prospectType,
						BAccountType.customerType,
						BAccountType.vendorType,
						BAccountType.combinedType>,
					And<Match<Current<AccessInfo.userName>>>>>),
			fieldList: new[]
			{
				typeof(BAccount.acctCD),
				typeof(BAccount.acctName),
				typeof(BAccount.classID),
				typeof(BAccount.type),
				typeof(BAccount.parentBAccountID),
				typeof(BAccount.acctReferenceNbr)
			},
			SubstituteKey = typeof(BAccount.acctCD),
			DescriptionField = typeof(BAccount.acctName),
			Filterable = true,
			DirtyRead = true)]
		[PXUIField(DisplayName = "Account")]
		[PXFormula(typeof(Default<CRRelation.targetNoteID>))]
		[CRRelationAccount]
		public virtual Int32? EntityID { get; set; }

		#endregion

		#region ContactID

		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Contact")]
		[PXFormula(typeof(Default<CRRelation.targetNoteID, CRRelation.entityID>))]
		[CRRelationContactSelector]
		public virtual Int32? ContactID { get; set; }

		#endregion

		#region AddToCC

		public abstract class addToCC : PX.Data.BQL.BqlBool.Field<addToCC> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Add to CC")]
		public virtual Boolean? AddToCC { get; set; }

		#endregion



		// Audit fields

		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		[PXUIField(DisplayName = "Creator", Enabled = false)]
		public virtual Guid? CreatedByID { get; set; }

		#endregion

		#region CreatedByScreenID

		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }

		#endregion

		#region CreatedDateTime

		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXUIField(DisplayName = "Created At", Enabled = false)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }

		#endregion

		#region LastModifiedByID

		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		[PXUIField(DisplayName = "Last Modified By", Enabled = false)]
		public virtual Guid? LastModifiedByID { get; set; }

		#endregion

		#region LastModifiedByScreenID

		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }

		#endregion

		#region LastModifiedDateTime

		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }

		#endregion


		// Unbound fields

		#region EntityCD

		public abstract class entityCD : PX.Data.BQL.BqlString.Field<entityCD> { }

		[PXString]
		[PXUIField(DisplayName = "Account/Employee", Enabled = false)]
		public virtual String EntityCD { get; set; }

		#endregion

		#region Name

		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

		[PXString]
		[PXUIField(DisplayName = "Name", Enabled = false)]
		public virtual String Name { get; set; }

		#endregion

		#region ContactName

		public abstract class contactName : PX.Data.BQL.BqlString.Field<contactName> { }

		[PXString]
		[PXUIField(DisplayName = "Contact", Enabled = false)]
		public virtual String ContactName { get; set; }

		#endregion

		#region Email

		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }

		[PXString]
		[PXUIField(DisplayName = "Email", Enabled = false)]
		public virtual String Email { get; set; }

		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// Status of the related entity.
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public virtual String Status { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// Description of the related entity.
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Description", Enabled = false)]
		public virtual String Description { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlString.Field<ownerID> { }

		/// <summary>
		/// An owner of the related entity.
		/// </summary>
		[Owner(IsDBField = false, Enabled = false)]
		public virtual int? OwnerID { get; set; }
		#endregion

		#region Methods

		public static void FillUnboundData(CRRelation relation, Contact contact, BAccount businessAccount, Users user)
		{
			relation.Name = businessAccount?.AcctName;
			relation.EntityCD = businessAccount?.AcctCD;
			relation.Email = contact?.EMail;

			if (businessAccount?.Type != BAccountType.EmployeeType)
			{
				relation.ContactName = contact?.DisplayName;
			}
			else
			{
				if (string.IsNullOrEmpty(relation.Name))
				{
					relation.Name = user?.FullName;
				}
				if (string.IsNullOrEmpty(relation.Email))
				{
					relation.Email = user?.Email;
				}
			}
		}

		#endregion
	}

	[Serializable]
	[PXHidden]
	public class CRRelation2 : CRRelation
	{
		public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		public new abstract class entityID : PX.Data.BQL.BqlInt.Field<entityID> { }
		public new abstract class role : PX.Data.BQL.BqlString.Field<role> { }
	}
}

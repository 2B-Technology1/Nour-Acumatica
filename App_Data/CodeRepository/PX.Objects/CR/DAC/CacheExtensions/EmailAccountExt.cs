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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.EP;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR
{
	[Serializable]
	public class EMailAccountExt : PXCacheExtension<EMailAccount>
	{
		#region EmailAccountID
		public abstract class emailAccountID : PX.Data.BQL.BqlInt.Field<emailAccountID> { }

		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.defContactID, Equal<Current<EMailAccount.defaultOwnerID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public virtual Int32? EmailAccountID { get; set; }
		#endregion

		#region DefaultWorkgroupID
		public abstract class defaultWorkgroupID : PX.Data.BQL.BqlInt.Field<defaultWorkgroupID> { }

		[PXFormula(typeof(IIf<Where<EMailAccount.defaultOwnerID.IsNotNull>, Null, EMailAccount.defaultWorkgroupID>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public virtual int? DefaultWorkgroupID { get; set; }
		#endregion

		#region DefaultOwnerID
		public abstract class defaultOwnerID : PX.Data.BQL.BqlInt.Field<defaultOwnerID> { }

		[PXDefault(typeof(Search<EPEmployee.defContactID, Where<EPEmployee.userID, Equal<Current<EMailAccount.userID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<EMailAccount.userID>))]
		[Owner(DisplayName = "Default Email Owner")]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual int? DefaultOwnerID { get; set; }
		#endregion

		#region Address
		public abstract class address : PX.Data.BQL.BqlString.Field<address> { }

		/// <summary>
		/// Email address for the corresponding email account
		/// </summary>
		[PXFormula(typeof(Switch<Case<Where<EntryStatus, Equal<EntryStatus.inserted>>, Selector<EMailAccount.userID, Users.email>>>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public virtual String Address { get; set; }
		#endregion

		#region DefaultEmailAssignmentMapID
		public abstract class defaultEmailAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultEmailAssignmentMapID> { }

		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID,
			Where<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeActivity>,
				Or<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeEmail>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public virtual int? DefaultEmailAssignmentMapID { get; set; }
		#endregion

		#region CreateCaseClassID
		public abstract class createCaseClassID : PX.Data.BQL.BqlString.Field<createCaseClassID> { }

		[PXSelector(typeof(CRCaseClass.caseClassID), DescriptionField = typeof(CRCaseClass.description), CacheGlobal = true)]
		[PXUIEnabled(typeof(EMailAccount.createCase))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public virtual string CreateCaseClassID { get; set; }
		#endregion

		#region CreateLeadClassID
		public abstract class createLeadClassID : PX.Data.BQL.BqlString.Field<createLeadClassID> { }

		[PXSelector(typeof(CRLeadClass.classID), DescriptionField = typeof(CRLeadClass.description), CacheGlobal = true)]
		[PXUIEnabled(typeof(EMailAccount.createLead))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public virtual string CreateLeadClassID { get; set; }
		#endregion

		#region CanUpdatePassword
		public abstract class canUpdatePassword : PX.Data.BQL.BqlBool.Field<canUpdatePassword> { }

		/// <summary>
		/// Field for enable / disable <i>Update Password</i> grid action on <see cref="MyProfileMaint">SM203010</see> screen
		/// </summary>
		[PXUIField(DisplayName = "Can Update Password", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXBool]
		[PXFormula(typeof(IIf<Where<EMailAccount.authenticationMethod, Equal<AuthenticationType.none>>, True, False>))]
		public virtual bool? CanUpdatePassword { get; set; }
		#endregion

		#region CanSignIn
		public abstract class canSignIn : PX.Data.BQL.BqlBool.Field<canSignIn> { }

		/// <summary>
		/// Field for enable / disable <i>Sign In</i> grid action on <see cref="MyProfileMaint">SM203010</see> screen
		/// </summary>
		[PXUIField(DisplayName = "Can Sign In", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXBool]
		[PXFormula(typeof(IIf<
			Where<EMailAccount.authenticationMethod, Equal<AuthenticationType.oAuth2>,
				And<EMailAccount.emailAccountType, NotEqual<EmailAccountTypesAttribute.exchange>>>,
			True,
			False>))]
		public virtual bool? CanSignIn { get; set; }
		#endregion
	}
}

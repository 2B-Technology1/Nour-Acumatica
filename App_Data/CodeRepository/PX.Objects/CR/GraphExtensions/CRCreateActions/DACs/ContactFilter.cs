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

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class ContactFilter : IBqlTable, IClassIdFilter
	{
		#region FirstName

		public abstract class firstName : PX.Data.BQL.BqlString.Field<firstName> { }

		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "First Name")]
		public virtual string FirstName { get; set; }

		#endregion

		#region LastName

		public abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }

		[PXDefault]
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Last Name", Required = true)]
		public virtual string LastName { get; set; }

		#endregion

		#region FullName

		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
		[PXDBString]
		[PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string FullName { get; set; }

		#endregion

		#region Salutation

		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Job Title", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Salutation { get; set; }

		#endregion

		#region Phone1

		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
		[PhoneValidation]
		public virtual string Phone1 { get; set; }

		#endregion

		#region Phone1Type

		public abstract class phone1Type : PX.Data.BQL.BqlString.Field<phone1Type> { }

		[PXDBString(3)]
		[PXDefault(PhoneTypesAttribute.Business1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 1 Type")]
		[PhoneTypes]
		public virtual string Phone1Type { get; set; }

		#endregion

		#region Phone2

		public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Phone 2", Visibility = PXUIVisibility.SelectorVisible)]
		[PhoneValidation]
		public virtual string Phone2 { get; set; }

		#endregion

		#region Phone2Type

		public abstract class phone2Type : PX.Data.BQL.BqlString.Field<phone2Type> { }

		[PXDBString(3)]
		[PXDefault(PhoneTypesAttribute.Cell, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 2 Type")]
		[PhoneTypes]
		public virtual string Phone2Type { get; set; }

		#endregion

		#region Email

		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }

		[PXDBEmail]
		[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string Email { get; set; }

		#endregion

		#region ContactClass

		public abstract class contactClass : PX.Data.BQL.BqlString.Field<contactClass> { }

		[PXDefault(typeof(CRSetup.defaultContactClassID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Contact Class")]
		[PXSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description))]
		public virtual string ContactClass { get; set; }

		string IClassIdFilter.ClassID => ContactClass;

		#endregion
	}
}
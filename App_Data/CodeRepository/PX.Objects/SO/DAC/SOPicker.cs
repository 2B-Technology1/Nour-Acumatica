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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.IN;
using PX.SM;
using System;

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOPicker, PXDacType.Details)]
	public class SOPicker : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOPicker>.By<worksheetNbr, pickerNbr>
		{
			public static SOPicker Find(PXGraph graph, string worksheetNbr, int? pickerNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, worksheetNbr, pickerNbr, options);
		}

		public static class FK
		{
			public class User : Users.PK.ForeignKeyOf<SOPicker>.By<userID> { }
			public class Worksheet : SOPickingWorksheet.PK.ForeignKeyOf<SOPicker>.By<worksheetNbr> { }
			public class Cart : INCart.PK.ForeignKeyOf<SOPicker>.By<siteID, cartID> { }
			public class FirstLocation : INLocation.PK.ForeignKeyOf<SOPicker>.By<firstLocationID> { }
			public class LastLocation : INLocation.PK.ForeignKeyOf<SOPicker>.By<lastLocationID> { }
			public class SortingLocation : INLocation.PK.ForeignKeyOf<SOPicker>.By<sortingLocationID> { }
			public class Site : INSite.PK.ForeignKeyOf<SOPicker>.By<siteID> { }
		}
		#endregion

		#region WorksheetNbr
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Worksheet Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBDefault(typeof(SOPickingWorksheet.worksheetNbr))]
		[PXParent(typeof(FK.Worksheet))]
		public virtual String WorksheetNbr { get; set; }
		public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
		#endregion
		#region PickerNbr
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(SOPickingWorksheet))]
		[PXUIField(DisplayName = "Picker Nbr.")]
		public virtual Int32? PickerNbr { get; set; }
		public abstract class pickerNbr : PX.Data.BQL.BqlInt.Field<pickerNbr> { }
		#endregion
		#region SiteID
		[Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(SOPickingWorksheet.siteID))]
		[PXForeignReference(typeof(FK.Site))]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>>))]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region UserID
		[PXDBGuid]
		[PXUIField(DisplayName = "User", Enabled = false)]
		[PXSelector(typeof(Search<Users.pKID, Where<Users.isHidden, Equal<False>>>), SubstituteKey = typeof(Users.username))]
		[PXForeignReference(typeof(FK.User))]
		public virtual Guid? UserID { get; set; }
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		#endregion
		#region CartID
		[PXDBInt]
		[PXUIField(DisplayName = "Cart ID", IsReadOnly = true)]
		[PXSelector(typeof(SearchFor<INCart.cartID>.In<SelectFrom<INCart>.Where<INCart.active.IsEqual<True>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		[PXForeignReference(typeof(FK.Cart))]
		public int? CartID { get; set; }
		public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
		#endregion
		#region NumberOfTotes
		[PXDBInt]
		[PXUIField(DisplayName = "Nbr. of Totes")]
		public int? NumberOfTotes { get; set; }
		public abstract class numberOfTotes : PX.Data.BQL.BqlInt.Field<numberOfTotes> { }
		#endregion
		#region FirstLocationID
		[Location(typeof(siteID), DisplayName = "First Location", Enabled = false)]
		[PXDefault]
		[PXForeignReference(typeof(FK.FirstLocation))]
		public virtual int? FirstLocationID { get; set; }
		public abstract class firstLocationID : PX.Data.BQL.BqlInt.Field<firstLocationID> { }
		#endregion
		#region LastLocationID
		[Location(typeof(siteID), DisplayName = "Last Location", Enabled = false)]
		[PXDefault]
		[PXForeignReference(typeof(FK.LastLocation))]
		public virtual int? LastLocationID { get; set; }
		public abstract class lastLocationID : PX.Data.BQL.BqlInt.Field<lastLocationID> { }
		#endregion
		#region PathLength
		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Path Length")]
		public virtual Int32? PathLength { get; set; }
		public abstract class pathLength : PX.Data.BQL.BqlInt.Field<pathLength> { }
		#endregion
		#region SortingLocationID
		[Location(typeof(siteID), DisplayName = "Sorting Location", Enabled = false)]
		[PXForeignReference(typeof(FK.SortingLocation))]
		public virtual int? SortingLocationID { get; set; }
		public abstract class sortingLocationID : PX.Data.BQL.BqlInt.Field<sortingLocationID> { }
		#endregion
		#region Confirmed
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Confirmed")]
		public bool? Confirmed { get; set; }
		public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
		#endregion

		#region PickListNbr
		[PXString]
		public string PickListNbr
		{
			get => WorksheetNbr + "/" + PickerNbr;
			set { }
		}
		public abstract class pickListNbr : PX.Data.BQL.BqlString.Field<pickListNbr> { }
		#endregion

		#region Audit Fields
		#region tstamp
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		[PXUIField(DisplayName = "Created At", Enabled = false, IsReadOnly = true)]
		public virtual String CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedByID, Enabled = false, IsReadOnly = true)]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		[PXUIField(DisplayName = "Last Modified At", Enabled = false, IsReadOnly = true)]
		public virtual String LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion
	}
}

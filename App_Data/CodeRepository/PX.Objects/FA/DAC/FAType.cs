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

namespace PX.Objects.FA
{
	[Serializable]
	[PXPrimaryGraph(typeof(AssetTypeMaint))]
	[PXCacheName(Messages.FAType)]
	public partial class FAType : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<FAType>.By<assetTypeID>
		{
			public static FAType Find(PXGraph graph, String assetTypeID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, assetTypeID, options);
		}
		#endregion
		#region AssetTypeID
		public abstract class assetTypeID : PX.Data.BQL.BqlString.Field<assetTypeID> { }
		protected string _AssetTypeID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Asset Type ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<FAType.assetTypeID>))]
		public virtual string AssetTypeID
		{
			get
			{
				return _AssetTypeID;
			}
			set
			{
				_AssetTypeID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected string _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Description
		{
			get
			{
				return _Description;
			}
			set
			{
				_Description = value;
			}
		}
		#endregion	
		#region IsTangible
		public abstract class isTangible : PX.Data.BQL.BqlBool.Field<isTangible> { }
		protected bool? _IsTangible;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Tangible", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? IsTangible
		{
			get
			{
				return _IsTangible;
			}
			set
			{
				_IsTangible = value;
			}
		}
		#endregion
		#region Depreciable
		public abstract class depreciable : PX.Data.BQL.BqlBool.Field<depreciable> { }
		protected bool? _Depreciable;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Depreciable", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? Depreciable
		{
			get
			{
				return _Depreciable;
			}
			set
			{
				_Depreciable = value;
			}
		}
		#endregion
	}
}

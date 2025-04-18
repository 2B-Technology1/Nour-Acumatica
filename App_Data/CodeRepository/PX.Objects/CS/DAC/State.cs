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

namespace PX.Objects.CS
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.State)]
	[PXPrimaryGraph(
		new Type[] { typeof(CountryMaint)},
		new Type[] { typeof(Select<State, 
			Where<State.countryID, Equal<Current<State.countryID>>, 
			  And<State.stateID, Equal<Current<State.stateID>>>>>)
		})]
	public partial class State : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<State>.By<countryID, stateID>
		{
			public static State Find(PXGraph graph, string countryID, string stateID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, countryID, stateID, options);
		}
		public static class FK
		{
			public class SalesTerritory : CS.SalesTerritory.PK.ForeignKeyOf<State>.By<salesTerritoryID> { }
		}

		#endregion

		#region Selected
		/// <summary>
		/// Indicates whether the record is selected or not.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		#endregion

		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		protected String _CountryID;
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask = ">??")]
		[PXDefault(typeof(Country.countryID))]
		[PXUIField(DisplayName = "Country",Visible = false)]
		[PXSelector(typeof(Country.countryID), DirtyRead = true)]
		[PXParent(typeof(Select<Country,Where<Country.countryID,Equal<Current<State.countryID>>>>))]
		public virtual String CountryID
		{
			get
			{
				return this._CountryID;
			}
			set
			{
				this._CountryID = value;
			}
		}
		#endregion
		#region StateID
		public abstract class stateID : PX.Data.BQL.BqlString.Field<stateID> { }
		protected String _StateID;
        [PXDBString(50, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "State ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
		[PXReferentialIntegrityCheck]
		public virtual String StateID
		{
			get
			{
				return this._StateID;
			}
			set
			{
				this._StateID = value;
			}
		}
		#endregion
		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		protected String _Name;
		[PXDBLocalizableString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "State Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}
		
		#endregion
		#region StateRegexp
		public abstract class stateRegexp : PX.Data.BQL.BqlString.Field<stateRegexp> { }

		[PXDBString(255)]
		[PXUIField(DisplayName = "Validation Regexp")]
		public virtual String StateRegexp { get; set; }
		#endregion

        #region LocationCode
        public abstract class locationCode : PX.Data.IBqlField
        {
        }
        protected String _LocationCode;
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Location Code", Visible = false, FieldClass = nameof(FeaturesSet.PayrollModule))]
        public virtual String LocationCode
        {
            get
            {
                return this._LocationCode;
            }
            set
            {
                this._LocationCode = value;
            }
        }
        #endregion

		#region IsTaxRegistrationRequired
		public abstract class isTaxRegistrationRequired : PX.Data.BQL.BqlBool.Field<isTaxRegistrationRequired> { }
		protected Boolean? _IsTaxRegistrationRequired;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax Registration Required")]
		public virtual Boolean? IsTaxRegistrationRequired
		{
			get
			{
				return this._IsTaxRegistrationRequired;
			}
			set
			{
				this._IsTaxRegistrationRequired = value;
			}
		}
		#endregion
		#region TaxRegistrationMask
		public abstract class taxRegistrationMask : PX.Data.BQL.BqlString.Field<taxRegistrationMask> { }
		protected String _TaxRegistrationMask;
		[PXDBString(50)]
		[PXUIField(DisplayName = "Tax Registration Mask")]
		public virtual String TaxRegistrationMask
		{
			get
			{
				return this._TaxRegistrationMask;
			}
			set
			{
				this._TaxRegistrationMask = value;
			}
		}
		#endregion
		#region TaxRegistrationRegexp
		public abstract class taxRegistrationRegexp : PX.Data.BQL.BqlString.Field<taxRegistrationRegexp> { }
		protected String _TaxRegistrationRegexp;
		[PXDBString(255)]
		[PXUIField(DisplayName = "Tax Registration Reg. Exp.")]
		public virtual String TaxRegistrationRegexp
		{
			get
			{
				return this._TaxRegistrationRegexp;
			}
			set
			{
				this._TaxRegistrationRegexp = value;
			}
		}
		#endregion
		#region NonTaxable
		public abstract class nonTaxable : PX.Data.BQL.BqlBool.Field<nonTaxable> { }
		/// <summary>
		/// Get or set NonTaxable that mark current state does not impose sales taxes.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Non-Taxable")]
		[PXDefault(false)]
		public virtual bool? NonTaxable
		{
			get;
			set;
		}
		#endregion
		#region SalesTerritoryID
		/// <summary>
		/// The reference to <see cref="SalesTerritory.salesTerritoryID"/> to that country belongs to. 
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Sales Territory ID", Visibility = PXUIVisibility.SelectorVisible, FieldClass = FeaturesSet.salesTerritoryManagement.FieldClass)]
		[PXSelector(typeof(Search<SalesTerritory.salesTerritoryID,
							Where<SalesTerritory.salesTerritoryType, Equal<SalesTerritoryTypeAttribute.byState>,
								And<SalesTerritory.countryID, Equal<Current<State.countryID>>>>>),
					fieldList: new []
					{
						typeof(SalesTerritory.salesTerritoryID),
						typeof(SalesTerritory.name)
					},
					DescriptionField = typeof(SalesTerritory.name))]
		[PXRestrictor(typeof(Where<SalesTerritory.isActive, Equal<True>>), CS.MessagesNoPrefix.SalesTerritoryInactive, ShowWarning = true)]
		[PXForeignReference(typeof(FK.SalesTerritory), Data.ReferentialIntegrity.ReferenceBehavior.SetNull)]
		public virtual String SalesTerritoryID { get; set; }
		public abstract class salesTerritoryID : PX.Data.BQL.BqlString.Field<salesTerritoryID> { }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		/// <exclude/>
		public abstract class Tstamp : Data.BQL.BqlByteArray.Field<Tstamp> { }
		/// <exclude/>
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}
}

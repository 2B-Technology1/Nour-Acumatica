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
using PX.Objects.GL;

namespace PX.Objects.CM
{
	/// <summary>
	/// Represents the currency translation definition, which determines the parameters of translation, such as source and destination
	/// ledgers and currencies. Records of this type are accompanied by details of the <see cref="TranslDefDet"/> type.
	/// Translation definitions are edited on the Translation Definition (CM203000) form backed by the <see cref="TranslationDefinitionMaint"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(TranslationDefinitionMaint))]
	[PXCacheName(Messages.TranslDef)]
	public partial class TranslDef : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<TranslDef>.By<translDefId>
		{
			public static TranslDef Find(PXGraph graph, String translDefId, PKFindOptions options = PKFindOptions.None) => FindBy(graph, translDefId, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<TranslDef>.By<branchID> { }
			public class SourceLedger : GL.Ledger.PK.ForeignKeyOf<TranslDef>.By<sourceLedgerId> { }
			public class DestinationLedger : GL.Ledger.PK.ForeignKeyOf<TranslDef>.By<destLedgerId> { }
			public class SourceCurrency : CM.Currency.PK.ForeignKeyOf<TranslDef>.By<sourceCuryID> { }
			public class DestinationCurrency : CM.Currency.PK.ForeignKeyOf<TranslDef>.By<destCuryID> { }
		}
		#endregion
		#region TranslDefId
		public abstract class translDefId : PX.Data.BQL.BqlString.Field<translDefId> { }
		protected String _TranslDefId;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
        [PXUIField(DisplayName = "Translation ID", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [PXSelector(typeof(TranslDef.translDefId))]
		[PX.Data.EP.PXFieldDescription]
		public virtual String TranslDefId
		{
			get
			{
				return this._TranslDefId;
			}
			set
			{
				this._TranslDefId = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(Required = false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected Boolean? _Active;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region SourceLedgerId
		public abstract class sourceLedgerId : PX.Data.BQL.BqlInt.Field<sourceLedgerId> { }
		protected Int32? _SourceLedgerId;
        [PXDBInt()]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
        [PXUIField(DisplayName = "Source Ledger ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<Ledger.ledgerID, Where<Ledger.balanceType, Equal<ActualLedger>, Or<Ledger.balanceType, Equal<ReportLedger>>>>), 
	        SubstituteKey = typeof(Ledger.ledgerCD), 
	        DescriptionField = typeof(Ledger.descr),
	        CacheGlobal = true)]
        public virtual Int32? SourceLedgerId
		{
			get
			{
				return this._SourceLedgerId;
			}
			set
			{
				this._SourceLedgerId = value;
			}
		}
		#endregion
		#region DestLedgerId
		public abstract class destLedgerId : PX.Data.BQL.BqlInt.Field<destLedgerId> { }
		protected Int32? _DestLedgerId;
        [PXDBInt()]
		[PXDefault()]
        [PXUIField(DisplayName = "Destination Ledger ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<Ledger.ledgerID, Where<Ledger.balanceType, Equal<ReportLedger>>>), 
	        SubstituteKey = typeof(Ledger.ledgerCD), 
	        DescriptionField = typeof(Ledger.descr),
	        CacheGlobal = true)]
        public virtual Int32? DestLedgerId
		{
			get
			{
				return this._DestLedgerId;
			}
			set
			{
				this._DestLedgerId = value;
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected Int32? _LineCntr;
        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote(DescriptionField = typeof(TranslDef.translDefId))]
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
		
		#region SourceCuryID
		public abstract class sourceCuryID : PX.Data.BQL.BqlString.Field<sourceCuryID> { }
		protected String _SourceCuryID;
		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Source Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String SourceCuryID
		{
			get
			{
				return this._SourceCuryID;
			}
			set
			{
				this._SourceCuryID = value;
			}
		}
		#endregion
		#region DestCuryID
		public abstract class destCuryID : PX.Data.BQL.BqlString.Field<destCuryID> { }
		protected String _DestCuryID;
		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Destination Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String DestCuryID
		{
			get
			{
				return this._DestCuryID;
			}
			set
			{
				this._DestCuryID = value;
			}
		}
		#endregion
			
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
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
    public class ActualLedger : PX.Data.BQL.BqlString.Constant<ActualLedger>
	{
        public ActualLedger() : base("A") { ;}
    }
    public class ReportLedger : PX.Data.BQL.BqlString.Constant<ReportLedger>
	{
        public ReportLedger() : base("R") { ;}
    }
}

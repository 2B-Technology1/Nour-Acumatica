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

namespace PX.Objects.AR
{
	/// <summary>
	/// Header of Dunning Letter
	/// </summary>
	[Serializable]
    [PXPrimaryGraph(typeof(ARDunningLetterUpdate))] //for notification
	[PXCacheName(Messages.DunningLetter)]
    public partial class ARDunningLetter : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<ARDunningLetter>.By<dunningLetterID, branchID> // TODO: ??? is branchID key field?
		{
			public static ARDunningLetter Find(PXGraph graph, Int32? dunningLetterID, Int32? branchID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, dunningLetterID, branchID, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<ARDunningLetter>.By<branchID> { }
			public class Customer : AR.Customer.PK.ForeignKeyOf<ARDunningLetter>.By<bAccountID> { }
		}
		#endregion

		#region DunningLetterID
		public abstract class dunningLetterID : PX.Data.BQL.BqlInt.Field<dunningLetterID> { }
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Dunning Letter ID", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(ARDunningLetter.dunningLetterID), 
			new Type[]
			{
				typeof(ARDunningLetter.dunningLetterID),
				typeof(ARDunningLetter.branchID_Branch_branchCD),
				typeof(ARDunningLetter.bAccountID_Customer_acctCD),
				typeof(ARDunningLetter.dunningLetterDate),
				typeof(ARDunningLetter.dunningLetterLevel),
				typeof(ARDunningLetter.deadline)
			})]
		public virtual Int32? DunningLetterID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public abstract class branchID_Branch_branchCD : PX.Data.BQL.BqlString.Field<branchID_Branch_branchCD> { }
		[PXDefault()]
		[GL.Branch]
        public virtual Int32? BranchID
		{
			get;
			set;
		}
		#endregion
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public abstract class bAccountID_Customer_acctCD : PX.Data.BQL.BqlString.Field<bAccountID_Customer_acctCD> { }
        [PXDBInt()]
        [PXDefault()]
		[PXUIField(DisplayName = "Customer", IsReadOnly=true)]
		[PXSelector(typeof(Search<Customer.bAccountID, Where<Customer.bAccountID, Equal<Current<ARDunningLetter.bAccountID>>>>), DescriptionField = typeof(Customer.acctCD), ValidateValue = false)]
		public virtual Int32? BAccountID
		{
			get;
			set;
		}
        #endregion
        #region DunningLetterDate
		public abstract class dunningLetterDate : PX.Data.BQL.BqlDateTime.Field<dunningLetterDate> { }
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Dunning Letter Date", IsReadOnly=true)]
		public virtual DateTime? DunningLetterDate
		{
			get;
			set;
		}
		#endregion
        #region Deadline
        public abstract class deadline : PX.Data.BQL.BqlDateTime.Field<deadline> { }
        [PXDBDate()]
        [PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Deadline")]
        public virtual DateTime? Deadline
		{
			get;
			set;
		}
        #endregion
        #region DunningLetterLevel
        public abstract class dunningLetterLevel : PX.Data.BQL.BqlInt.Field<dunningLetterLevel> { }
		[PXDBInt()]
		[PXDefault()]
        [PXUIField(DisplayName = Messages.DunningLetterLevel, IsReadOnly = true)]
        public virtual Int32? DunningLetterLevel
		{
			get;
			set;
		}
		#endregion
        #region Printed
        public abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Printed
		{
			get;
			set;
		}
        #endregion
        #region DontPrint
        public abstract class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Don't Print")]
        public virtual Boolean? DontPrint
		{
			get;
			set;
		}
        #endregion
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Released
		{
			get;
			set;
		}
        #endregion
        #region Voided
        public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Voided
		{
			get;
			set;
		}
        #endregion
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        [PXString(1, IsFixed = true)]
        [PXDefault("D")]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXStringList(new string[] { "D", "R", "V" },
                new string[] { Messages.Draft, Messages.Released, Messages.Voided })]
        public virtual String Status
        {
            get
            {
                if (Voided == true)
                    return "V";
                if (Released == true)
                    return "R";
                return "D";
            }
            set { }
        }
        #endregion
        #region DetailsCount
        public abstract class detailsCount : PX.Data.BQL.BqlInt.Field<detailsCount> { }
        [PXInt]
        [PXUIField(DisplayName="Number of Documents")]
        public virtual Int32? DetailsCount
		{
			get;
			set;
		}
        #endregion
        #region FeeDocType
        public abstract class feeDocType : PX.Data.BQL.BqlString.Field<feeDocType> { }
        [PXDBString(3, IsFixed = true)]
        [ARDocType.List()]
        [PXUIField(DisplayName = "Fee Type")]
        public virtual String FeeDocType
		{
			get;
			set;
		}
        #endregion
        #region FeeRefNbr
        public abstract class feeRefNbr : PX.Data.BQL.BqlString.Field<feeRefNbr> { }
        [PXDBString(15, IsUnicode = true)]
        [PXSelector(typeof(Search<ARInvoice.refNbr, Where<ARInvoice.docType, Equal<Current<ARDunningLetter.feeDocType>>, And<ARInvoice.refNbr, Equal<Current<ARDunningLetter.feeRefNbr>>>>>),ValidateValue=false)]
        [PXUIField(DisplayName = "Fee Reference Nbr.", Enabled=false)]
        public virtual String FeeRefNbr
		{
			get;
			set;
		}
		#endregion
		#region DunningFee
		public abstract class dunningFee : PX.Data.BQL.BqlDecimal.Field<dunningFee> { }
		[PXDBDecimal(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Dunning Fee")]
		public virtual Decimal? DunningFee { get; set; }
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Selector<branchID, GL.Branch.baseCuryID>))]
		public virtual string CuryID { get; set; }
		#endregion

		#region Emailed
		public abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Emailed
		{
			get;
			set;
		}
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        [PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
        #endregion
        #region DontEmail
        public abstract class dontEmail : PX.Data.BQL.BqlBool.Field<dontEmail> { }
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Don't Email")]
        public virtual Boolean? DontEmail
		{
			get;
			set;
		}
		#endregion
		#region ConsolidationSettings
		public abstract class consolidationSettings : PX.Data.BQL.BqlShort.Field<consolidationSettings> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ARSetup.prepareDunningLetters.ConsolidatedForCompany)]
		public virtual string ConsolidationSettings { get; set; }
		#endregion
        #region LastLevel
        public abstract class lastLevel : PX.Data.BQL.BqlBool.Field<lastLevel> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? LastLevel
		{
			get;
			set;
		}
        #endregion

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
		{
			get;
			set;
		}
        #endregion
    }

}

using System;
using PX.Data;
using PX.Data.BQL;

namespace SurveyCustomization
{
  [Serializable]
  [PXCacheName("SurveyTemplate")]
  public class SurveyTemplate : IBqlTable
  {
    #region SurveyTemplateID
    [PXDBIdentity(IsKey = true)]
    [PXUIField(DisplayName = "ID")]
    [PXSelector(
    typeof(SurveyTemplate.surveyTemplateID), // DAC field to be used for selection
    typeof(SurveyTemplate.surveyTemplateID), // Field to display in the selector
    typeof(SurveyTemplate.surveyTemplateName), // Additional column to show in the dropdown
    DescriptionField = typeof(SurveyTemplate.surveyTemplateName)) // Field for the description tooltip
    ]
        public virtual int? SurveyTemplateID { get; set; }
    public abstract class surveyTemplateID : PX.Data.BQL.BqlInt.Field<surveyTemplateID> { }
    #endregion

    #region SurveyTemplateName
    [PXDBString(255, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Name")]
    public virtual string SurveyTemplateName { get; set; }
    public abstract class surveyTemplateName : PX.Data.BQL.BqlString.Field<surveyTemplateName> { }
    #endregion

    #region LineCntr
    [PXDBInt()]
    [PXDefault(0)]
    [PXUIField(DisplayName = "Line Cntr")]
    public virtual int? LineCntr { get; set; }
    public abstract class lineCntr : BqlInt.Field<lineCntr> { }
    #endregion


    #region CreatedDateTime
    [PXDBCreatedDateTime()]
    public virtual DateTime? CreatedDateTime { get; set; }
    public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
    #endregion

    #region CreatedByID
    [PXDBCreatedByID()]
    public virtual Guid? CreatedByID { get; set; }
    public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
    #endregion

    #region CreatedByScreenID
    [PXDBCreatedByScreenID()]
    public virtual string CreatedByScreenID { get; set; }
    public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
    #endregion

    #region LastModifiedDateTime
    [PXDBLastModifiedDateTime()]
    public virtual DateTime? LastModifiedDateTime { get; set; }
    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
    #endregion

    #region LastModifiedByID
    [PXDBLastModifiedByID()]
    public virtual Guid? LastModifiedByID { get; set; }
    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
    #endregion

    #region LastModifiedByScreenID
    [PXDBLastModifiedByScreenID()]
    public virtual string LastModifiedByScreenID { get; set; }
    public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
    #endregion

    #region Tstamp
    [PXDBTimestamp()]
    public virtual byte[] Tstamp { get; set; }
    public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
    #endregion

    #region NoteID
    [PXNote()]
    public virtual Guid? NoteID { get; set; }
    public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
    #endregion
  
  }
}
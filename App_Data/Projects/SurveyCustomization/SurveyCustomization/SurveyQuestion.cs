using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using static SurveyCustomization.SurveyQuestion;

namespace SurveyCustomization
{
  [Serializable]
  [PXCacheName("SurveyQuestion")]
  public class SurveyQuestion : IBqlTable
  {
    #region SurveyTemplateID
    [PXDBInt(IsKey = true)]
    [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
    [PXDBDefault(typeof(SurveyTemplate.surveyTemplateID))]
    [PXParent(typeof(SelectFrom<SurveyTemplate>.
                            Where<SurveyTemplate.surveyTemplateID.
                        IsEqual<SurveyQuestion.surveyTemplateID.FromCurrent>>))]

    [PXUIField(DisplayName = "Survey Template ID")]
    public virtual int? SurveyTemplateID { get; set; }
    public abstract class surveyTemplateID : PX.Data.BQL.BqlInt.Field<surveyTemplateID> { }
    #endregion

    #region LineNbr
    [PXDBInt(IsKey = true)]
    [PXUIField(DisplayName = "Line Nbr")]
    [PXDefault]
    [PXLineNbr(typeof(SurveyTemplate.lineCntr))]
    public virtual int? LineNbr { get; set; }
    public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
    #endregion

    #region QuestionText
    [PXDBString(255, IsUnicode = true, InputMask = "")]
    [PXDefault]
    [PXUIField(DisplayName = "Question")]
    public virtual string Questiontext { get; set; }
    public abstract class questionText : PX.Data.BQL.BqlString.Field<questionText> { }
    #endregion

    #region Answertext
    [PXDBString(255, IsUnicode = true, InputMask = "")]
    [PXSelector(
    typeof(Search<survayanswers.answers,
    Where<survayanswers.qType, Equal<Current<SurveyQuestion.questionType>>>>), // Filter by QuestionType
    typeof(survayanswers.answers),
    ValidateValue = false)] // Tooltip for the selector

    [PXUIField(DisplayName = "Answertext")]
    public virtual string Answertext { get; set; }
    public abstract class answertext : PX.Data.BQL.BqlString.Field<answertext> { }
    #endregion

    #region AnswerYesNo
    [PXString(255)]
    [PXUIField(DisplayName = "Answer Yes No")]
    [PXStringList(
        new string[] { "Yes", "No" },
        new string[] { "Yes", "No" }
    )]
    public virtual string AnswerYesNo { get; set; }
    public abstract class answerYesNo : PX.Data.BQL.BqlString.Field<answerYesNo> { }
    #endregion

    #region AnswerRating
    [PXDBString(255, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Answer Rating")]
    [PXStringList(
        new string[] { "Very Bad", "Bad", "Good", "Very Good", "Excellent" },
        new string[] { "Very Bad", "Bad", "Good", "Very Good", "Excellent" }
    )]
    public virtual string AnswerRating { get; set; }
    public abstract class answerRating : PX.Data.BQL.BqlString.Field<answerRating> { }
    #endregion

    #region QuestionType
    [PXDBInt()]
    [PXDefault]
    [PXUIField(DisplayName = "Question Type")]
    public virtual int? QuestionType { get; set; }
    public abstract class questionType : PX.Data.BQL.BqlInt.Field<questionType> { }
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
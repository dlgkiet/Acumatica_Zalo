using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN;
using PX.SM;
using System;

namespace AnNhienCafe
{
    [Serializable]
    [PXCacheName("Zalo Template")]
    [PXPrimaryGraph(typeof(ZaloTemplateMaint))]
    public class ZaloTemplate : PXBqlTable, IBqlTable
    {
        #region NotificationID
        [PXDBIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Notification ID")]
        [PXSelector(typeof(Search<ZaloTemplate.notificationID>), DescriptionField = typeof(ZaloTemplate.description))]
        public virtual int? NotificationID { get; set; }
        public abstract class notificationID : PX.Data.BQL.BqlInt.Field<notificationID> { }
        #endregion

        #region SubscriberID
        [PXDBGuid()]
        [PXUIField(DisplayName = "Subscriber ID")]
        public virtual Guid? SubscriberID { get; set; }
        public abstract class subscriberID : PX.Data.BQL.BqlGuid.Field<subscriberID> { }
        #endregion

        #region Description
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Template Name")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual string Description { get; set; }
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        #endregion

        #region Screen
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Screen ID")]
        [PXSelector(
            typeof(Search<SiteMap.screenID>),
            typeof(SiteMap.title),
            typeof(SiteMap.url),
            SubstituteKey = typeof(SiteMap.screenID),
            DescriptionField = typeof(SiteMap.title)
        )]
        public virtual string Screen { get; set; }
        public abstract class screen : PX.Data.BQL.BqlString.Field<screen> { }
        #endregion

        #region From
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "From")]
        [PXSelector(typeof(Search<Users.username>),
                    typeof(Users.username),
                    typeof(Users.firstName),
                    typeof(Users.lastName),
                    SubstituteKey = typeof(Users.username),
                    DescriptionField = typeof(Users.firstName))]
        public virtual string From { get; set; }
        public abstract class from : PX.Data.BQL.BqlString.Field<from> { }
        #endregion

        #region To
        [PXDBString(1000, IsUnicode = false)]
        [PXUIField(DisplayName = "To Users")]
        [PXSelector(typeof(Search<ZaloUser.zaloUserID,
                           Where<ZaloUser.isActive, Equal<True>>>),
                    typeof(ZaloUser.zaloUserID),
                    typeof(ZaloUser.name),
                    typeof(ZaloUser.role),
                    SubstituteKey = typeof(ZaloUser.zaloUserID),
                    DescriptionField = typeof(ZaloUser.name))]
        public virtual string To { get; set; }
        public abstract class to : PX.Data.BQL.BqlString.Field<to> { }
        #endregion

        #region Cc
        [PXDBString(1000, IsUnicode = false)]
        [PXUIField(DisplayName = "CC Users")]
        [PXSelector(typeof(Search<ZaloUser.zaloUserID,
                           Where<ZaloUser.isActive, Equal<True>>>),
                    typeof(ZaloUser.zaloUserID),
                    typeof(ZaloUser.name),
                    typeof(ZaloUser.role),
                    SubstituteKey = typeof(ZaloUser.zaloUserID),
                    DescriptionField = typeof(ZaloUser.name))]
        public virtual string Cc { get; set; }
        public abstract class cc : PX.Data.BQL.BqlString.Field<cc> { }
        #endregion

        #region Bcc
        [PXDBString(1000, IsUnicode = false)]
        [PXUIField(DisplayName = "BCC Users")]
        [PXSelector(typeof(Search<ZaloUser.zaloUserID,
                           Where<ZaloUser.isActive, Equal<True>>>),
                    typeof(ZaloUser.zaloUserID),
                    typeof(ZaloUser.name),
                    typeof(ZaloUser.role),
                    SubstituteKey = typeof(ZaloUser.zaloUserID),
                    DescriptionField = typeof(ZaloUser.name))]
        public virtual string Bcc { get; set; }
        public abstract class bcc : PX.Data.BQL.BqlString.Field<bcc> { }
        #endregion

        #region Subject
        [PXDBString(500, IsUnicode = true)]
        [PXUIField(DisplayName = "Message Subject")]
        public virtual string Subject { get; set; }
        public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }
        #endregion

        #region LinkToEntity
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Link to Entity")]
        public virtual string LinkToEntity { get; set; }
        public abstract class linkToEntity : PX.Data.BQL.BqlString.Field<linkToEntity> { }
        #endregion

        #region LinkToContact
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Link to Contact")]
        public virtual string LinkToContact { get; set; }
        public abstract class linkToContact : PX.Data.BQL.BqlString.Field<linkToContact> { }
        #endregion

        #region LinkToAccount
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Link to Account")]
        public virtual string LinkToAccount { get; set; }
        public abstract class linkToAccount : PX.Data.BQL.BqlString.Field<linkToAccount> { }
        #endregion

        #region ActivityType
        [PXDBString(50, IsUnicode = true)]
        [PXDefault("Zalo")]
        [PXUIField(DisplayName = "Activity Type")]
        [PXStringList(new[] { "Zalo" }, new[] { "Zalo" })]
        public virtual string ActivityType { get; set; }
        public abstract class activityType : PX.Data.BQL.BqlString.Field<activityType> { }
        #endregion

        #region ReferenceNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Reference Number")]
        [PXSelector(
            typeof(Search<INPIHeader.pIID>),
            typeof(INPIHeader.createdDateTime),
            typeof(INPIHeader.siteID),
            DescriptionField = typeof(INPIHeader.siteID)
        )]
        public virtual string ReferenceNbr { get; set; }
        public abstract class referenceNbr : PX.Data.BQL.BqlString.Field<referenceNbr> { }
        #endregion

        #region Body
        [PXDBText(IsUnicode = true)]
        [PXUIField(DisplayName = "Body")]
        [PXNoteText] 
        public virtual string Body { get; set; }
        public abstract class body : PX.Data.BQL.BqlString.Field<body> { }
        #endregion

        #region PreviewMessage
        [PXDBString(4000, IsUnicode = true)]
        [PXUIField(DisplayName = "Preview Message", Enabled = false)]
        public virtual string PreviewMessage { get; set; }
        public abstract class previewMessage : PX.Data.BQL.BqlString.Field<previewMessage> { }
        #endregion

        #region System Columns
        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By")]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created Date")]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "Last Modified By")]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modified Date")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region Active
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? IsActive { get; set; }
        public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
        #endregion

        #region NoteID
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}
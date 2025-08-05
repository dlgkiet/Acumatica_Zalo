using System;
using PX.Data;

namespace AnNhienCafe
{
    [Serializable]
    [PXCacheName("Zalo Token")]
    [PXPrimaryGraph(typeof(ZaloTokenMaint))]
    public class ZaloToken : PXBqlTable, IBqlTable
    {
        #region TokenID
        [PXDBGuid]
        [PXUIField(DisplayName = "Token ID", Enabled = false)]
        public virtual Guid? TokenID { get; set; }
        public abstract class tokenID : PX.Data.BQL.BqlGuid.Field<tokenID> { }
        #endregion

        #region AppID
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "App ID")]
        public virtual string AppID { get; set; }
        public abstract class appID : PX.Data.BQL.BqlString.Field<appID> { }
        #endregion

        #region AppSecret
        [PXDBString(200, IsUnicode = true)]
        [PXUIField(DisplayName = "App Secret")]
        public virtual string AppSecret { get; set; }
        public abstract class appSecret : PX.Data.BQL.BqlString.Field<appSecret> { }
        #endregion

        #region RefreshToken
        [PXDBString(500, IsUnicode = true)]
        [PXUIField(DisplayName = "Refresh Token")]
        public virtual string RefreshToken { get; set; }
        public abstract class refreshToken : PX.Data.BQL.BqlString.Field<refreshToken> { }
        #endregion

        #region AccessToken
        [PXDBString(500, IsUnicode = true)]
        [PXUIField(DisplayName = "Access Token")]
        public virtual string AccessToken { get; set; }
        public abstract class accessToken : PX.Data.BQL.BqlString.Field<accessToken> { }
        #endregion

        #region AccessTokenExpiredAt
        [PXDBDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Access Token Expired At")]
        public virtual DateTime? AccessTokenExpiredAt { get; set; }
        public abstract class accessTokenExpiredAt : PX.Data.BQL.BqlDateTime.Field<accessTokenExpiredAt> { }
        #endregion

        #region NoteID
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion

        #region Timestamp
        [PXDBTimestamp]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion

        #region Helper Properties - Computed fields
        [PXBool]
        [PXUIField(DisplayName = "Access Token Valid", Enabled = false)]
        public virtual bool? IsAccessTokenValid
        {
            get
            {
                return AccessTokenExpiredAt.HasValue && AccessTokenExpiredAt.Value > DateTime.Now;
            }
        }
        public abstract class isAccessTokenValid : PX.Data.BQL.BqlBool.Field<isAccessTokenValid> { }
        #endregion
    }
}
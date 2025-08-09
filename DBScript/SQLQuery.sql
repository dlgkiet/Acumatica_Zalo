use ANCAFE
go

drop table ZaloTemplate

CREATE TABLE ZaloTemplate
(
    -- Key fields
    NotificationID int IDENTITY(1,1) PRIMARY KEY,
    SubscriberID uniqueidentifier NULL,
    
    -- Basic info
    Description nvarchar(100) NULL,
    Screen nvarchar(100) NULL,
    IsActive bit NOT NULL DEFAULT(1),
    ActivityType nvarchar(50) NOT NULL DEFAULT('Zalo'),
    
    -- Message info
    [From] nvarchar(255) NULL,
    [To] nvarchar(1000) NULL,
    Cc nvarchar(1000) NULL, 
    Bcc nvarchar(1000) NULL,
    Subject nvarchar(500) NULL,
    Body nvarchar(4000) NULL,
    PreviewMessage nvarchar(4000) NULL,
    
    -- References
    ReferenceNbr nvarchar(15) NULL,
    LinkToEntity nvarchar(100) NULL,
    LinkToContact nvarchar(100) NULL,
    LinkToAccount nvarchar(100) NULL,
    
    -- System fields
    CreatedByID uniqueidentifier NULL,
    CreatedDateTime datetime NULL,
    LastModifiedByID uniqueidentifier NULL,
    LastModifiedDateTime datetime NULL,
    NoteID uniqueidentifier NULL,

);

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON ZaloTemplate TO public;
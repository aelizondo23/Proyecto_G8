USE [master]
GO

-- =============================================
-- base de datos
-- =============================================
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'FieldTech')
BEGIN
    ALTER DATABASE [FieldTech] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [FieldTech];
END
GO

CREATE DATABASE [FieldTech]
GO

USE [FieldTech]
GO

-- 1. Users
CREATE TABLE [dbo].[Users] (
    [UserId]       INT IDENTITY(1,1) NOT NULL,
    [Email]        NVARCHAR(150)     NOT NULL,
    [PasswordHash] NVARCHAR(255)     NOT NULL,
    [Phone]        NVARCHAR(20)      NULL,
    [FirstName]    NVARCHAR(100)     NULL,
    [LastName]     NVARCHAR(100)     NULL,
    [UserType]     NVARCHAR(10)      NOT NULL,   -- 'TECH' | 'CLIENT'
    [IsActive]     BIT               NULL CONSTRAINT DF_Users_IsActive  DEFAULT (1),
    [CreatedAt]    DATETIME2(7)      NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT UK_Users_Email UNIQUE NONCLUSTERED ([Email] ASC)
)
GO

-- 2. TechnicianProfile
CREATE TABLE [dbo].[TechnicianProfile] (
    [UserId]             INT            NOT NULL,
    [Bio]                NVARCHAR(MAX)  NULL,
    [HourlyRate]         DECIMAL(10,2)  NULL,
    [Zone]               NVARCHAR(100)  NULL,
    [AvailabilityStatus] NVARCHAR(50)   NULL,
    [PortfolioUrl]       NVARCHAR(255)  NULL,
    CONSTRAINT PK_TechnicianProfile PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT FK_Tech_User FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
)
GO

-- 3. ClientProfile
CREATE TABLE [dbo].[ClientProfile] (
    [UserId]       INT            NOT NULL,
    [ClientType]   NVARCHAR(50)   NULL,
    [DisplayName]  NVARCHAR(150)  NULL,
    [ContactName]  NVARCHAR(150)  NULL,
    [ContactPhone] NVARCHAR(20)   NULL,
    [LocationText] NVARCHAR(255)  NULL,
    CONSTRAINT PK_ClientProfile PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT FK_Client_User FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
)
GO

-- 4. FileStorage
CREATE TABLE [dbo].[FileStorage] (
    [FileId]            INT IDENTITY(1,1) NOT NULL,
    [UploadedByUserId]  INT               NOT NULL,
    [OriginalFileName]  NVARCHAR(255)     NULL,
    [StoragePathOrUrl]  NVARCHAR(500)     NULL,
    [MimeType]          NVARCHAR(100)     NULL,
    [SizeBytes]         INT               NULL,
    [CreatedAt]         DATETIME2(7)      NULL CONSTRAINT DF_FileStorage_CreatedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_FileStorage PRIMARY KEY CLUSTERED ([FileId] ASC)
)
GO

-- 5. TechnicianCV
CREATE TABLE [dbo].[TechnicianCV] (
    [TechnicianUserId] INT          NOT NULL,
    [FileId]           INT          NOT NULL,
    [UpdatedAt]        DATETIME2(7) NULL CONSTRAINT DF_TechCV_UpdatedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_TechnicianCV PRIMARY KEY CLUSTERED ([TechnicianUserId] ASC),
    CONSTRAINT FK_CV_Tech FOREIGN KEY ([TechnicianUserId]) REFERENCES [dbo].[TechnicianProfile] ([UserId]),
    CONSTRAINT FK_CV_File FOREIGN KEY ([FileId])           REFERENCES [dbo].[FileStorage]       ([FileId])
)
GO

-- 6. TechnicianEducation
CREATE TABLE [dbo].[TechnicianEducation] (
    [EducationId]      INT IDENTITY(1,1) NOT NULL,
    [TechnicianUserId] INT               NOT NULL,
    [Institution]      NVARCHAR(150)     NULL,
    [TitleOrDegree]    NVARCHAR(150)     NULL,
    [Year]             INT               NULL,
    [Notes]            NVARCHAR(500)     NULL,
    CONSTRAINT PK_TechnicianEducation PRIMARY KEY CLUSTERED ([EducationId] ASC),
    CONSTRAINT FK_Edu_Tech FOREIGN KEY ([TechnicianUserId]) REFERENCES [dbo].[TechnicianProfile] ([UserId])
)
GO

-- 7. TechnicianExperience
CREATE TABLE [dbo].[TechnicianExperience] (
    [ExperienceId]     INT IDENTITY(1,1) NOT NULL,
    [TechnicianUserId] INT               NOT NULL,
    [RoleTitle]        NVARCHAR(150)     NULL,
    [CompanyName]      NVARCHAR(150)     NULL,
    [StartYear]        INT               NULL,
    [EndYear]          INT               NULL,
    [Description]      NVARCHAR(MAX)     NULL,
    CONSTRAINT PK_TechnicianExperience PRIMARY KEY CLUSTERED ([ExperienceId] ASC),
    CONSTRAINT FK_Exp_Tech FOREIGN KEY ([TechnicianUserId]) REFERENCES [dbo].[TechnicianProfile] ([UserId])
)
GO

-- 8. WorkOrder
CREATE TABLE [dbo].[WorkOrder] (
    [WorkOrderId]  INT IDENTITY(1,1) NOT NULL,
    [ClientUserId] INT               NOT NULL,
    [Title]        NVARCHAR(200)     NULL,
    [Description]  NVARCHAR(MAX)     NULL,
    [Category]     NVARCHAR(100)     NULL,
    [LocationText] NVARCHAR(255)     NULL,
    [BudgetAmount] DECIMAL(10,2)     NULL,
    [Urgency]      NVARCHAR(20)      NULL,
    [Status]       NVARCHAR(20)      NULL CONSTRAINT DF_WorkOrder_Status    DEFAULT ('OPEN'),
    [CreatedAt]    DATETIME2(7)      NULL CONSTRAINT DF_WorkOrder_CreatedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_WorkOrder PRIMARY KEY CLUSTERED ([WorkOrderId] ASC),
    CONSTRAINT FK_WorkOrder_Client FOREIGN KEY ([ClientUserId]) REFERENCES [dbo].[ClientProfile] ([UserId])
)
GO

-- 9. WorkOrderAssignment
CREATE TABLE [dbo].[WorkOrderAssignment] (
    [AssignmentId]     INT IDENTITY(1,1) NOT NULL,
    [WorkOrderId]      INT               NOT NULL,
    [TechnicianUserId] INT               NOT NULL,
    [Status]           NVARCHAR(20)      NULL,
    [AgreedAmount]     DECIMAL(10,2)     NULL,
    [CreatedAt]        DATETIME2(7)      NULL CONSTRAINT DF_Assign_CreatedAt DEFAULT (SYSDATETIME()),
    [AssignedAt]       DATETIME2(7)      NULL,
    [RespondedAt]      DATETIME2(7)      NULL,
    CONSTRAINT PK_WorkOrderAssignment PRIMARY KEY CLUSTERED ([AssignmentId] ASC),
    CONSTRAINT FK_Assign_Order FOREIGN KEY ([WorkOrderId])      REFERENCES [dbo].[WorkOrder]         ([WorkOrderId]),
    CONSTRAINT FK_Assign_Tech  FOREIGN KEY ([TechnicianUserId]) REFERENCES [dbo].[TechnicianProfile] ([UserId])
)
GO

-- 10. WorkOrderCheckIn
CREATE TABLE [dbo].[WorkOrderCheckIn] (
    [CheckInId]        INT IDENTITY(1,1) NOT NULL,
    [WorkOrderId]      INT               NOT NULL,
    [TechnicianUserId] INT               NOT NULL,
    [CheckInAt]        DATETIME2(7)      NULL,
    [CheckOutAt]       DATETIME2(7)      NULL,
    [Notes]            NVARCHAR(500)     NULL,
    [DurationMinutes]  INT               NULL,
    CONSTRAINT PK_WorkOrderCheckIn PRIMARY KEY CLUSTERED ([CheckInId] ASC),
    CONSTRAINT FK_CheckIn_Order FOREIGN KEY ([WorkOrderId])      REFERENCES [dbo].[WorkOrder]         ([WorkOrderId]),
    CONSTRAINT FK_CheckIn_Tech  FOREIGN KEY ([TechnicianUserId]) REFERENCES [dbo].[TechnicianProfile] ([UserId])
)
GO

-- 11. WorkOrderMessage
CREATE TABLE [dbo].[WorkOrderMessage] (
    [MessageId]    INT IDENTITY(1,1) NOT NULL,
    [WorkOrderId]  INT               NOT NULL,
    [SenderUserId] INT               NOT NULL,
    [Body]         NVARCHAR(MAX)     NULL,
    [CreatedAt]    DATETIME2(7)      NULL CONSTRAINT DF_Message_CreatedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_WorkOrderMessage PRIMARY KEY CLUSTERED ([MessageId] ASC),
    CONSTRAINT FK_Message_Order FOREIGN KEY ([WorkOrderId])  REFERENCES [dbo].[WorkOrder] ([WorkOrderId]),
    CONSTRAINT FK_Message_User  FOREIGN KEY ([SenderUserId]) REFERENCES [dbo].[Users]     ([UserId])
)
GO

-- 12. WorkOrderMessageAttachment
CREATE TABLE [dbo].[WorkOrderMessageAttachment] (
    [AttachmentId] INT IDENTITY(1,1) NOT NULL,
    [MessageId]    INT               NOT NULL,
    [FileId]       INT               NOT NULL,
    CONSTRAINT PK_WorkOrderMessageAttachment PRIMARY KEY CLUSTERED ([AttachmentId] ASC),
    CONSTRAINT FK_Attachment_Message FOREIGN KEY ([MessageId]) REFERENCES [dbo].[WorkOrderMessage] ([MessageId]),
    CONSTRAINT FK_Attachment_File    FOREIGN KEY ([FileId])    REFERENCES [dbo].[FileStorage]       ([FileId])
)
GO

-- =============================================
-- ÍNDICES
-- =============================================
CREATE NONCLUSTERED INDEX IX_Tech_Zone        ON [dbo].[TechnicianProfile]    ([Zone] ASC)
CREATE NONCLUSTERED INDEX IX_Client_Type      ON [dbo].[ClientProfile]        ([ClientType] ASC)
CREATE NONCLUSTERED INDEX IX_Users_Email      ON [dbo].[Users]                ([Email] ASC)
CREATE NONCLUSTERED INDEX IX_Users_UserType   ON [dbo].[Users]                ([UserType] ASC)
CREATE NONCLUSTERED INDEX IX_WorkOrder_Status ON [dbo].[WorkOrder]            ([Status] ASC)
CREATE NONCLUSTERED INDEX IX_WorkOrder_Client ON [dbo].[WorkOrder]            ([ClientUserId] ASC)
CREATE NONCLUSTERED INDEX IX_WorkOrder_Cat    ON [dbo].[WorkOrder]            ([Category] ASC)
CREATE NONCLUSTERED INDEX IX_WorkOrder_Urg    ON [dbo].[WorkOrder]            ([Urgency] ASC)
CREATE NONCLUSTERED INDEX IX_Assign_Order     ON [dbo].[WorkOrderAssignment]  ([WorkOrderId] ASC)
CREATE NONCLUSTERED INDEX IX_Assign_Tech      ON [dbo].[WorkOrderAssignment]  ([TechnicianUserId] ASC)
CREATE NONCLUSTERED INDEX IX_CheckIn_Order    ON [dbo].[WorkOrderCheckIn]     ([WorkOrderId] ASC)
CREATE NONCLUSTERED INDEX IX_CheckIn_Tech     ON [dbo].[WorkOrderCheckIn]     ([TechnicianUserId] ASC)
CREATE NONCLUSTERED INDEX IX_Message_Order    ON [dbo].[WorkOrderMessage]     ([WorkOrderId] ASC)
CREATE NONCLUSTERED INDEX IX_Message_Date     ON [dbo].[WorkOrderMessage]     ([CreatedAt] ASC)
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================

-- =============================================
-- sp_RegisterUser
-- =============================================
CREATE PROCEDURE [dbo].[sp_RegisterUser]
    @Email       NVARCHAR(150),
    @PasswordHash NVARCHAR(255),
    @UserType    INT,            -- 0 = TECH, 1 = CLIENT
    @FirstName   NVARCHAR(100),
    @LastName    NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        RAISERROR('EMAIL_EXISTS', 16, 1);
        RETURN;
    END

    DECLARE @UserTypeStr NVARCHAR(10) = CASE WHEN @UserType = 0 THEN 'TECH' ELSE 'CLIENT' END;

    INSERT INTO Users (Email, PasswordHash, UserType, FirstName, LastName, IsActive, CreatedAt)
    VALUES (@Email, @PasswordHash, @UserTypeStr, @FirstName, @LastName, 1, SYSDATETIME());

    DECLARE @UserId INT = SCOPE_IDENTITY();

    IF @UserTypeStr = 'TECH'
        INSERT INTO TechnicianProfile (UserId) VALUES (@UserId);
    ELSE
        INSERT INTO ClientProfile (UserId, ClientType) VALUES (@UserId, 'GENERAL');

    SELECT @UserId AS UserId;
END
GO

-- =============================================
-- sp_LoginUser
-- =============================================
CREATE PROCEDURE [dbo].[sp_LoginUser]
    @Email        NVARCHAR(150),
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        Email,
        UserType,
        FirstName,
        LastName
    FROM Users
    WHERE Email        = @Email
      AND PasswordHash = @PasswordHash
      AND IsActive     = 1;
END
GO

-- =============================================
-- sp_ValidarCorreo  (usado por RecuperarAcceso)
-- =============================================
CREATE PROCEDURE [dbo].[sp_ValidarCorreo]
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        Email,
        FirstName,
        LastName,
        UserType
    FROM Users
    WHERE Email    = @Email
      AND IsActive = 1;
END
GO

-- =============================================
-- sp_UpdateUserCredentials
-- =============================================
CREATE PROCEDURE [dbo].[sp_UpdateUserCredentials]
    @UserId          INT,
    @NewPasswordHash NVARCHAR(255) = NULL,
    @NewPhone        NVARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND IsActive = 1)
    BEGIN
        RAISERROR('USER_NOT_FOUND', 16, 1);
        RETURN;
    END

    UPDATE Users
    SET
        PasswordHash = ISNULL(@NewPasswordHash, PasswordHash),
        Phone        = ISNULL(@NewPhone,        Phone)
    WHERE UserId = @UserId;

    SELECT 'OK' AS Result;
END
GO

-- =============================================
-- sp_CreateWorkOrder
-- =============================================
CREATE PROCEDURE [dbo].[sp_CreateWorkOrder]
    @ClientUserId INT,
    @Title        NVARCHAR(200),
    @Description  NVARCHAR(MAX) = NULL,
    @Category     NVARCHAR(100) = NULL,
    @LocationText NVARCHAR(255) = NULL,
    @BudgetAmount DECIMAL(10,2) = NULL,
    @Urgency      NVARCHAR(20)  = 'NORMAL'
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM ClientProfile WHERE UserId = @ClientUserId)
    BEGIN
        RAISERROR('CLIENT_NOT_FOUND', 16, 1);
        RETURN;
    END

    INSERT INTO WorkOrder (ClientUserId, Title, Description, Category, LocationText, BudgetAmount, Urgency, Status)
    VALUES (@ClientUserId, @Title, @Description, @Category, @LocationText, @BudgetAmount, @Urgency, 'OPEN');

    SELECT SCOPE_IDENTITY() AS WorkOrderId;
END
GO

-- =============================================
-- sp_UpdateWorkOrder
-- =============================================
CREATE PROCEDURE [dbo].[sp_UpdateWorkOrder]
    @WorkOrderId  INT,
    @Title        NVARCHAR(200) = NULL,
    @Description  NVARCHAR(MAX) = NULL,
    @Category     NVARCHAR(100) = NULL,
    @LocationText NVARCHAR(255) = NULL,
    @BudgetAmount DECIMAL(10,2) = NULL,
    @Urgency      NVARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM WorkOrder WHERE WorkOrderId = @WorkOrderId)
    BEGIN
        RAISERROR('WORKORDER_NOT_FOUND', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM WorkOrder WHERE WorkOrderId = @WorkOrderId AND Status = 'OPEN')
    BEGIN
        RAISERROR('WORKORDER_NOT_EDITABLE', 16, 1);
        RETURN;
    END

    UPDATE WorkOrder
    SET
        Title        = ISNULL(@Title,        Title),
        Description  = ISNULL(@Description,  Description),
        Category     = ISNULL(@Category,     Category),
        LocationText = ISNULL(@LocationText, LocationText),
        BudgetAmount = ISNULL(@BudgetAmount, BudgetAmount),
        Urgency      = ISNULL(@Urgency,      Urgency)
    WHERE WorkOrderId = @WorkOrderId;

    SELECT 'OK' AS Result;
END
GO

-- =============================================
-- sp_CancelWorkOrder
-- =============================================
CREATE PROCEDURE [dbo].[sp_CancelWorkOrder]
    @WorkOrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM WorkOrder WHERE WorkOrderId = @WorkOrderId)
    BEGIN
        RAISERROR('WORKORDER_NOT_FOUND', 16, 1);
        RETURN;
    END

    UPDATE WorkOrder SET Status = 'CANCELLED' WHERE WorkOrderId = @WorkOrderId;

    UPDATE WorkOrderAssignment
    SET Status = 'CANCELLED'
    WHERE WorkOrderId = @WorkOrderId AND Status IN ('PENDING', 'ACCEPTED');

    SELECT 'OK' AS Result;
END
GO

-- =============================================
-- sp_GetWorkOrder
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetWorkOrder]
    @WorkOrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        wo.WorkOrderId,
        wo.Title,
        wo.Description,
        wo.Category,
        wo.LocationText,
        wo.BudgetAmount,
        wo.Urgency,
        wo.Status,
        wo.CreatedAt,
        u.FirstName + ' ' + u.LastName   AS ClientName,
        cp.DisplayName                   AS ClientDisplayName,
        cp.ContactPhone                  AS ClientPhone
    FROM WorkOrder wo
    INNER JOIN ClientProfile cp ON cp.UserId = wo.ClientUserId
    INNER JOIN Users         u  ON u.UserId  = wo.ClientUserId
    WHERE wo.WorkOrderId = @WorkOrderId;
END
GO

-- =============================================
-- sp_ListWorkOrders
-- =============================================
CREATE PROCEDURE [dbo].[sp_ListWorkOrders]
    @Status         NVARCHAR(20)  = NULL,
    @Category       NVARCHAR(100) = NULL,
    @Urgency        NVARCHAR(20)  = NULL,
    @Zone           NVARCHAR(100) = NULL,
    @SoloDisponibles BIT          = 0,
    @PageNum        INT           = 1,
    @PageSize       INT           = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        wo.WorkOrderId,
        wo.Title,
        wo.Description,
        wo.Category,
        wo.LocationText,
        wo.BudgetAmount,
        wo.Urgency,
        wo.Status,
        wo.CreatedAt,
        u.FirstName + ' ' + u.LastName   AS ClientName,
        cp.DisplayName                   AS ClientDisplayName,
        cp.ContactPhone                  AS ClientPhone
    FROM WorkOrder wo
    INNER JOIN ClientProfile cp ON cp.UserId = wo.ClientUserId
    INNER JOIN Users         u  ON u.UserId  = wo.ClientUserId
    WHERE
        (@Status   IS NULL OR wo.Status   = @Status)
        AND (@Category IS NULL OR wo.Category = @Category)
        AND (@Urgency  IS NULL OR wo.Urgency  = @Urgency)
        AND (@SoloDisponibles = 0 OR (
                wo.Status = 'OPEN'
                AND NOT EXISTS (
                    SELECT 1 FROM WorkOrderAssignment
                    WHERE WorkOrderId = wo.WorkOrderId AND Status = 'ACCEPTED'
                )
            )
        )
    ORDER BY wo.CreatedAt DESC
    OFFSET (@PageNum - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- sp_AplicarOrden  (técnico se auto-asigna)
-- =============================================
CREATE PROCEDURE [dbo].[sp_AplicarOrden]
    @WorkOrderId      INT,
    @TechnicianUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM WorkOrder WHERE WorkOrderId = @WorkOrderId AND Status = 'OPEN')
    BEGIN
        RAISERROR('WORKORDER_NOT_AVAILABLE', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM WorkOrderAssignment
               WHERE WorkOrderId = @WorkOrderId AND Status = 'ACCEPTED')
    BEGIN
        RAISERROR('WORKORDER_ALREADY_TAKEN', 16, 1);
        RETURN;
    END

    INSERT INTO WorkOrderAssignment (WorkOrderId, TechnicianUserId, Status, AssignedAt, RespondedAt)
    VALUES (@WorkOrderId, @TechnicianUserId, 'ACCEPTED', SYSDATETIME(), SYSDATETIME());

    -- Actualizar estado de la orden a IN_PROGRESS
    UPDATE WorkOrder SET Status = 'IN_PROGRESS' WHERE WorkOrderId = @WorkOrderId;

    SELECT SCOPE_IDENTITY() AS AssignmentId;
END
GO

-- =============================================
-- sp_CreateAssignment  (cliente asigna técnico)
-- =============================================
CREATE PROCEDURE [dbo].[sp_CreateAssignment]
    @WorkOrderId      INT,
    @TechnicianUserId INT,
    @AgreedAmount     DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM WorkOrder WHERE WorkOrderId = @WorkOrderId AND Status = 'OPEN')
    BEGIN
        RAISERROR('WORKORDER_NOT_AVAILABLE', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM WorkOrderAssignment
               WHERE WorkOrderId = @WorkOrderId AND TechnicianUserId = @TechnicianUserId
               AND Status != 'CANCELLED')
    BEGIN
        RAISERROR('ASSIGNMENT_ALREADY_EXISTS', 16, 1);
        RETURN;
    END

    INSERT INTO WorkOrderAssignment (WorkOrderId, TechnicianUserId, Status, AgreedAmount, AssignedAt)
    VALUES (@WorkOrderId, @TechnicianUserId, 'PENDING', @AgreedAmount, SYSDATETIME());

    SELECT SCOPE_IDENTITY() AS AssignmentId;
END
GO

-- =============================================
-- sp_RespondAssignment  (técnico acepta/rechaza)
-- =============================================
CREATE PROCEDURE [dbo].[sp_RespondAssignment]
    @AssignmentId INT,
    @Accept       BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM WorkOrderAssignment WHERE AssignmentId = @AssignmentId AND Status = 'PENDING')
    BEGIN
        RAISERROR('ASSIGNMENT_NOT_PENDING', 16, 1);
        RETURN;
    END

    DECLARE @NewStatus NVARCHAR(20) = CASE WHEN @Accept = 1 THEN 'ACCEPTED' ELSE 'REJECTED' END;

    UPDATE WorkOrderAssignment
    SET Status = @NewStatus, RespondedAt = SYSDATETIME()
    WHERE AssignmentId = @AssignmentId;

    -- Si aceptó, pasar la orden a IN_PROGRESS
    IF @Accept = 1
    BEGIN
        DECLARE @WorkOrderId INT;
        SELECT @WorkOrderId = WorkOrderId FROM WorkOrderAssignment WHERE AssignmentId = @AssignmentId;
        UPDATE WorkOrder SET Status = 'IN_PROGRESS' WHERE WorkOrderId = @WorkOrderId;
    END

    SELECT 'OK' AS Result;
END
GO

-- =============================================
-- sp_CompleteAssignment
-- =============================================
CREATE PROCEDURE [dbo].[sp_CompleteAssignment]
    @AssignmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM WorkOrderAssignment WHERE AssignmentId = @AssignmentId AND Status = 'ACCEPTED')
    BEGIN
        RAISERROR('ASSIGNMENT_NOT_ACTIVE', 16, 1);
        RETURN;
    END

    UPDATE WorkOrderAssignment
    SET Status = 'COMPLETED', RespondedAt = SYSDATETIME()
    WHERE AssignmentId = @AssignmentId;

    -- Actualizar la orden a COMPLETED
    DECLARE @WorkOrderId INT;
    SELECT @WorkOrderId = WorkOrderId FROM WorkOrderAssignment WHERE AssignmentId = @AssignmentId;
    UPDATE WorkOrder SET Status = 'COMPLETED' WHERE WorkOrderId = @WorkOrderId;

    SELECT 'OK' AS Result;
END
GO

-- =============================================
-- sp_GetAssignmentsByWorkOrder
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetAssignmentsByWorkOrder]
    @WorkOrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.AssignmentId,
        a.Status,
        a.AgreedAmount,
        a.AssignedAt,
        a.RespondedAt,
        a.TechnicianUserId,
        u.FirstName + ' ' + u.LastName AS TechnicianName,
        tp.Zone,
        tp.HourlyRate,
        tp.AvailabilityStatus,
        a.WorkOrderId,
        wo.Title,
        wo.Category,
        wo.LocationText,
        wo.Urgency,
        wo.BudgetAmount
    FROM WorkOrderAssignment a
    INNER JOIN TechnicianProfile tp ON tp.UserId = a.TechnicianUserId
    INNER JOIN Users             u  ON u.UserId  = a.TechnicianUserId
    INNER JOIN WorkOrder         wo ON wo.WorkOrderId = a.WorkOrderId
    WHERE a.WorkOrderId = @WorkOrderId;
END
GO

-- =============================================
-- sp_GetAssignmentsByTechnician
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetAssignmentsByTechnician]
    @TechnicianUserId INT,
    @Status           NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.AssignmentId,
        a.Status,
        a.AgreedAmount,
        a.AssignedAt,
        a.RespondedAt,
        a.TechnicianUserId,
        u.FirstName + ' ' + u.LastName AS TechnicianName,
        tp.Zone,
        tp.HourlyRate,
        tp.AvailabilityStatus,
        a.WorkOrderId,
        wo.Title,
        wo.Category,
        wo.LocationText,
        wo.Urgency,
        wo.BudgetAmount
    FROM WorkOrderAssignment a
    INNER JOIN TechnicianProfile tp ON tp.UserId     = a.TechnicianUserId
    INNER JOIN Users             u  ON u.UserId      = a.TechnicianUserId
    INNER JOIN WorkOrder         wo ON wo.WorkOrderId = a.WorkOrderId
    WHERE a.TechnicianUserId = @TechnicianUserId
      AND (@Status IS NULL OR a.Status = @Status)
    ORDER BY a.AssignedAt DESC;
END
GO

-- =============================================
-- sp_CheckIn
-- =============================================
CREATE PROCEDURE [dbo].[sp_CheckIn]
    @WorkOrderId      INT,
    @TechnicianUserId INT,
    @Notes            NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM WorkOrder WHERE WorkOrderId = @WorkOrderId AND Status = 'IN_PROGRESS')
    BEGIN
        RAISERROR('WORKORDER_NOT_IN_PROGRESS', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM WorkOrderCheckIn
               WHERE WorkOrderId = @WorkOrderId AND TechnicianUserId = @TechnicianUserId
               AND CheckOutAt IS NULL)
    BEGIN
        RAISERROR('CHECKIN_ALREADY_OPEN', 16, 1);
        RETURN;
    END

    INSERT INTO WorkOrderCheckIn (WorkOrderId, TechnicianUserId, CheckInAt, Notes)
    VALUES (@WorkOrderId, @TechnicianUserId, SYSDATETIME(), @Notes);

    SELECT SCOPE_IDENTITY() AS CheckInId;
END
GO

-- =============================================
-- sp_CheckOut
-- =============================================
CREATE PROCEDURE [dbo].[sp_CheckOut]
    @CheckInId INT,
    @Notes     NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM WorkOrderCheckIn WHERE CheckInId = @CheckInId AND CheckOutAt IS NULL)
    BEGIN
        RAISERROR('CHECKIN_NOT_OPEN', 16, 1);
        RETURN;
    END

    UPDATE WorkOrderCheckIn
    SET
        CheckOutAt      = SYSDATETIME(),
        Notes           = ISNULL(@Notes, Notes),
        DurationMinutes = DATEDIFF(MINUTE, CheckInAt, SYSDATETIME())
    WHERE CheckInId = @CheckInId;

    SELECT 'OK' AS Result;
END
GO

-- =============================================
-- sp_GetCheckInsByWorkOrder
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetCheckInsByWorkOrder]
    @WorkOrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ci.CheckInId,
        ci.CheckInAt,
        ci.CheckOutAt,
        ci.DurationMinutes,
        ci.Notes,
        u.FirstName + ' ' + u.LastName AS TechnicianName
    FROM WorkOrderCheckIn ci
    INNER JOIN Users u ON u.UserId = ci.TechnicianUserId
    WHERE ci.WorkOrderId = @WorkOrderId
    ORDER BY ci.CheckInAt DESC;
END
GO

-- =============================================
-- sp_SendMessage
-- =============================================
CREATE PROCEDURE [dbo].[sp_SendMessage]
    @WorkOrderId  INT,
    @SenderUserId INT,
    @Body         NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM WorkOrder WHERE WorkOrderId = @WorkOrderId)
    BEGIN
        RAISERROR('WORKORDER_NOT_FOUND', 16, 1);
        RETURN;
    END

    INSERT INTO WorkOrderMessage (WorkOrderId, SenderUserId, Body)
    VALUES (@WorkOrderId, @SenderUserId, @Body);

    SELECT SCOPE_IDENTITY() AS MessageId;
END
GO

-- =============================================
-- sp_GetMessagesByWorkOrder
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetMessagesByWorkOrder]
    @WorkOrderId INT,
    @PageNum     INT = 1,
    @PageSize    INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.MessageId,
        m.Body,
        m.CreatedAt,
        m.SenderUserId,
        u.FirstName + ' ' + u.LastName AS SenderName,
        u.UserType                     AS SenderType
    FROM WorkOrderMessage m
    INNER JOIN Users u ON u.UserId = m.SenderUserId
    WHERE m.WorkOrderId = @WorkOrderId
    ORDER BY m.CreatedAt ASC
    OFFSET (@PageNum - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- sp_ListTechnicians
-- =============================================
CREATE PROCEDURE [dbo].[sp_ListTechnicians]
    @Zone               NVARCHAR(100) = NULL,
    @AvailabilityStatus NVARCHAR(50)  = NULL,
    @MaxHourlyRate      DECIMAL(10,2) = NULL,
    @PageNum            INT           = 1,
    @PageSize           INT           = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.UserId,
        u.FirstName + ' ' + u.LastName AS FullName,
        u.Email,
        tp.Bio,
        tp.HourlyRate,
        tp.Zone,
        tp.AvailabilityStatus,
        tp.PortfolioUrl,
        (SELECT COUNT(*) FROM WorkOrderAssignment a
         WHERE a.TechnicianUserId = u.UserId AND a.Status = 'COMPLETED') AS CompletedJobs
    FROM TechnicianProfile tp
    INNER JOIN Users u ON u.UserId = tp.UserId
    WHERE u.IsActive = 1
      AND (@Zone               IS NULL OR tp.Zone               = @Zone)
      AND (@AvailabilityStatus IS NULL OR tp.AvailabilityStatus = @AvailabilityStatus)
      AND (@MaxHourlyRate      IS NULL OR tp.HourlyRate        <= @MaxHourlyRate)
    ORDER BY CompletedJobs DESC
    OFFSET (@PageNum - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- sp_GetTechnicianProfile
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetTechnicianProfile]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.UserId,
        u.FirstName,
        u.LastName,
        u.Email,
        u.Phone,
        tp.Bio,
        tp.HourlyRate,
        tp.Zone,
        tp.AvailabilityStatus,
        tp.PortfolioUrl
    FROM TechnicianProfile tp
    INNER JOIN Users u ON u.UserId = tp.UserId
    WHERE tp.UserId = @UserId;
END
GO

-- =============================================
-- sp_UpdateTechnicianProfile
-- =============================================
CREATE PROCEDURE [dbo].[sp_UpdateTechnicianProfile]
    @UserId             INT,
    @Bio                NVARCHAR(MAX) = NULL,
    @HourlyRate         DECIMAL(10,2) = NULL,
    @Zone               NVARCHAR(100) = NULL,
    @AvailabilityStatus NVARCHAR(50)  = NULL,
    @PortfolioUrl       NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TechnicianProfile
    SET
        Bio                = ISNULL(@Bio,                Bio),
        HourlyRate         = ISNULL(@HourlyRate,         HourlyRate),
        Zone               = ISNULL(@Zone,               Zone),
        AvailabilityStatus = ISNULL(@AvailabilityStatus, AvailabilityStatus),
        PortfolioUrl       = ISNULL(@PortfolioUrl,       PortfolioUrl)
    WHERE UserId = @UserId;

    SELECT 'OK' AS Result;
END
GO

-- =============================================
-- sp_GetClientProfile
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetClientProfile]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.UserId,
        u.FirstName,
        u.LastName,
        u.Email,
        u.Phone,
        cp.ClientType,
        cp.DisplayName,
        cp.ContactName,
        cp.ContactPhone,
        cp.LocationText
    FROM ClientProfile cp
    INNER JOIN Users u ON u.UserId = cp.UserId
    WHERE cp.UserId = @UserId;
END
GO

-- =============================================
-- sp_UpdateClientProfile
-- =============================================
CREATE PROCEDURE [dbo].[sp_UpdateClientProfile]
    @UserId       INT,
    @DisplayName  NVARCHAR(150) = NULL,
    @ContactName  NVARCHAR(150) = NULL,
    @ContactPhone NVARCHAR(20)  = NULL,
    @LocationText NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ClientProfile
    SET
        DisplayName  = ISNULL(@DisplayName,  DisplayName),
        ContactName  = ISNULL(@ContactName,  ContactName),
        ContactPhone = ISNULL(@ContactPhone, ContactPhone),
        LocationText = ISNULL(@LocationText, LocationText)
    WHERE UserId = @UserId;

    SELECT 'OK' AS Result;
END
GO

-- =============================================
-- sp_DeactivateUser
-- =============================================
CREATE PROCEDURE [dbo].[sp_DeactivateUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET IsActive = 0 WHERE UserId = @UserId;
    SELECT 'OK' AS Result;
END
GO

USE [master]
GO
ALTER DATABASE [FieldTech] SET READ_WRITE
GO

USE [FieldTech]
GO

-- =============================================
-- SP: sp_ValidarCorreo
-- Valida que un correo exista y esté activo.
-- Retorna datos básicos del usuario.
-- Usado por RecuperarAcceso.
-- =============================================
IF OBJECT_ID('dbo.sp_ValidarCorreo', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ValidarCorreo;
GO

CREATE PROCEDURE [dbo].[sp_ValidarCorreo]
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        Email,
        FirstName,
        LastName,
        UserType
    FROM Users
    WHERE Email    = @Email
      AND IsActive = 1;
END
GO

-- =========================================================
-- TABLA: WorkOrderNote
-- Esta tabla almacena las notas o comentarios asociados
-- a una orden de trabajo.
-- Cada nota pertenece a una orden específica y registra
-- qué usuario la escribió, el contenido y la fecha.
-- =========================================================
CREATE TABLE [dbo].[WorkOrderNote] (
    [NoteId]         INT IDENTITY(1,1) NOT NULL, -- Identificador único de la nota
    [WorkOrderId]    INT               NOT NULL, -- Id de la orden de trabajo a la que pertenece la nota
    [UserId]         INT               NOT NULL, -- Id del usuario que escribió la nota
    [NoteText]       NVARCHAR(1000)    NOT NULL, -- Texto o contenido de la nota
    [CreatedAt]      DATETIME2(7)      NOT NULL CONSTRAINT DF_WorkOrderNote_CreatedAt DEFAULT (SYSDATETIME()), -- Fecha y hora de creación automática
    CONSTRAINT PK_WorkOrderNote PRIMARY KEY CLUSTERED ([NoteId] ASC), -- Llave primaria
    CONSTRAINT FK_WorkOrderNote_Order FOREIGN KEY ([WorkOrderId]) REFERENCES [dbo].[WorkOrder]([WorkOrderId]), -- Relación con la orden de trabajo
    CONSTRAINT FK_WorkOrderNote_User FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) -- Relación con el usuario autor de la nota
);
GO

-- =========================================================
-- PROCEDIMIENTO: sp_AddWorkOrderNote
-- Este procedimiento agrega una nueva nota a una orden de trabajo.
-- Valida que el texto de la nota no esté vacío.
-- Si la inserción se realiza correctamente, devuelve el Id de la nota creada.
-- =========================================================
CREATE PROCEDURE [dbo].[sp_AddWorkOrderNote]
    @WorkOrderId INT,
    @UserId INT,
    @NoteText NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validación para evitar notas vacías o con solo espacios
    IF LTRIM(RTRIM(ISNULL(@NoteText, ''))) = ''
    BEGIN
        RAISERROR('EMPTY_NOTE', 16, 1);
        RETURN;
    END

    -- Inserta la nueva nota en la tabla
    INSERT INTO WorkOrderNote(WorkOrderId, UserId, NoteText)
    VALUES(@WorkOrderId, @UserId, @NoteText);

    -- Devuelve el Id de la nota recién creada
    SELECT SCOPE_IDENTITY() AS NoteId;
END
GO

-- =========================================================
-- PROCEDIMIENTO: sp_GetWorkOrderNotes
-- Este procedimiento obtiene todas las notas de una orden de trabajo.
-- También muestra el nombre completo del autor de cada nota.
-- Los resultados se ordenan de la más reciente a la más antigua.
-- =========================================================
CREATE PROCEDURE [dbo].[sp_GetWorkOrderNotes]
    @WorkOrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        n.NoteId,
        n.WorkOrderId,
        n.UserId,
        n.NoteText,
        n.CreatedAt,
        u.FirstName + ' ' + u.LastName AS AuthorName
    FROM WorkOrderNote n
    INNER JOIN Users u ON u.UserId = n.UserId
    WHERE n.WorkOrderId = @WorkOrderId
    ORDER BY n.CreatedAt DESC;
END
GO

-- =========================================================
-- TABLA: WorkOrderCalendarEvent
-- Esta tabla almacena eventos de calendario relacionados
-- con órdenes de trabajo o eventos creados manualmente por un usuario.
-- Permite registrar título, fecha de inicio, fecha de fin
-- y una descripción opcional.
-- =========================================================
CREATE TABLE [dbo].[WorkOrderCalendarEvent] (
    [EventId]        INT IDENTITY(1,1) NOT NULL, -- Identificador único del evento
    [WorkOrderId]    INT               NULL, -- Orden de trabajo asociada al evento (puede ser nulo)
    [CreatedByUserId] INT              NOT NULL, -- Usuario que creó el evento
    [Title]          NVARCHAR(150)     NOT NULL, -- Título del evento
    [StartAt]        DATETIME2(7)      NOT NULL, -- Fecha y hora de inicio
    [EndAt]          DATETIME2(7)      NOT NULL, -- Fecha y hora de finalización
    [Description]    NVARCHAR(500)     NULL, -- Descripción opcional del evento
    CONSTRAINT PK_WorkOrderCalendarEvent PRIMARY KEY CLUSTERED ([EventId] ASC), -- Llave primaria
    CONSTRAINT FK_WorkOrderCalendarEvent_Order FOREIGN KEY ([WorkOrderId]) REFERENCES [dbo].[WorkOrder]([WorkOrderId]), -- Relación opcional con la orden
    CONSTRAINT FK_WorkOrderCalendarEvent_User FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[Users]([UserId]) -- Relación con el usuario creador
);
GO

-- =========================================================
-- PROCEDIMIENTO: sp_CreateCalendarEvent
-- Este procedimiento crea un evento en el calendario.
-- Puede estar asociado o no a una orden de trabajo.
-- Valida que la fecha final sea mayor que la fecha inicial.
-- Devuelve el Id del evento creado.
-- =========================================================
CREATE PROCEDURE [dbo].[sp_CreateCalendarEvent]
    @WorkOrderId INT = NULL,
    @CreatedByUserId INT,
    @Title NVARCHAR(150),
    @StartAt DATETIME2(7),
    @EndAt DATETIME2(7),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Validación del rango de fechas
    IF @EndAt <= @StartAt
    BEGIN
        RAISERROR('INVALID_DATE_RANGE', 16, 1);
        RETURN;
    END

    -- Inserta el evento en la tabla
    INSERT INTO WorkOrderCalendarEvent(WorkOrderId, CreatedByUserId, Title, StartAt, EndAt, Description)
    VALUES(@WorkOrderId, @CreatedByUserId, @Title, @StartAt, @EndAt, @Description);

    -- Devuelve el Id del evento creado
    SELECT SCOPE_IDENTITY() AS EventId;
END
GO

-- =========================================================
-- PROCEDIMIENTO: sp_GetCalendarEvents
-- Este procedimiento obtiene los eventos de calendario
-- creados por un usuario dentro de un rango de fechas.
-- Solo devuelve los eventos que se traslapan con el rango indicado.
-- =========================================================
CREATE PROCEDURE [dbo].[sp_GetCalendarEvents]
    @UserId INT,
    @StartDate DATETIME2(7),
    @EndDate DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EventId,
        e.WorkOrderId,
        e.Title,
        e.StartAt,
        e.EndAt,
        e.Description
    FROM WorkOrderCalendarEvent e
    WHERE e.CreatedByUserId = @UserId
      AND e.StartAt < @EndDate
      AND e.EndAt > @StartDate
    ORDER BY e.StartAt ASC;
END
GO

USE FieldTech;
GO

-- =========================================================
-- TABLA: WorkOrderHistory
-- Esta tabla guarda el historial de acciones realizadas
-- sobre una orden de trabajo.
-- Permite auditar quién hizo una acción, qué tipo de acción fue,
-- un detalle opcional y cuándo ocurrió.
-- =========================================================
CREATE TABLE [dbo].[WorkOrderHistory] (
    [HistoryId]      INT IDENTITY(1,1) NOT NULL, -- Identificador único del historial
    [WorkOrderId]    INT               NOT NULL, -- Orden de trabajo sobre la cual se realizó la acción
    [UserId]         INT               NOT NULL, -- Usuario que realizó la acción
    [ActionType]     NVARCHAR(50)      NOT NULL, -- Tipo de acción realizada (ejemplo: creación, asignación, cierre)
    [ActionDetail]   NVARCHAR(500)     NULL, -- Detalle adicional de la acción
    [CreatedAt]      DATETIME2(7)      NOT NULL CONSTRAINT DF_WorkOrderHistory_CreatedAt DEFAULT (SYSDATETIME()), -- Fecha y hora automática
    CONSTRAINT PK_WorkOrderHistory PRIMARY KEY CLUSTERED ([HistoryId] ASC), -- Llave primaria
    CONSTRAINT FK_WorkOrderHistory_Order FOREIGN KEY ([WorkOrderId]) REFERENCES [dbo].[WorkOrder]([WorkOrderId]), -- Relación con la orden de trabajo
    CONSTRAINT FK_WorkOrderHistory_User FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) -- Relación con el usuario
);
GO

-- =========================================================
-- PROCEDIMIENTO: sp_AddWorkOrderHistory
-- Este procedimiento registra una nueva acción en el historial
-- de una orden de trabajo.
-- Se usa para guardar eventos importantes como cambios de estado,
-- asignaciones, cierres, etc.
-- Devuelve el Id del registro creado.
-- =========================================================
CREATE PROCEDURE [dbo].[sp_AddWorkOrderHistory]
    @WorkOrderId INT,
    @UserId INT,
    @ActionType NVARCHAR(50),
    @ActionDetail NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Inserta un nuevo registro en el historial
    INSERT INTO WorkOrderHistory(WorkOrderId, UserId, ActionType, ActionDetail)
    VALUES(@WorkOrderId, @UserId, @ActionType, @ActionDetail);

    -- Devuelve el Id del historial creado
    SELECT SCOPE_IDENTITY() AS HistoryId;
END
GO

-- =========================================================
-- PROCEDIMIENTO: sp_GetWorkOrderHistory
-- Este procedimiento obtiene el historial completo de una orden de trabajo.
-- También muestra el nombre del usuario que realizó cada acción.
-- Los resultados se ordenan desde la acción más reciente.
-- =========================================================
CREATE PROCEDURE [dbo].[sp_GetWorkOrderHistory]
    @WorkOrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        h.HistoryId,
        h.WorkOrderId,
        h.UserId,
        h.ActionType,
        h.ActionDetail,
        h.CreatedAt,
        u.FirstName + ' ' + u.LastName AS UserName
    FROM WorkOrderHistory h
    INNER JOIN Users u ON u.UserId = h.UserId
    WHERE h.WorkOrderId = @WorkOrderId
    ORDER BY h.CreatedAt DESC;
END
GO

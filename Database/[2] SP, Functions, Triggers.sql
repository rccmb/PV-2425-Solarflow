DROP FUNCTION IF EXISTS fn_HashPassword;
GO
CREATE FUNCTION fn_HashPassword
(
    @Password NVARCHAR(128),
    @Salt NVARCHAR(128)
)
RETURNS NVARCHAR(64)
AS
BEGIN
    RETURN CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @Password + @Salt), 2)
END;
GO


DROP PROCEDURE IF EXISTS sp_AddUserAccount;
GO
CREATE PROCEDURE sp_AddUserAccount
    @Name NVARCHAR(255),
    @Email NVARCHAR(255),
    @Password NVARCHAR(128),
    @Photo NVARCHAR(255) = NULL,
    @BatteryAPI NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @UserID INT;
        DECLARE @Salt NVARCHAR(128) = NEWID();
        DECLARE @HashedPassword NVARCHAR(64);

        -- Check if the email is already registered
        IF EXISTS (SELECT 1 FROM UserAccount WHERE email = @Email)
        BEGIN
            PRINT '[Account Creation] Error. The email is already registered.';
            RETURN;
        END

        -- Hash the provided password with the generated salt
        SET @HashedPassword = dbo.fn_HashPassword(@Password, @Salt);

        -- Insert the user into the UserAccount table with the real hashed password
        INSERT INTO UserAccount (name, email, photo, salt, hashed_password, view_account_status, battery_api, created_at)
        VALUES (@Name, @Email, @Photo, @Salt, @HashedPassword, 0, @BatteryAPI, GETDATE());

        PRINT '[Account Creation] UserAccount successfully created.';
    END TRY
    BEGIN CATCH
        RAISERROR ('An error occurred while executing [sp_AddUserAccount].', 18, 1);
    END CATCH
END;
GO


DROP PROCEDURE IF EXISTS sp_NewPassword;
GO
CREATE PROCEDURE sp_NewPassword
    @UserID INT,
    @NewPassword NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @Salt NVARCHAR(128) = NEWID(); 

        -- Check if the user exists
        IF EXISTS (SELECT 1 FROM UserAccount WHERE user_id = @UserID)
        BEGIN
            -- Update the password and salt
            UPDATE UserAccount
            SET hashed_password = dbo.fn_HashPassword(@NewPassword, @Salt),
                salt = @Salt
            WHERE user_id = @UserID;

            PRINT '[Password Update] Password successfully changed.';
        END
        ELSE
        BEGIN
            PRINT '[Password Update] Error. User not found.';
        END
    END TRY
    BEGIN CATCH
        RAISERROR ('An error occurred while executing [sp_NewPassword].', 18, 1);
    END CATCH
END;
GO


DROP PROCEDURE IF EXISTS sp_UpdateUserAccount;
GO
CREATE PROCEDURE sp_UpdateUserAccount
    @UserID INT,
    @NewName NVARCHAR(255) = NULL,
    @NewPhoto NVARCHAR(255) = NULL,
    @NewEmail NVARCHAR(255) = NULL,
    @NewBatteryAPI NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Check if the user exists
        IF NOT EXISTS (SELECT 1 FROM UserAccount WHERE user_id = @UserID)
        BEGIN
            PRINT '[User Update] Error. User not found.';
            RETURN;
        END

        -- Check if the new email is already in use by another user
        IF @NewEmail IS NOT NULL AND EXISTS (SELECT 1 FROM UserAccount WHERE email = @NewEmail AND user_id <> @UserID)
        BEGIN
            PRINT '[User Update] Error. Email is already in use by another account.';
            RETURN;
        END

        -- Update only fields that are provided (email check is already done)
        UPDATE UserAccount
        SET name = COALESCE(@NewName, name),
            photo = COALESCE(@NewPhoto, photo),
            email = COALESCE(@NewEmail, email),
            battery_api = COALESCE(@NewBatteryAPI, battery_api)
        WHERE user_id = @UserID;

        PRINT '[User Update] UserAccount successfully updated.';
    END TRY
    BEGIN CATCH
        RAISERROR ('An error occurred while executing [sp_UpdateUserAccount].', 18, 1);
    END CATCH
END;
GO


DROP PROCEDURE IF EXISTS sp_AddViewAccount;
GO
CREATE PROCEDURE sp_AddViewAccount
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @UserName NVARCHAR(255);
        DECLARE @RandomNumber NVARCHAR(6);
        DECLARE @LoginName NVARCHAR(255);
        DECLARE @GeneratedPassword NVARCHAR(12);
        DECLARE @HashedPassword NVARCHAR(255);
        DECLARE @Salt NVARCHAR(128) = NEWID(); -- Generate a new salt

        -- Check if the UserAccount exists
        IF NOT EXISTS (SELECT 1 FROM UserAccount WHERE user_id = @UserID)
        BEGIN
            PRINT '[ViewAccount Creation] Error. UserAccount not found.';
            RETURN;
        END

        -- Get the user's name
        SELECT @UserName = name FROM UserAccount WHERE user_id = @UserID;

        -- Generate a random number (6 digits)
        SET @RandomNumber = RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000 AS NVARCHAR(6)), 6);

        -- Generate the Login_Name (UserName + 6 random digits)
        SET @LoginName = @UserName + @RandomNumber;

        -- Generate a random 12-character password
        SET @GeneratedPassword = 
            SUBSTRING(CONVERT(NVARCHAR(255), NEWID()), 1, 4) +
            SUBSTRING(CONVERT(NVARCHAR(255), NEWID()), 1, 4) +
            SUBSTRING(CONVERT(NVARCHAR(255), NEWID()), 1, 4);

        -- Hash the password before storing
        SET @HashedPassword = dbo.fn_HashPassword(@GeneratedPassword, @Salt);

        -- Insert into ViewAccount
        INSERT INTO ViewAccount (user_id, login_name, hashed_password)
        VALUES (@UserID, @LoginName, @HashedPassword);

        PRINT '[ViewAccount Creation] ViewAccount successfully created.';
        PRINT '[ViewAccount Info] Login_Name: ' + @LoginName;
        PRINT '[ViewAccount Info] Generated Password: ' + @GeneratedPassword;
    END TRY
    BEGIN CATCH
        RAISERROR ('An error occurred while executing [sp_AddViewAccount].', 18, 1);
    END CATCH
END;
GO

DROP TRIGGER IF EXISTS trg_CreateViewAccount;
GO
CREATE TRIGGER trg_CreateViewAccount
ON UserAccount
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UserID INT;

    -- Select users where viewAccount_status changed from 0 to 1
    SELECT @UserID = inserted.user_id
    FROM inserted
    INNER JOIN deleted ON inserted.user_id = deleted.user_id
    WHERE deleted.view_account_status = 0 AND inserted.view_account_status = 1;

    -- If a user was found, execute sp_AddViewAccount
    IF @UserID IS NOT NULL
    BEGIN
        PRINT '[Trigger] Creating ViewAccount for UserID: ' + CAST(@UserID AS NVARCHAR(10));
        EXEC sp_AddViewAccount @UserID;
    END
END;
GO


DROP TRIGGER IF EXISTS trg_DeleteViewAccount;
GO
CREATE TRIGGER trg_DeleteViewAccount
ON UserAccount
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UserID INT;

    -- Check if the status changed from 1 to 0
    SELECT @UserID = deleted.user_id
    FROM inserted
    INNER JOIN deleted ON inserted.user_id = deleted.user_id
    WHERE deleted.view_account_status = 1 AND inserted.view_account_status = 0;

    -- If found, delete the corresponding ViewAccount
    IF @UserID IS NOT NULL
    BEGIN
        PRINT '[Trigger] Deleting ViewAccount for UserID: ' + CAST(@UserID AS NVARCHAR(10));
        DELETE FROM ViewAccount WHERE user_id = @UserID;
    END
END;
GO


DROP PROCEDURE IF EXISTS sp_AuthenticateAccount;
GO
CREATE PROCEDURE sp_AuthenticateAccount
    @Identifier NVARCHAR(255), 
    @Password NVARCHAR(128),
    @AuthResult INT OUTPUT  -- Add an output parameter
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @UserID INT, @StoredHash NVARCHAR(64), @Salt NVARCHAR(128);
        DECLARE @AccountType NVARCHAR(20);

        -- Check if the user exists in UserAccount or ViewAccount
        IF EXISTS (SELECT 1 FROM UserAccount WHERE email = @Identifier)
        BEGIN
            SET @AccountType = 'User';
            SELECT @UserID = user_id, @StoredHash = hashed_password, @Salt = salt
            FROM UserAccount WHERE email = @Identifier;
        END
        ELSE IF EXISTS (SELECT 1 FROM ViewAccount WHERE login_name = @Identifier)
        BEGIN
            SET @AccountType = 'View';
            SELECT @UserID = user_id, @StoredHash = hashed_password
            FROM ViewAccount WHERE login_name = @Identifier;
        END
        ELSE
        BEGIN
            SET @AuthResult = 0;  -- User not found
            RETURN;
        END

        -- Verify password
        IF dbo.fn_HashPassword(@Password, @Salt) = @StoredHash
        BEGIN
            SET @AuthResult = 1;  -- Authentication successful
        END
        ELSE
        BEGIN
            SET @AuthResult = 0;  -- Incorrect password
        END
    END TRY
    BEGIN CATCH
        RAISERROR ('An error occurred while executing [sp_AuthenticateAccount].', 18, 1);
        SET @AuthResult = 0;  -- Error case
    END CATCH
END;
GO

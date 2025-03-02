-- Delete the existing test user.
DECLARE @ExistingUserID INT;

SELECT @ExistingUserID = user_id FROM UserAccount WHERE email = 'johndoe@example.com';

IF @ExistingUserID IS NOT NULL
BEGIN
    DELETE FROM UserAccount WHERE user_id = @ExistingUserID;
    PRINT '[Test Setup] Existing user deleted.';
END

-- Add a new test user. Should be successful.
EXEC sp_AddUserAccount 
    @Name = 'John Doe', 
    @Email = 'johndoe@example.com', 
	@Password = 'PASSWORD!',
    @Photo = NULL, 
    @BatteryAPI = NULL;

-- Try to add a new test user. Should throw error.
EXEC sp_AddUserAccount 
    @Name = 'John Doe', 
    @Email = 'johndoe@example.com', 
	@Password = 'PASSWORD!',
    @Photo = NULL, 
    @BatteryAPI = NULL;

-- Try to authenticate as test user. Should be successful.
EXEC sp_AuthenticateAccount 
    @Identifier = 'johndoe@example.com', 
    @Password = 'PASSWORD!';

-- Try to authenticate as test user. Should throw error.
EXEC sp_AuthenticateAccount 
    @Identifier = 'johndoe@example.com', 
    @Password = 'WRONGPASSWORD!';


-- Change password for an existing user
EXEC sp_NewPassword 
    @UserID = 1, 
    @NewPassword = 'NewSecurePassword!';

-- Try to change password for a non-existing user (should fail)
EXEC sp_NewPassword 
    @UserID = 999,  -- Non-existent user
    @NewPassword = 'NewPassword123!';

-- Update name and email for an existing user
EXEC sp_UpdateUserAccount 
    @UserID = 1, 
    @NewName = 'John Updated',
    @NewEmail = 'john123@cenas.com';

-- First, add another user
EXEC sp_AddUserAccount 
    @Name = 'Maria Doe', 
    @Email = 'maria@example.com',
    @Password = 'password';

-- Try to update John's email to Jane's email (should fail)
EXEC sp_UpdateUserAccount 
    @UserID = 1, 
    @NewEmail = 'maria@example.com'; -- Duplicate email

-- Enable ViewAccount for an existing user
UPDATE UserAccount 
SET view_account_status = 1 
WHERE user_id = 1;

-- Disable ViewAccount for an existing user
UPDATE UserAccount 
SET view_account_status = 0 
WHERE user_id = 1;

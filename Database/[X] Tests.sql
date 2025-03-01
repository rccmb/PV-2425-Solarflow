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
CREATE DATABASE SolarFlow;
GO

USE SolarFlow;
GO

CREATE TABLE UserAccount (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    photo NVARCHAR(255),
    email NVARCHAR(255) UNIQUE NOT NULL,
    salt NVARCHAR(255) NOT NULL,
    hashed_password NVARCHAR(255) NOT NULL,
    view_account_status BIT DEFAULT 0,
    battery_api NVARCHAR(255),
    created_at DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE ViewAccount (
    view_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    login_name NVARCHAR(255) UNIQUE NOT NULL,
    salt NVARCHAR(255) NOT NULL,
    hashed_password NVARCHAR(255) NOT NULL,
    CONSTRAINT fk_view_account_user FOREIGN KEY (user_id) 
    REFERENCES UserAccount(user_id) ON DELETE CASCADE
);
GO
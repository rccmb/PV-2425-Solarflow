
CREATE DATABASE SolarFlow;



USE SolarFlow;

CREATE TABLE UserAccount (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    photo NVARCHAR(255),
    email NVARCHAR(255) UNIQUE NOT NULL,
    Salt NVARCHAR(255) NOT NULL,
    hashedPassword NVARCHAR(255) NOT NULL,
    viewAccount_status BIT DEFAULT 0,
    battery_api NVARCHAR(255),
    created_at DATETIME DEFAULT GETDATE()
);


CREATE TABLE ViewAccount (
    view_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    login_Name NVARCHAR(255) UNIQUE NOT NULL,
    salt NVARCHAR(255) NOT NULL,
    hashedPassword NVARCHAR(255) NOT NULL,
    CONSTRAINT FK_ViewAccount_User FOREIGN KEY (user_id) 
    REFERENCES UserAccount(user_id) ON DELETE CASCADE
);


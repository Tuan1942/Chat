Project Overview:  
A chat website, allowing the sending of multimedia files. It leverages ASP.NET Core for the backend, Entity Framework Core for data access, and FFmpeg for media processing tasks including compression and metadata management.  
  
User Authentication:  
User registration, login, and logout functionalities secured with JWT tokens.  
  
Media Management:  
Upload, compress, and retrieve images, audio, and video files.  
Store original and compressed versions of files.  
Generate unique filenames to avoid conflicts.  
  
Message System:  
Record and store messages related to media uploads.  
Messages include type, value, and timestamp for tracking events.  
  
Technologies Used:  
Backend: ASP.NET Core  
Database: SQL Server with Entity Framework Core  
Media Processing: FFmpeg  
Authentication: JWT Tokens  
Development Tools: Visual Studio 2022  
  
API Endpoints:  
  User Management:  
  /user/register: Register a new user.  
  /user/login: Authenticate a user and generate a token.  
  /user/logout: Logout a user.  
  /user/current: Get current logged in user information.  
  /user/list: Get all user in database.  
  
  Image Management:  
  /image/upload: Upload an image, compress it, and store both versions.  
  /image/{id}: Retrieve an image by ID.  
    
  Video Management:  
  /video/upload: Upload a video, compress it, and store both versions.  
  /video/{fileName}: Retrieve a video by filename.  
    
  Audio Management:  
  /audio/upload: Upload an audio file, compress it, and store both versions.  
  /audio/{id}: Retrieve an audio file by ID.  
    
  Message Management:  
  /message/send: Create a message related to media uploads.  
  /message/{id}: Retrieve a message by ID.  
  
FFmpeg used: https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-full.7z  

Database Configuration:
Update appsettings.json with your SQL Server connection string.  
Rebuild solution  
Run following command at Nuget Package Manager Console:  
dotnet ef database update --context "MultimediaContext"  
dotnet ef database update --context "UserContext"  

Demo video:  https://drive.google.com/file/d/1-2b8oeDvOnLlQntrNaQmenXF-WMdfibZ/view?usp=sharing

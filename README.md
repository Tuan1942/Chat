Project Overview:  
A chat website, allowing the sending of multimedia files. It leverages ASP.NET Core for the backend, Entity Framework Core for data access, and FFmpeg for media processing tasks including compression and metadata management.  
  
Demo video:  
  Website: https://drive.google.com/file/d/1-2b8oeDvOnLlQntrNaQmenXF-WMdfibZ/view?usp=sharing  
  Android: https://drive.google.com/file/d/1tm-Gsk6pKMrl1XU6O2rIcQtsAp6XKB78/view?usp=sharing  
  
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
  ![image](https://github.com/Tuan1942/Chat/assets/148052417/95766d4e-a742-4ca9-b9d6-913619645556)
  Rebuild solution  
  Run following command at Nuget Package Manager Console:  
  dotnet ef database update --context "MultimediaContext"  
  dotnet ef database update --context "UserContext"  

MAUI Configuration:  
  replace IP in \Chat\Resources\xml\network_security_config.xml and \Chat\ViewModel\Connection.cs with your server IP then rebuild  
  ![image](https://github.com/Tuan1942/Chat/assets/148052417/dc86bdbb-6d0e-436d-9a80-bae934728ba0)
  ![image](https://github.com/Tuan1942/Chat/assets/148052417/9c4ed209-d352-4dce-ae67-42c7f2302e9b)
  ![image](https://github.com/Tuan1942/Chat/assets/148052417/799b8d68-4236-4d31-a1bb-c15f106d66a5)


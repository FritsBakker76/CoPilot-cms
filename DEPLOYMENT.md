# SmarterASP.net Deployment Guide

## 📦 Deployment Package Ready
Your application has been published to: `C:\Frits\CmsModern\publish\`

## 🔧 Pre-Deployment Steps

### 1. Get MySQL Database Credentials from SmarterASP.net
Log into your SmarterASP.net control panel and:
- Navigate to **Database Manager** → **MySQL**
- Note down:
  - Server address (e.g., `mysql8001.site4now.net`)
  - Database name (e.g., `DB_xxxxx_cmsmodern`)
  - Username (e.g., `DB_xxxxx_cmsmodern_admin`)
  - Password

### 2. Update Production Configuration
Edit `publish\appsettings.Production.json` with your MySQL credentials:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_MYSQL_SERVER;Port=3306;Database=YOUR_DATABASE;Uid=YOUR_USERNAME;Pwd=YOUR_PASSWORD"
  }
}
```

## 🚀 Upload to SmarterASP.net

### Method 1: FTP Upload (Recommended)
1. Open your FTP client (FileZilla, WinSCP, or Windows Explorer)
2. Connect using credentials from SmarterASP.net control panel:
   - Host: `ftp.yourdomain.com` or FTP server provided
   - Username: Your hosting username
   - Password: Your hosting password
   
3. Navigate to your website root directory (usually `/wwwroot` or `/httpdocs`)

4. Upload ALL files from `C:\Frits\CmsModern\publish\` to the root directory

### Method 2: File Manager
1. Log into SmarterASP.net control panel
2. Go to **File Manager**
3. Navigate to your site root
4. Upload all files from `publish\` folder

## ⚙️ SmarterASP.net Configuration

### 1. Change Application Pool to .NET Core ⚠️ CRITICAL
In SmarterASP.net control panel:
1. Go to **IIS Manager** or **Pool Manager**
2. Click **"Change to .Net Core"**
3. The pool must be set to **.NET Core**, NOT "ASP.NET 4.X Integrated"

**Your app will NOT work on ASP.NET 4.X!** Make sure it says ".NET Core" after the change.

### 2. Set Environment Variable
In Pool Manager or control panel:
- Click **"Environment Variables"** in the Pool Manager menu
- Add: `ASPNETCORE_ENVIRONMENT` = `Production`

### 3. Enable 64-bit (if not already)
In Pool Manager:
- Click **"Change to 64-bit"** (should be default for .NET Core)

## 🗃️ Database Initialization

The database tables will be **automatically created** on first startup thanks to your Startup.cs logic!

1. After uploading files, browse to your website
2. The app will create all tables (pages, users, pagecontent, websitesettings)
3. Default admin account will be created: `admin` / `admin`

## ✅ Post-Deployment Checklist

- [ ] Website loads without errors
- [ ] Login at `/login` with admin/admin works
- [ ] Access admin panel at `/admin`
- [ ] Check that pages display correctly
- [ ] Test uploading images to verify file permissions
- [ ] **Change admin password** in production!

## 🐛 Troubleshooting

### "500 Internal Server Error"
- Check `ASPNETCORE_ENVIRONMENT` is set to `Production`
- Verify MySQL connection string is correct
- Enable detailed errors temporarily in Startup.cs (already done in Development mode)

### Database Connection Issues
- Verify MySQL server address (may include port in format `server:3306`)
- Check if database user has all privileges
- Ensure database exists (created in SmarterASP.net panel)

### File Upload Errors
- Check folder permissions for `wwwroot/uploads/banners/` and `wwwroot/uploads/content/`
- These folders should exist and be writable

### Can't Access /admin
- Clear browser cookies
- Try logging in again at `/login`

## 📝 Important Notes

- Your local `appsettings.json` has development credentials - never upload this with production passwords
- `appsettings.Production.json` overrides settings when ASPNETCORE_ENVIRONMENT=Production
- Admin password resets to "admin" on every app restart (see Startup.cs line 247)
- For security, consider removing the password reset in production after initial setup

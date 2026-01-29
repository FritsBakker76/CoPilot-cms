# CmsModern - AI Agent Instructions

## Project Overview
ASP.NET Core CMS system with MySQL backend for managing pages, content blocks, and website settings. Built with Entity Framework Core, Bootstrap UI, and cookie authentication. Language: Dutch (documentation, comments, README).

**Hosting**: Designed for deployment on SmarterASP.net hosting platform.

## Tech Stack
- **Framework**: ASP.NET Core (targeting net10.0 - optimized for SmarterASP.net)
- **Database**: MySQL with Pomelo EF Core provider
- **Authentication**: Cookie-based with BCrypt password hashing
- **UI**: Razor Views + Bootstrap

## Architecture

### Data Flow
1. **Startup.cs** handles seeding & migrations at runtime (lines 72-257) - manually creates tables/columns if missing via raw SQL
2. **CmsDbContext** defines entities but Startup.cs owns schema initialization
3. Controllers use direct DbContext injection (no repository pattern)

### Key Components
- **Pages**: Main content entities with SEO fields (GoogleTitle, GoogleDescription), menu items, banner images
- **PageContent**: Modular content blocks positioned on pages using template system (see Content Templates below)
- **WebsiteSettings**: Global theme configuration (colors, fonts, footer content) - single row expected, injected in _Layout.cshtml
- **Users**: Admin accounts with BCrypt hashes

### Content Templates (Alinea Types)
PageContent uses predefined layout templates identified by `Type` property. Available templates in [Views/Home/PageTemplates.cshtml](Views/Home/PageTemplates.cshtml):
- **alinea#1**: Full-width with title + text
- **alinea#2**: Title + text with image slideshow on right
- **alinea#3**: Title + text with image slideshow on left
- **alinea#4**: Full-width text only (no title)
- **alinea#5**: Text with image slideshow on right (no title)
- **alinea#6**: Text with image slideshow on left (no title)

Rendering logic for each template is in [Views/Home/Page.cshtml](Views/Home/Page.cshtml) (lines 127-240 for view mode, 265+ for edit mode).

## Critical Patterns

### Database Schema Management
- **NO migrations folder** - schema changes go in Startup.cs Configure()
- Add columns via ALTER TABLE checks (see lines 115-180 in Startup.cs for pattern)
- Always check column existence before ALTER to prevent crashes
- Use raw SQL via `context.Database.ExecuteSqlRaw()`

### Model Conventions
- Use `[Column("snake_case")]` attributes to map C# PascalCase to MySQL snake_case
- Example: `[Column("menu_item")]` for MenuItem property
- See [Models/Page.cs](Models/Page.cs) for reference

### Routing
Custom route patterns in Startup.cs:
- `/login` → AccountController.Login
- `/admin/*` → PagesController actions
- Default: `/{controller=Home}/{action=Page}/{id=1}` (shows page 1 by default)

### File Uploads
- **Banners**: `wwwroot/uploads/banners/`
- **Content images**: `wwwroot/uploads/content/`
- Use IWebHostEnvironment.WebRootPath for physical paths
- Store relative paths in database (e.g., `/uploads/banners/image.jpg`)

## Development Workflows

### Running Locally
```powershell
dotnet run
```
Navigates to Home/Page/1 by default. Admin at `/admin` (requires login).

### Database Setup
1. Configure connection string in [appsettings.json](appsettings.json)
2. Run app - Startup.cs auto-creates schema and seeds admin user (username/password: admin/admin)
3. For fresh start: drop database and restart (manual schema via database.sql is optional)

### Authentication Testing
- Default admin credentials: `admin` / `admin` (reset on every startup - see Startup.cs line 242)
- Controllers use `[Authorize]` attribute
- Admin-specific actions check `User.Claims` for `IsAdmin = "True"`

## Common Tasks

### Adding New Page Fields
1. Add property to [Models/Page.cs](Models/Page.cs) with `[Column("db_name")]`
2. In Startup.cs Configure(), add ALTER TABLE check before seeding (follow menu_item pattern)
3. Update [Views/Pages/Edit.cshtml](Views/Pages/Edit.cshtml) form fields

### New PageContent Template (Alinea Type)
1. Add new template card to [Views/Home/PageTemplates.cshtml](Views/Home/PageTemplates.cshtml) with `type="alinea#N"`
2. Add rendering logic in [Views/Home/Page.cshtml](Views/Home/Page.cshtml):
   - View mode: lines 127-240 (`@if (content.Type == "alinea#N")`)
   - Edit mode: lines 265+ (editing form for template)
3. Update CSS in [wwwroot/css/site.css](wwwroot/css/site.css) if new layout requires custom styling
4. No model changes needed - Type is a string field

### Styling Changes
- Global styles: [wwwroot/css/site.css](wwwroot/css/site.css)
- Dynamic theme: WebsiteSettings entity controls colors/fonts injected via DbContext in [Views/Shared/_Layout.cshtml](Views/Shared/_Layout.cshtml)
  - **Colors**: HeaderBg, HeaderTextColor, MenuBg, MenuTextColor, SiteBg, SiteTextColor, FooterBg, FooterTextColor
  - **Fonts**: FontPageTitle, FontAlineaTitle, FontWebsiteText, FontSlideshowFooter (in pixels)
  - **Logo**: LogoPath displayed in navbar, falls back to SiteTitle text
  - Applied inline via `style="background-color: @headerBg"` throughout layout
- Edit theme settings at `/admin/Settings` (admin only)

## Pitfalls to Avoid
- **Don't use EF migrations** - schema logic is in Startup.cs raw SQL
- **Connection string has credentials** - don't commit real passwords to appsettings.json
- HTTPS/HSTS disabled by default (line 45 in Startup.cs) - enable for production
- PageContent.Created uses MySQL CURRENT_TIMESTAMP, but C# DateTime.Now in seed data
- When adding alinea templates, must update BOTH view mode and edit mode sections in Page.cshtml

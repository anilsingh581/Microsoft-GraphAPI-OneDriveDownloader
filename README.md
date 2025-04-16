# What is Microsoft Graph API?
Microsoft Graph API is a unified REST API provided by Microsoft that lets you access Microsoft 365 services like:

# 📧 Outlook Mail & Calendar

👤 Users & Groups
🗂️ OneDrive & SharePoint Files
📝 Microsoft Teams

# ✅ To-Do, Tasks, Planner, and more
It’s the gateway to data and intelligence in Microsoft 365.

# 📁 What is OneDrive Downloader using Graph API?
A OneDrive Downloader is an app that:
Connects to a Microsoft account or organization
Accesses the OneDrive files and folders
Downloads files programmatically

This is done through Microsoft Graph's Files API.

# 🛠️ Key Graph API Endpoints for OneDrive
🔹 Get the root drive
http
GET /me/drive

🔹 List files in a folder
http
GET /me/drive/root/children
GET /me/drive/root:/Documents:/children

🔹 Download a file
http
GET /me/drive/items/{item-id}/content

Or by path:
http
GET /me/drive/root:/Documents/file.txt:/content
This returns the raw file stream, which you can save locally.

# 🧩 Authentication Required
Graph API uses Azure Active Directory (AAD) for auth.

You can authenticate using:
Authorization Code Flow (for user login)
Client Credentials Flow (for app-only access)
Requires application registration in Azure Portal

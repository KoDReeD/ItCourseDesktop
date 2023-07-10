using Avalonia.Controls;
using ItCourseDesktop.ViewModels;

namespace ItCourseDesktop.Data;

public static class DataManager
{
    public static AuthResponse User { get; set; } = new AuthResponse();
    public static Window mainWindow { get; set; }
    
    // public static string Username { get; set; }
    // public static string Token { get; set; }
    // public static string RefreshToken { get; set; }
    // public static int RoleId { get; set; }
    // public static int UserId { get; set; }

    // public static string HostUrl { get; set; } = "https://localhost:7188";
    public static string HostUrl { get; set; } = "https://workhost.bsite.net";

    // public static void SetUser(string username, string token, string refreshToken, int roleId, int userId)
    // {
    //     Username = username;
    //     Token = token;
    //     RefreshToken = token;
    //     RoleId = roleId;
    //     UserId = userId;
    // }
}
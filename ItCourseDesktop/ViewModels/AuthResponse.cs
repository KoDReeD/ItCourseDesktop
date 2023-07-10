namespace ItCourseDesktop.ViewModels;

public class AuthResponse
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public int RoleId { get; set; }
}
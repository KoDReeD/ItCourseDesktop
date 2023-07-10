using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using ItCourseDesktop.ViewModels;
using ItCourseDesktop.Windows;

namespace ItCourseDesktop.Data;

public static class AuthorizeManager
{
    public static async Task<bool> RefreshRequest(Window contextWindow)
    {
        bool flag = false;
        
        using (HttpClient client = new HttpClient())
        {
            var json = new
            {
                Username = DataManager.User.Username,
                RefreshToken = DataManager.User.RefreshToken
            };

            string refreshUrl = $"{DataManager.HostUrl}/Account/Refresh";
            HttpResponseMessage responseRefresh = await client.PostAsJsonAsync(refreshUrl, json);
            try
            {
                var value = responseRefresh.Content.ReadAsStringAsync();
                if (responseRefresh.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AuthorizeWindow authorizeWindow = new AuthorizeWindow();
                    authorizeWindow.Show();
                    DataManager.User = new AuthResponse();
                    contextWindow.Close();
                    flag = false;
                }
                else if (responseRefresh.StatusCode == HttpStatusCode.BadRequest)
                {
                    AuthorizeWindow authorizeWindow = new AuthorizeWindow();
                    authorizeWindow.Show();
                    DataManager.User = new AuthResponse();
                    contextWindow.Close();
                    flag = false;
                }
                else if (responseRefresh.StatusCode == HttpStatusCode.OK)
                {
                    string newToken = responseRefresh.Content.ReadAsStringAsync().Result;
                    DataManager.User.Token = newToken;
                    flag = true;
                }
            }
            catch(Exception ex)
            {
                flag = false;
            }
            
        }

        return flag;

    }
}
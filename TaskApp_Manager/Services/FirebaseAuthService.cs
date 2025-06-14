using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace TaskManagerApp.Services
{
    public class FirebaseAuthService
    {
        private readonly string _apiKey = "AIzaSyDgD5SUiPrLjJ5d6xa4NHKx7zXecqmiM3U"; 
        private readonly HttpClient _httpClient;

        public FirebaseAuthService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<AuthResult> SignInWithGoogleAsync(string idToken)
        {
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("id_token", idToken),
                    new KeyValuePair<string, string>("requestUri", "http://localhost:8080/oauth-callback")
                });
                var response = await _httpClient.PostAsync("https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Log.Error("Ошибка SignInWithGoogle: {0}", errorContent);
                    throw new Exception($"Ошибка авторизации через Google: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AuthResult>(responseContent);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка в SignInWithGoogleAsync");
                throw;
            }
        }

        public async Task<AuthResult> SignInWithEmailAsync(string email, string password)
        {
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("email", email),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("returnSecureToken", "true")
                });

                var response = await _httpClient.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Log.Error("Ошибка SignInWithEmail: {0}", errorContent);
                    throw new Exception($"Ошибка входа: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AuthResult>(responseContent);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка в SignInWithEmailAsync");
                throw;
            }
        }

        public async Task<AuthResult> SignUpWithEmailAsync(string email, string password)
        {
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("email", email),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("returnSecureToken", "true")
                });

                var response = await _httpClient.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Log.Error("Ошибка SignUpWithEmail: {0}", errorContent);
                    throw new Exception($"Ошибка регистрации: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AuthResult>(responseContent);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка в SignUpWithEmailAsync");
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("email", email),
                    new KeyValuePair<string, string>("requestType", "PASSWORD_RESET")
                });

                var response = await _httpClient.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Log.Error("Ошибка SendPasswordResetEmail: {0}", errorContent);
                    throw new Exception($"Ошибка отправки письма: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка в SendPasswordResetEmailAsync");
                throw;
            }
        }

        public class AuthResult
        {
            public string IdToken { get; set; }
            public string RefreshToken { get; set; }
            public string UserId { get; set; }
        }
    }
}
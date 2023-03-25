
using Microsoft.IdentityModel.Tokens;
using SupportBot.Service.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SupportBot.Service
{
    //The first crutch version service of Api
    class SupportService
    {
        private string api = "https://localhost:7165/api/";
        private HttpClient client = new HttpClient();
        public Token token = new Token();

        public SupportService(string url)
        {
            api = url;
        }

        public async Task<AuthResponse?> SendAuthModelAsync(string username, string password)
        {
            var user = new AuthEmployerDto
            {
                Username = username,
                Password = password
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(user));
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync(api + "auth/login", requestContent);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            token = ValidateToken(result.Tokens.AccessToken);

            return result;
        }

        public async Task SendUserModelAsync(string name, string chat_id)
        {
            var user = new UserDto
            {
                Name = name,
                TelegramId = chat_id
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(user));
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");


            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.TokenApi);
            
            var response = await client.PostAsync(api + "users/create", requestContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Не удалось получить ответ от сервера. Код ответа: {response.StatusCode}");
            }
        }

        public async Task<User?> GetUserByTelegramIdAsync(string chat_id)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.TokenApi);

            var response = await client.GetAsync(api + $"tickets/{chat_id}/get");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<User>();

            return user;
        }


        public async Task<List<Ticket>> GetAllOpenTicketsModelAsync()
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.TokenApi);

            var response = await client.GetAsync(api + "tickets/all-open-tickets");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Не удалось получить ответ от сервера. Код ответа: {response.StatusCode}");
            }

            var tickets = await response.Content.ReadFromJsonAsync<List<Ticket>>();

            return tickets;
        }

        private Token? ValidateToken(string token)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "idk",
                ValidAudience = "idk",
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("aPdSgUkXp2s5v8y/"))
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var user = handler.ValidateToken(token, validationParameters, out var validatedToken);

                return new Token
                {
                    TokenApi = token,
                    TimeOfToken = DateTime.UtcNow,
                };
            }
            catch (SecurityTokenException e)
            {
                Console.WriteLine($"Ошибка проверки токена: {e.Message}");
                return null;
            }
        }

    }
}

namespace SupportBot.Service.Models
{
    public class UserDto
    {
        public string Name { get; set; }
        public string TelegramId { get; set; }
    }

    public class AuthEmployerDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public int Rating { get; set; }
        public Guid? TicketId { get; set; }
        public Tokens Tokens { get; set; }
    }

    public class Tokens
    {
        public string AccessToken { get; set; }
    }

    public class Token
    {
        public string TokenApi { get; set; }
        public DateTime TimeOfToken { get; set; }
    }

    public class Message
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Guid SenderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Text { get; set; }
        public User Sender { get; set; }
        public Ticket Ticket { get; set; }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string Role { get; set; }
        public string? TelegramId { get; set; }
        public Guid? TicketId { get; set; }
        public int Rating { get; set; }
    }

    public class Ticket
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public string Status { get; set; }
        public ICollection<Message> Messages { get; set; }
    }

    public class MessageInformation
    {
        public string Message { get; set; }
    }
}
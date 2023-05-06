using Microsoft.IdentityModel.Tokens;
using SupportBot.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace SPIAPI
{
    class SupportService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public Token token = new Token();

        public SupportService(HttpClient httpClient, string apiUrl)
        {
            _httpClient = httpClient;
            _apiUrl = apiUrl;
        }

        //api/auth/login | return AuthResponse
        public async Task<AuthResponse?> LoginAsync(string username, string password)
        {
            var user = new AuthEmployerDto
            {
                Username = username,
                Password = password
            };

            var requestContent = JsonSerializer.Serialize(user);

            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl + "auth/login")
            {
                Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (result.Success)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var authResponse = JsonSerializer.Deserialize<AuthResponse>(result.Data.ToString(), options);

                token = ValidateToken(authResponse.Tokens.AccessToken);

                return authResponse;
            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                throw new Exception(JsonSerializer.Deserialize<ErrorMessage>(result.Data.ToString(), options).Error);
            }
        }

        //api/tickets/all-open-tickets
        public async Task<IEnumerable<Ticket>?> GetOpenTicketsAsync()
        {

            if (token.TokenApi == "")
            {
                throw new Exception("Your token is not installed");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, _apiUrl + "tickets/all-open-tickets")
            {
                Content = new StringContent("application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.TokenApi);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //fix
                throw new Exception("You are not authorized");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (result.Success)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var ticketsResponse = JsonSerializer.Deserialize<IEnumerable<Ticket>>(result.Data.ToString(), options);

                return ticketsResponse;

            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                throw new Exception(JsonSerializer.Deserialize<ErrorMessage>(result.Data.ToString(), options).Error);
            }
        }

        //api/users/create
        public async Task<SuccessfulMessage?> CreateTelegramUserAsync(string name, string telegramid)
        {
            if (token.TokenApi == "")
            {
                throw new Exception("Your token is not installed");
            }

            var user = new UserDto
            {
                Name = name,
                TelegramId = telegramid
            };

            var requestContent = JsonSerializer.Serialize(user);

            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl + "users/create")
            {
                Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.TokenApi);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new Exception("You are not authorized");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (result.Success)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var createdResponse = JsonSerializer.Deserialize<SuccessfulMessage>(result.Data.ToString(), options);
                return createdResponse;
            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return null;
                //throw new Exception(JsonSerializer.Deserialize<ErrorMessage>(result.Data.ToString(), options).Error);
            }
        }

        //api/users/{id}/get
        public async Task<User?> GetUserByTelegramIdAsync(string telegramId)
        {

            if (token.TokenApi == "")
            {
                throw new Exception("Your token is not installed");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, _apiUrl + $"users/{telegramId}/get")
            {
                Content = new StringContent("application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.TokenApi);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //fix
                throw new Exception("You are not authorized");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (result.Success)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var user = JsonSerializer.Deserialize<User>(result.Data.ToString(), options);

                return user;

            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                //throw new Exception(JsonSerializer.Deserialize<ErrorMessage>(result.Data.ToString(), options).Error);
                return null;
            }
        }

        //api/tickets/{id}/get
        public async Task<Ticket?> GetTicketByIdAsync(Guid ticketId)
        {
            if (token.TokenApi == "")
            {
                throw new Exception("Your token is not installed");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, _apiUrl + $"tickets/{ticketId}/get")
            {
                Content = new StringContent("application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.TokenApi);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //fix
                throw new Exception("You are not authorized");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (result.Success)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var ticket = JsonSerializer.Deserialize<Ticket>(result.Data.ToString(), options);

                return ticket;

            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                //throw new Exception(JsonSerializer.Deserialize<ErrorMessage>(result.Data.ToString(), options).Error);

                return null;
            }
        }

        //api/tickets/{id}/messages
        public async Task<SuccessfulMessage?> SendMessageToTicketAsync(Guid ticketId, Guid senderId, string text)
        {
            var message = new MessageDto
            {
                SenderId = senderId,
                Text = text
            };

            if (token.TokenApi == "")
            {
                throw new Exception("Your token is not installed");
            }

            var requestContent = JsonSerializer.Serialize(message);

            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl + $"tickets/{ticketId}/messages")
            {
                Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.TokenApi);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //fix
                throw new Exception("You are not authorized");
            }
            //problem
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (result.Success)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<SuccessfulMessage>(result.Data.ToString(), options);
            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                //throw new Exception(JsonSerializer.Deserialize<ErrorMessage>(result.Data.ToString(), options).Error);
                return null;
            }
        }

        //api/tickets/create
        public async Task<SuccessfulMessage?> CreateTicket(Guid userId, string title, string description)
        {
            var ticket = new TicketDto
            {
                UserId= userId,
                Title = title,
                Description = description
            };

            if (token.TokenApi == "")
            {
                throw new Exception("Your token is not installed");
            }

            var requestContent = JsonSerializer.Serialize(ticket);

            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl + "tickets/create")
            {
                Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.TokenApi);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //fix
                throw new Exception("You are not authorized");
            }
            //problem
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (result.Success)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<SuccessfulMessage>(result.Data.ToString(), options);
            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                //throw new Exception(JsonSerializer.Deserialize<ErrorMessage>(result.Data.ToString(), options).Error);

                return null;
            }
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
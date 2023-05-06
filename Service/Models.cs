using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportBot.Service
{
    public class SuccessfulMessage
    {
        public string Message { get; set; }
    }

    public class ErrorMessage
    {
        public string Error { get; set; }
    }
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
    }

    public class AuthEmployerDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserDto
    {
        public string Name { get; set; }
        public string TelegramId { get; set; }
    }

    public class TicketDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
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

        public class MessageDto
        {
            public Guid SenderId { get; set; }
            public string Text { get; set; }
        }

        public class Tokens
        {
            public string AccessToken { get; set; }
        }

        public class Token
        {
            public string? TokenApi { get; set; }
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
}

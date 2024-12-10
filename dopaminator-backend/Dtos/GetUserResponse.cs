using Dopaminator.Models;

namespace Dopaminator.Dtos
{
    public class GetUserResponse
    {
        public required string Username { get; set; }
        public required Guid Id {get; set;}
        public ICollection<PostResponse> Posts { get; set; }
    }
}

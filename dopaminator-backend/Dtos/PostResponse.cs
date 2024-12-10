namespace Dopaminator.Dtos
{
    public class PostResponse
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required byte[] Content { get; set; }
        public required float Price {get; set;}
        public required bool IsOwned {get; set;}
    }
}

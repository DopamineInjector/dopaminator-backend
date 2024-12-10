namespace dopaminator_backend.Dtos
{
    public class CreatePostRequest
    {
        public required string Title { get; set; }
        public required byte[] Content { get; set; }
        public required float Price {get; set;}
    }
}

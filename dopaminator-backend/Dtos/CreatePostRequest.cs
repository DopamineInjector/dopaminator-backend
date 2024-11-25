namespace dopaminator_backend.Dtos
{
    public class CreatePostRequest
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
    }
}

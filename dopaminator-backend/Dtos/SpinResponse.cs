namespace Dopaminator.Dtos
{
    public class SpinResponse
    {
        public required bool isWin { get; set; }
        public byte[]? Image { get; set; }

        public string? Name { get; set; }
    }
}

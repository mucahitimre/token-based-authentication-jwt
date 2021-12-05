namespace JwtTokenize.Models.Dtos
{
    public class ProductResponse : ResponseDto
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public string ProductCode { get; set; }
    }
}

namespace JwtTokenize.Models.Dtos
{
    public class ResponseDto
    {
        public ResponseDto()
        {
        }

        public ResponseDto(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; set; } = true;
    }
}

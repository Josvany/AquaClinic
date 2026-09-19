namespace AquaClinic.App.Services.Base
{
    public class BaseDataService
    {
        protected IClient _client;

        public BaseDataService(IClient client)
        {
            _client = client;
        }

        protected ApiResponse<Guid> ConvertApiExceptions<Guid>(ApiException ex)
        {
            if (ex.StatusCode == 400)
            {
                return new ApiResponse<Guid>() { Message = "Ocurrieron errores de validaci&#243;n.", ValidationErrors = ex.Response, Success = false };
            }
            else if (ex.StatusCode == 404)
            {
                return new ApiResponse<Guid>() { Message = "No se encontr&#243; el elemento solicitado.", Success = false };
            }
            else
            {
                return new ApiResponse<Guid>() { Message = "Ocurri&#243; un error, intente nuevamente.", Success = false };
            }
        }
    }
}

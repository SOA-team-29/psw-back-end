using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Explorer.API.Controllers;

[ApiController]
public class BaseApiController : ControllerBase
{
    protected ActionResult CreateErrorResponse(List<IError> errors)
    {
        var code = 500;
        if (ContainsErrorCode(errors, 400)) code = 400;
        if (ContainsErrorCode(errors, 403)) code = 403;
        if (ContainsErrorCode(errors, 404)) code = 404;
        if (ContainsErrorCode(errors, 409)) code = 409;
        return CreateErrorObject(errors, code);
    }

    private static bool ContainsErrorCode(List<IError> errors, int code)
    {
        return errors.Any(e =>
        {
            e.Metadata.TryGetValue("code", out var errorCode);
            if (errorCode == null) return false;
            return (int)errorCode == code;
        });
    }

    private ObjectResult CreateErrorObject(List<IError> errors, int code)
    {
        var sb = new StringBuilder();
        foreach (var error in errors)
        {
            sb.Append(error);
            error.Metadata.TryGetValue("subCode", out var subCode);
            if(subCode != null)
            {
                sb.Append(';');
                sb.Append(subCode);
            }

            sb.AppendLine();
        }
        return Problem(statusCode: code, detail: sb.ToString());
    }

    protected ActionResult CreateResponse(Result result)
    {
        return result.IsSuccess ? Ok() : CreateErrorResponse(result.Errors);
    }

    protected ActionResult CreateResponse<T>(Result<T> result)
    {
        return result.IsSuccess ? Ok(result.Value) : CreateErrorResponse(result.Errors);
    }

[HttpDelete("{baseEncounterId:int}")]
public async Task<ActionResult> DeleteEncounter(int baseEncounterId)
{
    var baseEncounterResponse = await DeleteEncounterAsync(baseEncounterId);
    if (baseEncounterResponse.IsSuccessStatusCode || baseEncounterResponse.StatusCode == HttpStatusCode.NoContent)
    {
        var socialEncounterIdResponse = await GetSocialEncounterIdAsync(baseEncounterId);

        //ovo sam radila jer bi mi bio potreban dodatni dto 
        //citamo odgovor kao json string
        string jsonResponse1 = await socialEncounterIdResponse.Content.ReadAsStringAsync();
        //json string konvertujemo u json objekat
        JObject jsonObject1 = JObject.Parse(jsonResponse1);
        //odavde (iz json objekta) izvlacimo vrednost polja socialEncounterId 
        int socialEncounterId = (int)jsonObject1["socialEncounterId"];

        var hiddenLocationEncounterIdResponse = await GetHiddenLocationEncounterIdAsync(baseEncounterId);

        string jsonResponse2 = await hiddenLocationEncounterIdResponse.Content.ReadAsStringAsync();
        JObject jsonObject2 = JObject.Parse(jsonResponse2);
        int hiddenLocationEncounterId = (int)jsonObject2["hiddenLocationEncounterId"];

        if (socialEncounterId != -1)
        {
            var socialEncounterResponse = await DeleteSocialEncounterAsync(socialEncounterId);
            return CreateResponse(socialEncounterResponse);
        }
        else if (hiddenLocationEncounterId != -1)
        {
            var hiddenLocationEncounterResponse = await DeleteHiddenLocationEncounterAsync(hiddenLocationEncounterId);
            return CreateResponse(hiddenLocationEncounterResponse);
        }
        return CreateResponse(baseEncounterResponse);
    }

}
}
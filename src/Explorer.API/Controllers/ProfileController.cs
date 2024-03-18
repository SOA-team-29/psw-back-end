using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.UseCases;
using Explorer.Tours.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Explorer.Stakeholders.Core.Domain;
using FluentResults;
using Newtonsoft.Json;
using System.Text;

namespace Explorer.API.Controllers;

//[Authorize(Policy = "touristPolicy")]
//[Authorize(Policy = "authorPolicy")]
[Route("api/profile")]
public class ProfileController : BaseApiController
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("{userId}")]
    /*public ActionResult<UserProfileDto> Get([FromRoute] int userId)
    {
        var result = _profileService.Get(userId);
        return CreateResponse(result);
    }*/
    public async Task<ActionResult<UserProfileDto>> Get([FromRoute] int userId)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:8081/");

        try
        {
            var response = await httpClient.GetAsync("people/get/" + userId);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Result<UserProfileDto> profile = JsonConvert.DeserializeObject<UserProfileDto>(content);
                return CreateResponse(profile);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve accounts from the other app.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while communicating with the other app: " + ex.Message);
        }

    }

    [HttpPut("{id:int}")]
    /*public ActionResult<UserProfileDto> Update([FromBody] UserProfileDto profile)
    {
        var result = _profileService.Update(profile);
        return CreateResponse(result);
    }*/
    public async Task<ActionResult<UserProfileDto>> Update([FromBody] UserProfileDto profile)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:8081/");

        try
        {
            var jsonContent = JsonConvert.SerializeObject(profile);
            var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync("people/update", stringContent);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Result<UserProfileDto> userProfile = JsonConvert.DeserializeObject<UserProfileDto>(content);
                return CreateResponse(userProfile);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve accounts from the other app.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while communicating with the other app: " + ex.Message);
        }

    }
}


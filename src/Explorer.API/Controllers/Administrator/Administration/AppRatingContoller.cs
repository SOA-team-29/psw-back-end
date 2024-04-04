using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
/*
namespace Explorer.API.Controllers.Administrator.Administration
{
    [Route("api/administration/app-ratings")] 
    public class AppRatingController : BaseApiController
    {
        private readonly IAppRatingService _appRatingService;

        public AppRatingController(IAppRatingService appRatingService)
        {
            _appRatingService = appRatingService;
        }

        [HttpGet]
        [Authorize(Policy = "administratorPolicy")]
        public ActionResult<PagedResult<AppRatingDto>> GetAll([FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _appRatingService.GetPaged(page, pageSize);
            return CreateResponse(result);
        }

        [HttpPost]
        [Authorize(Policy = "touristAuthorPolicy")]
        //[Authorize(Policy = "touristPolicy, authorPolicy")]
        // [Authorize(Policy = "touristPolicy")]
        //[Authorize(Policy = "authorPolicy")]

        public ActionResult<AppRatingDto> Create([FromBody] AppRatingDto appRating)
        {
            bool userAlreadyRated = _appRatingService.HasUserRated(appRating.UserId);
            if (userAlreadyRated) { return BadRequest("User has already rated the app."); }

            var result = _appRatingService.Create(appRating);
            return CreateResponse(result);
        }
    }

}
*/

namespace Explorer.API.Controllers.Administrator.Administration
{
    [Route("api/administration/app-ratings")]
    public class AppRatingController : BaseApiController
    {
        private readonly IAppRatingService _appRatingService;

        public AppRatingController(IAppRatingService appRatingService)
        {
            _appRatingService = appRatingService;
        }

        [HttpGet]
       // [Authorize(Policy = "administratorPolicy")]
        public async Task<List<AppRatingDto>> GetAll()
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://host.docker.internal:8083/");

            try
            {
                var response = await httpClient.GetAsync("ratings/getAll");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    List<AppRatingDto> ratings = JsonConvert.DeserializeObject<List<AppRatingDto>>(content);
                    return ratings;
                }
                else
                {
                    return null;//return StatusCode((int)response.StatusCode, "Failed to retrieve ratings from the other app.");
                }
            }
            catch (Exception ex)
            {
                return null;//return StatusCode(500, "An error occurred while communicating with the other app: " + ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = "touristAuthorPolicy")]
        public async Task<ActionResult<AppRatingDto>> Create([FromBody] AppRatingDto appRating)
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://host.docker.internal:8083/");

            try
            {
                var json = JsonConvert.SerializeObject(appRating);
                var accountjson = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("ratings/create", accountjson);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Result<AuthenticationTokensDto> authToken = JsonConvert.DeserializeObject<AuthenticationTokensDto>(content);
                    return CreateResponse(authToken);
                }
                else
                {
                    return StatusCode((int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while communicating with the other app while registration: " + ex.Message);
            }
            /* bool userAlreadyRated = _appRatingService.HasUserRated(appRating.UserId);
             if (userAlreadyRated) { return BadRequest("User has already rated the app."); }

             var result = _appRatingService.Create(appRating);
             return CreateResponse(result);*/
        }
    }

}
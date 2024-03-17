using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Administration;
using Explorer.Tours.Core.UseCases.Administration;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers.Author.Administration
{

   // [Authorize(Policy = "authorAndAdminPolicy")]

    [Route("api/administration/tourPoint")] 
    public class TourPointController : BaseApiController
    {
        private readonly ITourPointService _tourPointService;

        public TourPointController(ITourPointService tourPointService)
        {
            _tourPointService = tourPointService;
        }
        [Authorize(Policy = "authorPolicy")]
        [HttpGet]
        public ActionResult<PagedResult<TourPointDto>> GetAll([FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _tourPointService.GetPaged(page, pageSize);
            return CreateResponse(result);
        }
        
        
        
       
        [HttpPost]
        public async Task<ActionResult<TourPointDto>> Create([FromBody] TourPointDto tourPoint)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "http://localhost:8081/tourPoint";



                    var response = await client.PostAsJsonAsync(url, tourPoint);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response from server: " + responseContent);
                        return CreateResponse(Result.Ok(response));
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                        return CreateResponse(Result.Fail("An error occurred"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    return CreateResponse(Result.Fail("An error occurred").WithError(ex.Message));
                }
            }
        }

        [Authorize(Policy = "authorPolicy")]
        [HttpPut("{id:int}")]
        public ActionResult<TourPointDto> Update([FromBody] TourPointDto tourPoint)
        {
            var result = _tourPointService.Update(tourPoint);
            return CreateResponse(result);
        }

        [Authorize(Policy = "authorPolicy")]
        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var result = _tourPointService.Delete(id);
            return CreateResponse(result);
        }


        /*
         [Authorize(Policy = "touristAuthorPolicy")]
        [HttpGet("{tourId:int}")]

		public ActionResult<List<TourPointDto>> GetTourPointsByTourId(int tourId)
		{
			var result = _tourPointService.GetTourPointsByTourId(tourId);
			return CreateResponse(result);
		}*/

        
        [Authorize(Policy = "touristAuthorPolicy")]
        [HttpGet("{tourId:int}")]
        public async Task<ActionResult<List<TourPointDto>>> GetTourPointsByTourId(int tourId)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "http://localhost:8081/tourPoint/allPointsInTour/" + tourId;

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<List<TourPointDto>>();
                        

                        return Ok(responseData);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                        return BadRequest("An error occurred");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    return BadRequest("An error occurred: " + ex.Message);
                }
            }
        }

        [HttpGet("getById/{id:int}")]
        public ActionResult<TourPointDto> GetTourPointById(int id)
        {
            var result = _tourPointService.Get(id);
            return CreateResponse(Result.Ok(result));
        }

    }
}

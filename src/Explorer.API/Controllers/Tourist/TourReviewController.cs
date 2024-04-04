using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Administration;
using Explorer.Tours.Core.UseCases.Administration;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers.Tourist
{
    
    [Route("api/tourist/tourReview")]
    public class TourReviewController : BaseApiController
    {
        private readonly ITourReviewService _tourReviewService;
        public TourReviewController(ITourReviewService tourReviewService)
        {
            _tourReviewService = tourReviewService;
        }

        /*
         [HttpPost]
         public ActionResult<TourReviewDto> Create([FromBody] TourReviewDto tourReviewDto) { 

             var result= _tourReviewService.Create(tourReviewDto);
             return CreateResponse(result);
         }
         */

        [HttpPost]
        public async Task<ActionResult<TourReviewDto>> Create([FromBody] TourReviewDto tourReview)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "http://host.docker.internal:8081/tourReviews/create";



                    var response = await client.PostAsJsonAsync(url, tourReview);

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
        /*
        [HttpGet]
        public ActionResult<PagedResult<TourReviewDto>> GetAll([FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _tourReviewService.GetPaged(page, pageSize);
            return CreateResponse(result);
        }*/


        [HttpGet]
        public async Task<ActionResult<PagedResult<TourReviewDto>>> GetAll([FromQuery] int page, [FromQuery] int pageSize)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "http://host.docker.internal:8081/tourReviews/see" + "?page=" + page + "&pageSize=" + pageSize;

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<List<TourReviewDto>>();
                        var pagedResult = new PagedResult<TourReviewDto>(responseData, responseData.Count);

                        return Ok(pagedResult);
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

        [HttpPut("{id:int}")]
        public ActionResult<TourReviewDto> Update([FromBody] TourReviewDto tourReviewDto)
        {
            var result = _tourReviewService.Update(tourReviewDto);
            return CreateResponse(result);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var result = _tourReviewService.Delete(id);
            return CreateResponse(result);
        }

    }
}

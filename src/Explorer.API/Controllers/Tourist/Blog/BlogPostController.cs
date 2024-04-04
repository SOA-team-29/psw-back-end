using Explorer.Blog.API.Dtos;
using Explorer.Blog.API.Public;
using Explorer.Blog.Core.Domain;
using Explorer.Blog.Core.UseCases;
using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Tours.API.Dtos;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentResults;
using System.Text;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace Explorer.API.Controllers.Tourist.Blog
{
    [Authorize(Policy = "touristPolicy")]
    [Route("api/blog/blogpost")]
    public class BlogPostController : BaseApiController
    {
        private readonly IBlogPostService _blogPostService;
        public BlogPostController(IBlogPostService blogPostService)
        {
            _blogPostService = blogPostService;
        }
        [HttpGet]
        public async Task<ActionResult<PagedResult<BlogPostDto>>> GetAll([FromQuery] int page, [FromQuery] int pageSize)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"http://host.docker.internal:8082/blogs/all?page={page}&pageSize={pageSize}";

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                       
                        var responseData = await response.Content.ReadFromJsonAsync<List<BlogPostDto>>();
                        var pagedResult = new PagedResult<BlogPostDto>(responseData, responseData.Count);

                        return CreateResponse(Result.Ok(pagedResult));
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
        public ActionResult<PagedResult<BlogPostDto>> GetAll([FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _blogPostService.GetAll(page, pageSize);
            return CreateResponse(result);
        }
        */

        [HttpGet("{blogPostId:int}")]
        public ActionResult<BlogPostDto> GetById(int blogPostId)
        {
            var result = _blogPostService.GetById(blogPostId);
            return CreateResponse(result);
        }
        [HttpPost("{blogPostId:int}/comments")]
        public async Task<ActionResult<BlogPostDto>> AddComment(int blogPostId, [FromBody] BlogPostCommentDto blogPostComment)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"http://host.docker.internal:8082/blogs/{blogPostId}/comments";

                    var response = await client.PostAsJsonAsync(url, blogPostComment);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response from server: " + responseContent);
                        return CreateResponse(Result.Ok(responseContent));
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
        [HttpPost("{blogPostid:int}/comments")]
        public ActionResult<BlogPostDto> AddComment(int blogPostid, [FromBody] BlogPostCommentDto blogPostComment)
        {
            var result = _blogPostService.AddComment(blogPostid, blogPostComment);
            return CreateResponse(result);
        }*/
        [HttpPost("{blogPostid:int}/ratings")]
        public ActionResult<BlogPostDto> AddRating(int blogPostid, [FromBody] BlogPostRatingDto blogPostRating)
        {
            var result = _blogPostService.AddRating(blogPostid, blogPostRating);
            return CreateResponse(result);
        }
        /*
        [HttpPost]
        public ActionResult<BlogPostDto> Create([FromBody] BlogPostDto blogPost)
        {
            var result = _blogPostService.Create(blogPost);
            return CreateResponse(result);
        }
        */
        [HttpPut("{blogPostId:int}/comments")]
        public ActionResult<BlogPostDto> UpdateComment(int blogPostId, [FromBody] BlogPostCommentDto editedComment)
        {
            var result = _blogPostService.UpdateComment(blogPostId, editedComment);
            return CreateResponse(result);
        }
        
        [HttpPost]
        public async Task<ActionResult<BlogPostDto>> Create([FromBody] BlogPostDto blog)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "http://host.docker.internal:8082/blogs";



                    var response = await client.PostAsJsonAsync(url, blog);

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

            [HttpPut("{id:int}")]
        public ActionResult<BlogPostDto> Update([FromBody] BlogPostDto blogPost)
        {
            var result = _blogPostService.Update(blogPost);
            return CreateResponse(result);
        }
        /*
        [HttpDelete("{blogPostId:int}/comments/{userId:int}/{creationTime:datetime}")]
        public ActionResult<BlogPostDto> DeleteComment(int blogPostId, int userId, DateTime creationTime)
        {
            var result = _blogPostService.RemoveComment(blogPostId, userId, creationTime);
            return CreateResponse(result);
        }*/

        [HttpDelete("{blogPostId:int}/comments/{userId:int}/{creationTime:datetime}")]
        public async Task<ActionResult<BlogPostDto>> DeleteComment(int blogPostId, int userId, DateTime creationTime)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"http://host.docker.internal:8082/blogs/{blogPostId}/comments/{userId}/{creationTime}";

                    var response = await client.DeleteAsync(url);

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

        [HttpDelete("{blogPostId:int}/ratings/{userId:int}")]
        public ActionResult<BlogPostDto> DeleteRating(int blogPostId, int userId)
        {
            var result = _blogPostService.RemoveRating(blogPostId, userId);
            return CreateResponse(result);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var result = _blogPostService.Delete(id);
            return CreateResponse(result);
        }

    }
}

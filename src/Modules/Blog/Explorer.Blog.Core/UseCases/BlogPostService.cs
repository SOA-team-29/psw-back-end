using AutoMapper;
using Explorer.Blog.API.Dtos;
using Explorer.Blog.API.Public;
using Explorer.Blog.Core.Domain;
using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Stakeholders.API.Internal;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Blog.Core.UseCases
{
    public class BlogPostService : CrudService<BlogPostDto, BlogPost>, IBlogPostService
    {
        private readonly IInternalUserService _internalUserService;
        private readonly IMapper _mapper;
        public BlogPostService(ICrudRepository<BlogPost> crudRepository, IMapper mapper, IInternalUserService internalUserService) : base(crudRepository, mapper)
        {
            _internalUserService = internalUserService;
            _mapper = mapper;
        }

        Result<BlogPostDto> IBlogPostService.GetById(int id)
        {
            try
            {
                var blogPost = CrudRepository.Get(id);
                var userIds = CollectSingleUserIds(blogPost);
                var userIdUsernameDictionary = _internalUserService.GetUsernames(userIds.Select(id => (long)id).ToList());
                var mappedDto = MapSingleUsernames(blogPost, userIdUsernameDictionary);
                return mappedDto;
            }
            catch (ArgumentException e)
            {
                return Result.Fail(FailureCode.InvalidArgument).WithError(e.Message);
            }
        }

        private List<int> CollectSingleUserIds(BlogPost blogPost)
        {
            var userIds = new List<int>();

            // Collect AuthorId
            userIds.Add(blogPost.AuthorId);

            // Collect Comment.UserIds
            userIds.AddRange(blogPost.Comments?.Select(comment => comment.UserId) ?? Enumerable.Empty<int>());

            // Remove duplicates and return the list
            return userIds.Distinct().ToList();
        }

        private List<int> CollectUserIds(PagedResult<BlogPost> blogPosts)
        {
            var userIds = new List<int>();

            // Collect AuthorIds
            userIds.AddRange(blogPosts.Results.Select(blogPost => blogPost.AuthorId));

            // Collect Comment.UserIds
            userIds.AddRange(blogPosts.Results.SelectMany(blogPost => blogPost.Comments?.Select(comment => comment.UserId) ?? Enumerable.Empty<int>()));

            // Remove duplicates and return the list
            return userIds.Distinct().ToList();
        }

        private BlogPostDto MapSingleUsernames(BlogPost blogPost, Dictionary<long, string> userIdUsernameDictionary)
        {
            var blogPostDto = _mapper.Map<BlogPostDto>(blogPost);

            // Map AuthorUsername based on the dictionary
            //blogPostDto.AuthorUsername = userIdUsernameDictionary.GetValueOrDefault(blogPost.AuthorId);

            // Map Username for each comment based on the dictionary
            if (blogPostDto.Comments != null)
            {
                foreach (var commentDto in blogPostDto.Comments)
                {
                    //commentDto.Username = userIdUsernameDictionary.GetValueOrDefault(commentDto.UserId);
                }
            }

            return blogPostDto;
        }

        private PagedResult<BlogPostDto> MapUsernames(PagedResult<BlogPost> blogPosts, Dictionary<long, string> userIdUsernameDictionary)
        {
            var mappedData = new List<BlogPostDto>();

            foreach (var blogPost in blogPosts.Results)
            {
                var blogPostDto = _mapper.Map<BlogPostDto>(blogPost); // Use AutoMapper for the general mapping

                // Map AuthorUsername based on the dictionary
                //blogPostDto.AuthorUsername = userIdUsernameDictionary.GetValueOrDefault(blogPost.AuthorId);

                // Map Username for each comment based on the dictionary
                if (blogPostDto.Comments != null)
                {
                    foreach (var commentDto in blogPostDto.Comments)
                    {
                        //commentDto.Username = userIdUsernameDictionary.GetValueOrDefault(commentDto.UserId);
                    }
                }

                mappedData.Add(blogPostDto);
            }

            // Create a new PagedResult containing the mapped data
            return new PagedResult<BlogPostDto>(mappedData, blogPosts.TotalCount);
        }

        public Result<PagedResult<BlogPostDto>> GetAll(int page, int pageSize)
        {
            try
            {
                var pagedBlogPosts = CrudRepository.GetPaged(page, pageSize);
                var userIds = CollectUserIds(pagedBlogPosts);
                var userIdUsernameDictionary = _internalUserService.GetUsernames(userIds.Select(id => (long)id).ToList());
                var mappedDto = MapUsernames(pagedBlogPosts, userIdUsernameDictionary);
                return mappedDto;
            }
            catch(ArgumentException e)
            {
                return Result.Fail(FailureCode.InvalidArgument).WithError(e.Message);
            }
        }



        public Result<BlogPostDto> AddComment(int blogPostId, BlogPostCommentDto comment)
        {
            try
            {
                // Retrieve the blog post from the repository
                var blogPost = CrudRepository.Get(blogPostId);

                // Map the DTO to the domain entity
                //var commentEntity = MapToDomain(comment);


                // Add the comment to the blog post
                blogPost.AddComment(comment);

                // Update the blog post in the repository
                CrudRepository.Update(blogPost);

                // Map the updated blog post back to DTO
                var updatedBlogPostDto = MapToDto(blogPost);
                return updatedBlogPostDto;
            }
            catch (ArgumentException e)
            {
                return Result.Fail(FailureCode.InvalidArgument).WithError(e.Message);
            }
            
        }
        public Result<BlogPostDto> AddRating(int blogPostId, BlogPostRatingDto rating)
        {
            try
            {
                var blogPost = CrudRepository.Get(blogPostId); 

                blogPost.AddRating(rating);

                CrudRepository.Update(blogPost);

                var updatedBlogPostDto = MapToDto(blogPost);
                return updatedBlogPostDto;
            }
            catch (ArgumentException e)
            {
                return Result.Fail(FailureCode.InvalidArgument).WithError(e.Message);
            }

        }


        public Result<BlogPostDto> RemoveComment(int blogPostId, int userId, DateTime creationTime)
        {
            try
            {
                // Retrieve the blog post from the repository
                var blogPost = CrudRepository.Get(blogPostId);

                // Assuming that RemoveComment method in BlogPost entity handles the removal logic
                blogPost.RemoveComment(userId, creationTime);

                // Update the blog post in the repository
                CrudRepository.Update(blogPost);

                // Map the updated blog post back to DTO
                var updatedBlogPostDto = MapToDto(blogPost);
                return updatedBlogPostDto;
            }
            catch (ArgumentException e)
            {
                return Result.Fail(FailureCode.InvalidArgument).WithError(e.Message);
            }
        }
        public Result<BlogPostDto> RemoveRating(int blogPostId, int userId)
        {
            try
            {
                // Retrieve the blog post from the repository
                var blogPost = CrudRepository.Get(blogPostId);

                // Assuming that RemoveComment method in BlogPost entity handles the removal logic
                blogPost.RemoveRating(userId);

                // Update the blog post in the repository
                CrudRepository.Update(blogPost);

                // Map the updated blog post back to DTO
                var updatedBlogPostDto = MapToDto(blogPost);
                return updatedBlogPostDto;
            }
            catch (ArgumentException e)
            {
                return Result.Fail(FailureCode.InvalidArgument).WithError(e.Message);
            }
        }

        public Result<BlogPostDto> UpdateComment(int blogPostId, BlogPostCommentDto editedComment)
        {
            try
            {
                // Retrieve the blog post from the repository
                var blogPost = CrudRepository.Get(blogPostId);

                // Assuming BlogPost entity has an EditComment method
                blogPost.EditComment(editedComment);

                // Update the blog post in the repository
                CrudRepository.Update(blogPost);

                // Map the updated blog post back to DTO
                var updatedBlogPostDto = MapToDto(blogPost);
                return updatedBlogPostDto;
            }
            catch (ArgumentException e)
            {
                return Result.Fail(FailureCode.InvalidArgument).WithError(e.Message);
            }
        }

        
    }
}

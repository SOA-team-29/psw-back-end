using AutoMapper;
using Explorer.Blog.API.Dtos;
using Explorer.Blog.Core.Domain;

namespace Explorer.Blog.Core.Mappers;

public class BlogProfile : Profile
{ 
    public BlogProfile()
    {
        CreateMap<BlogPostCommentDto, BlogPostComment>().ReverseMap();
        CreateMap<BlogPostRatingDto, BlogPostRating>().ReverseMap();
        CreateMap<BlogPostDto, BlogPost>()
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId))
            .ForMember(dest => dest.TourId, opt => opt.MapFrom(src => src.TourId))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
            .ForMember(dest => dest.ImageURLs, opt => opt.MapFrom(src => src.ImageURLs))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments.Select(commentDto => 
                new BlogPostComment(commentDto.Text, commentDto.UserId, commentDto.CreationTime, commentDto.LastUpdatedTime))))
            .ForMember(dest => dest.Ratings, opt=> opt.MapFrom(src => src.Ratings.Select(ratingDto =>
            new BlogPostRating(ratingDto.UserId, ratingDto.CreationTime, ratingDto.IsPositive))))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse(typeof(BlogPostStatus), src.Status)));

        CreateMap<BlogPost, BlogPostDto>()
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId))
            .ForMember(dest => dest.TourId, opt => opt.MapFrom(src => src.TourId))
            //.ForMember(dest => dest.AuthorUsername, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
            .ForMember(dest => dest.ImageURLs, opt => opt.MapFrom(src => src.ImageURLs))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
            .ForMember(dest => dest.Ratings, opt => opt.MapFrom(src => src.Ratings))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        //--------------------------------------------------------------------------------------------------------

    }
}
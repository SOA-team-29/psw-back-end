using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Explorer.Blog.API.Dtos
{
    public class BlogPostDto
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public int TourId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public List<string>? ImageURLs { get; set; }
        public List<BlogPostCommentDto>? Comments {  get; set; }
        public List<BlogPostRatingDto>? Ratings { get; set; }
        public string Status { get; set; }
    }
}

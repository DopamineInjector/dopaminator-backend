using Dopaminator.Models;
using Dopaminator.Dtos;

namespace Dopaminator.Services {

    public class PostService {
        private readonly AppDbContext _context;

        public PostService(AppDbContext context) {
            this._context = context;
        }

        private bool IsBoughtByUser(Post post, Guid id) {
            return (post.Author.Id.Equals(id))||(post.PurchasedBy.Any(p => p.Equals(id)));
        }

        private Post? GetPostFromDb(Guid postId) {
            return this._context.Posts.First(p => p.Id.Equals(postId));
        }

        public PostResponse? ParsePostForUser(Guid postId, Guid userId) {
            Post? post = GetPostFromDb(postId);
            if(post == null) {
                return null;
            }
            bool isSubscribed = this.IsBoughtByUser(post, userId);
            PostResponse res = new PostResponse {
                Id = post.Id,
                Title = post.Title,
                Content = isSubscribed ? post.ImageData : post.BlurredImageData,
                Price = post.Price,
                IsOwned = isSubscribed
            };
            return res;
        }
    }
}
